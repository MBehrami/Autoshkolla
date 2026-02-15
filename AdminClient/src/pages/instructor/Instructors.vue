<template>
    <div class="instructors-container">
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Instructors</div>
        </div>
        <v-data-table
            :headers="headers"
            :items="items"
            :loading="loading"
            class="elevation-2 instructors-table"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="instructors-toolbar">
                    <div class="instructors-actions-wrap">
                        <div class="instructors-export-group">
                            <v-btn class="instructors-action-btn text-none" variant="outlined" prepend-icon="mdi-file-excel">
                                <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="instructors"
                                    name="instructors.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="instructors-action-btn text-none" variant="outlined" prepend-icon="mdi-file-word"
                                @click.stop="exportWord">Word</v-btn>
                        </div>
                        <div class="instructors-filters-wrap">
                            <v-text-field
                                v-model="searchText"
                                label="Search (First Name, Last Name)"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="instructors-filter-search"
                                @update:model-value="handleSearch"
                            ></v-text-field>
                            <v-dialog v-model="dialog" max-width="1000" scrollable>
                                <template v-slot:activator="{ props: activatorProps }">
                                    <v-btn v-bind="activatorProps" color="primary" variant="elevated"
                                        class="instructors-add-btn text-none" prepend-icon="mdi-plus">
                                        Add Instructor
                                    </v-btn>
                                </template>
                                <InstructorForm
                                    v-model="dialog"
                                    :instructor="editedInstructor"
                                    :instructor-id="editedInstructorId"
                                    :is-edit="editedIndex > -1"
                                    @saved="handleSaved"
                                />
                            </v-dialog>
                        </div>
                    </div>
                </v-toolbar>
            </template>
            <template v-slot:item.isActive="{ item }">
                <v-chip :color="item.isActive ? 'success' : 'default'" size="small" variant="flat">
                    {{ item.isActive ? 'Active' : 'Inactive' }}
                </v-chip>
            </template>
            <template v-slot:item.actions="{ item }">
                <div class="d-flex align-center flex-wrap ga-2">
                    <v-btn size="small" variant="tonal" color="primary" class="text-capitalize" density="comfortable"
                        prepend-icon="mdi-eye" @click.stop="viewItem(item)">
                        View
                    </v-btn>
                    <v-btn size="small" variant="tonal" color="secondary" class="text-capitalize" density="comfortable"
                        prepend-icon="mdi-pencil" @click.stop="editItem(item)">
                        Edit
                    </v-btn>
                    <v-tooltip :text="item.isActive ? 'Set Inactive' : 'Set Active'">
                        <template v-slot:activator="{ props: tipProps }">
                            <v-btn v-bind="tipProps" size="small" variant="tonal"
                                :color="item.isActive ? 'error' : 'success'" class="text-capitalize" density="comfortable"
                                :prepend-icon="item.isActive ? 'mdi-account-off' : 'mdi-account-check'"
                                @click.stop="toggleActive(item)">
                                {{ item.isActive ? 'Deactivate' : 'Activate' }}
                            </v-btn>
                        </template>
                    </v-tooltip>
                </div>
            </template>
        </v-data-table>
        <v-dialog v-model="viewDialog" max-width="900" scrollable>
            <InstructorDetails v-if="viewDialog" :instructor-id="viewingInstructorId" @close="viewDialog = false" />
        </v-dialog>
    </div>
</template>

<script setup>
import { useInstructorStore } from '@/store/InstructorStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, onMounted, watch } from 'vue';
import InstructorForm from '@/components/instructor/InstructorForm.vue';
import InstructorDetails from '@/components/instructor/InstructorDetails.vue';
import DownloadExcel from 'vue-json-excel3';

const instructorStore = useInstructorStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(instructorStore)

const headers = ref([
    { title: 'Full Name', key: 'fullName' },
    { title: 'Phone', key: 'phoneNumber' },
    { title: 'Schedule', key: 'scheduleType' },
    { title: 'License No.', key: 'licenseNumber' },
    { title: 'License Valid', key: 'licenseValidityDate' },
    { title: 'Status', key: 'isActive' },
    { title: 'Actions', key: 'actions', sortable: false }
])

const headersExcel = {
    'Full Name': 'fullName',
    'Phone': 'phoneNumber',
    'Schedule': 'scheduleType',
    'License No.': 'licenseNumber',
    'License Valid': 'licenseValidityDate',
    'Status': 'statusLabel'
}

const items = ref([])
const searchText = ref('')
const dialog = ref(false)
const viewDialog = ref(false)
const viewingInstructorId = ref(null)
const editedIndex = ref(-1)
const editedInstructor = ref(null)
const editedInstructorId = ref(null)

// Full name = FirstName + LastName only (no ParentName). Backend returns fullName built that way.
function normalizeInstructor(row) {
    if (!row || typeof row !== 'object') return null
    const fullName = row.fullName ?? row.FullName ?? ''
    return {
        id: row.userId ?? row.UserId,
        userId: row.userId ?? row.UserId,
        fullName,
        phoneNumber: row.phoneNumber ?? row.PhoneNumber ?? '',
        scheduleType: row.scheduleType ?? row.ScheduleType ?? '',
        licenseNumber: row.licenseNumber ?? row.LicenseNumber ?? '',
        licenseValidityDate: row.licenseValidityDate ?? row.LicenseValidityDate ?? '',
        isActive: row.isActive ?? row.IsActive ?? false,
        statusLabel: row.isActive ?? row.IsActive ? 'Active' : 'Inactive'
    }
}

