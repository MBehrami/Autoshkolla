<template>
    <div class="page-container">
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Instruktorët</div>
                <div class="page-subtitle">Menaxhoni listën e instruktorëve</div>
            </div>
            <v-dialog v-model="dialog" max-width="1000" scrollable>
                <template v-slot:activator="{ props: activatorProps }">
                    <v-btn v-bind="activatorProps" color="primary" variant="flat"
                        class="text-none" prepend-icon="mdi-plus" size="default">
                        Regjistro Instruktor
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

        <v-card class="table-card">
            <div class="filter-bar">
                <div class="export-group">
                    <v-btn variant="tonal" color="success" size="small" class="text-none" prepend-icon="mdi-file-excel">
                        <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="instructors"
                            name="instructors.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportWord">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-text-field
                        v-model="searchText"
                        label="Kërko (Emri, Mbiemri)"
                        prepend-inner-icon="mdi-magnify"
                        clearable
                        class="filter-field filter-field--search"
                        @update:model-value="handleSearch"
                    ></v-text-field>
                </div>
            </div>

            <v-data-table
                :headers="headers"
                :items="items"
                :loading="loading"
            >
                <template v-slot:item.isActive="{ item }">
                    <v-chip :color="item.isActive ? 'success' : 'default'" size="small" variant="flat">
                        {{ item.isActive ? 'Active' : 'Inactive' }}
                    </v-chip>
                </template>
                <template v-slot:item.actions="{ item }">
                    <div class="d-flex align-center ga-1">
                        <v-btn icon variant="text" color="primary" class="action-btn" @click.stop="viewItem(item)">
                            <v-icon size="18">mdi-eye-outline</v-icon>
                            <v-tooltip activator="parent" location="top">Shiko</v-tooltip>
                        </v-btn>
                        <v-btn icon variant="text" color="secondary" class="action-btn" @click.stop="editItem(item)">
                            <v-icon size="18">mdi-pencil-outline</v-icon>
                            <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                        </v-btn>
                        <v-btn icon variant="text" :color="item.isActive ? 'error' : 'success'" class="action-btn" @click.stop="toggleActive(item)">
                            <v-icon size="18">{{ item.isActive ? 'mdi-account-off' : 'mdi-account-check' }}</v-icon>
                            <v-tooltip activator="parent" location="top">{{ item.isActive ? 'Deactivate' : 'Activate' }}</v-tooltip>
                        </v-btn>
                    </div>
                </template>
            </v-data-table>
        </v-card>

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
    { title: 'Emri i plotë', key: 'fullName' },
    { title: 'Numri i telefonit', key: 'phoneNumber' },
    { title: 'Lloji i kontratës', key: 'scheduleType' },
    { title: 'Numri i licensës', key: 'licenseNumber' },
    { title: 'Data e skadimit', key: 'licenseValidityDate' },
    { title: 'Statusi', key: 'isActive' },
    { title: 'Veprimet', key: 'actions', sortable: false }
])

const headersExcel = {
    'Emri i plotë': 'fullName',
    'Numri i telefonit': 'phoneNumber',
    'Tipi i orarit': 'scheduleType',
    'Numri i licensës': 'licenseNumber',
    'Data e skadimit': 'licenseValidityDate',
    'Statusi': 'statusLabel'
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
    const cols = ['Emri i plotë', 'Numri i telefonit', 'Tipi i orarit', 'Numri i licensës', 'Data e skadimit', 'Statusi']
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
.filter-inputs {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.filter-field--search {
    min-width: 280px;
}

@media (max-width: 960px) {
    .filter-bar {
        flex-direction: column !important;
        align-items: stretch !important;
    }

    .filter-inputs {
        width: 100%;
    }

    .filter-field--search {
        min-width: 0;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search {
        width: 100%;
    }

    .export-group {
        width: 100%;
    }

    .export-group .v-btn {
        flex: 1;
    }
}
</style>