const loadInstructors = () => {
    instructorStore.getInstructorsList(searchText.value)
        .then((response) => {
            const body = response?.data
            if (body && (body.status === 'error' || body.responseMsg)) {
                settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'Error loading instructors' })
                return
            }
            const arr = Array.isArray(body?.data) ? body.data : (body?.data?.data ?? []) || []
            items.value = arr.map(normalizeInstructor).filter(Boolean)
        })
        .catch((err) => {
            settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error loading instructors' })
        })
}

let searchTimeout = null
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout)
    searchTimeout = setTimeout(loadInstructors, 400)
}

const viewItem = (item) => {
    viewingInstructorId.value = item.userId
    viewDialog.value = true
}

const editItem = (item) => {
    editedInstructorId.value = item.userId
    instructorStore.getInstructorDetails(item.userId)
        .then((response) => {
            editedInstructor.value = response.data
            editedIndex.value = items.value.findIndex((i) => i.userId === item.userId)
            dialog.value = true
        })
        .catch(() => {
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading instructor' })
        })
}

const toggleActive = (item) => {
    instructorStore.toggleInstructorActive(item.userId)
        .then((response) => {
            const data = response?.data
            const isActive = data?.isActive ?? !item.isActive
            const idx = items.value.findIndex((i) => i.userId === item.userId)
            if (idx >= 0) {
                items.value = items.value.map((r, i) =>
                    i === idx ? { ...r, isActive, statusLabel: isActive ? 'Active' : 'Inactive' } : r
                )
            }
            settingStore.toggleSnackbar({ status: true, msg: isActive ? 'Instructor activated' : 'Instructor deactivated' })
        })
        .catch((err) => {
            settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error updating status' })
        })
}

function exportWord() {
    const cols = ['Full Name', 'Phone', 'Schedule', 'License No.', 'License Valid', 'Status']
    const rows = items.value.map((r) => [
        r.fullName || '',
        r.phoneNumber || '',
        r.scheduleType || '',
        r.licenseNumber || '',
        r.licenseValidityDate || '',
        r.statusLabel || (r.isActive ? 'Active' : 'Inactive')
    ])
    const html = `
<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>Instructors</title></head>
<body>
<table border="1" cellpadding="4" cellspacing="0">
<thead><tr>${cols.map((c) => `<th>${c}</th>`).join('')}</tr></thead>
<tbody>
${rows.map((row) => `<tr>${row.map((c) => `<td>${c}</td>`).join('')}</tr>`).join('')}
</tbody>
</table>
</body>
</html>`
    const blob = new Blob([html], { type: 'application/msword' })
    const a = document.createElement('a')
    a.href = URL.createObjectURL(blob)
    a.download = 'instructors.doc'
    a.click()
    URL.revokeObjectURL(a.href)
}

const handleSaved = () => {
    dialog.value = false
    editedIndex.value = -1
    editedInstructor.value = null
    editedInstructorId.value = null
    loadInstructors()
}

onMounted(loadInstructors)

watch(dialog, (val) => {
    if (!val) {
        editedIndex.value = -1
        editedInstructor.value = null
        editedInstructorId.value = null
    }
})
</script>

<style scoped>
.instructors-container {
    width: 100%;
    padding: 16px 20px;
    margin: 0;
    background: linear-gradient(180deg, #f8f9fa 0%, #fff 100%);
    min-height: 100%;
}

.instructors-table {
    width: 100%;
    border-radius: 12px;
    overflow: hidden;
}

.instructors-toolbar {
    padding: 16px 24px !important;
    min-height: 64px !important;
    gap: 12px;
    background: rgba(255, 255, 255, 0.95);
    border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.instructors-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.instructors-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.instructors-action-btn {
    border-radius: 4px !important;
}

.instructors-filters-wrap {
    margin-left: auto;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 10px;
}

.instructors-add-btn {
    min-height: 40px;
    border-radius: 4px !important;
}

.instructors-filter-search {
    min-width: 280px;
}

.instructors-table :deep(.v-data-table__wrapper) {
    width: 100%;
}

.instructors-table :deep(thead th) {
    font-weight: 600;
    font-size: 13px;
    padding: 14px 16px !important;
    background: #f5f5f5;
    color: #424242;
}

.instructors-table :deep(tbody td) {
    padding: 12px 16px !important;
    font-size: 14px;
}

.instructors-table :deep(.v-toolbar) {
    padding: 16px 24px !important;
    min-height: 64px !important;
}

.instructors-toolbar :deep(.v-toolbar__content) {
    height: auto !important;
    min-height: 0 !important;
    overflow: visible !important;
}

.instructors-toolbar :deep(.v-btn),
.instructors-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 1024px) and (min-width: 601px) {
    .instructors-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
    }

    .instructors-filters-wrap {
        margin-left: 0;
        width: 100%;
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 10px;
    }

    .instructors-filter-search {
        width: 100%;
        min-width: 0;
    }

    .instructors-add-btn {
        width: 100%;
        grid-column: 1 / -1;
    }
}

@media (max-width: 600px) {
    .instructors-container {
        padding: 8px !important;
    }

    .instructors-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        width: 100%;
    }

    .instructors-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 8px;
    }

    .instructors-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .instructors-filters-wrap {
        margin-left: 0;
        width: 100%;
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
    }

    .instructors-filter-search {
        width: 100%;
        min-width: 0;
    }

    .instructors-add-btn {
        width: 100%;
        min-height: 44px;
    }

    .instructors-table :deep(thead th),
    .instructors-table :deep(tbody td) {
        padding: 6px 8px !important;
        font-size: 0.75rem !important;
    }
    .instructors-table :deep(.v-toolbar) {
        padding: 8px !important;
        flex-wrap: wrap;
    }
}
</style>
