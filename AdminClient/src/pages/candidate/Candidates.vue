<template>
    <div class="candidates-container">
        <v-data-table
            :headers="headers"
            :items="items"
            :loading="loading"
            class="elevation-2 candidates-table"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="candidates-toolbar">
                    <template v-slot:prepend>
                        <div class="d-flex flex-row align-center ga-2">
                            <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                name="candidates.xlsx">
                                <v-btn icon variant="outlined" size="small" color="success">
                                    <v-icon icon="mdi-file-excel"></v-icon>
                                </v-btn>
                            </download-excel>
                            <v-btn icon variant="outlined" size="small" color="error" @click.stop="exportPdf">
                                <v-icon icon="mdi-file-pdf"></v-icon>
                            </v-btn>
                        </div>
                    </template>
                    <template v-slot:append>
                        <div class="candidates-filters d-flex flex-wrap align-center ga-4">
                            <v-text-field
                                v-model="searchText"
                                label="Search (Name, Personal Number)"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="candidates-filter-search"
                                @update:model-value="handleSearch"
                            ></v-text-field>
                            <v-select
                                v-model="selectedCategory"
                                :items="categories"
                                item-title="categoryName"
                                item-value="categoryId"
                                label="Category"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="candidates-filter-category"
                                @update:model-value="handleFilter"
                            ></v-select>
                            <v-select
                                v-model="selectedYear"
                                :items="years"
                                label="Year"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="candidates-filter-year"
                                @update:model-value="handleFilter"
                            ></v-select>
                            <v-dialog v-model="dialog" max-width="1200" scrollable>
                                <template v-slot:activator="{ props: activatorProps }">
                                    <v-btn v-if="!isInstructor" v-bind="activatorProps" color="primary" variant="elevated"
                                        class="text-capitalize" prepend-icon="mdi-plus">
                                        Add Candidate
                                    </v-btn>
                                </template>
                                <CandidateForm
                                    v-model="dialog"
                                    :candidate="editedCandidate"
                                    :candidate-id="editedCandidateId"
                                    :is-edit="editedIndex > -1"
                                    @saved="handleSaved"
                                />
                            </v-dialog>
                        </div>
                    </template>
                </v-toolbar>
            </template>
            <template v-slot:item.actions="{ item }">
                <div class="d-flex align-center ga-2">
                    <v-btn size="small" variant="tonal" color="primary" class="text-capitalize" density="comfortable"
                        prepend-icon="mdi-eye" @click.stop="viewItem(item)">
                        View
                    </v-btn>
                    <v-btn size="small" variant="tonal" color="secondary" class="text-capitalize" density="comfortable"
                        prepend-icon="mdi-pencil" @click.stop="editItem(item)">
                        Edit
                    </v-btn>
                </div>
            </template>
        </v-data-table>
        <v-dialog v-model="viewDialog" max-width="1000" scrollable>
            <CandidateDetails v-if="viewDialog" :candidate-id="viewingCandidateId" @close="viewDialog = false" />
        </v-dialog>
        <v-dialog v-model="instructorEditDialog" max-width="900" scrollable persistent>
            <CandidateInstructorEdit
                v-if="instructorEditDialog && instructorEditCandidateId"
                :candidate-id="instructorEditCandidateId"
                @close="closeInstructorEdit"
                @saved="loadCandidates"
            />
        </v-dialog>
    </div>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { computed, ref, onMounted, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';
import CandidateForm from '@/components/candidate/CandidateForm.vue';
import CandidateDetails from '@/components/candidate/CandidateDetails.vue';
import CandidateInstructorEdit from '@/components/candidate/CandidateInstructorEdit.vue';

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}')
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || ''
        return role === 'Instructor'
    } catch { return false }
})

// Admin headers
const headersAdmin = [
    { title: 'Serial Number', key: 'serialNumber' },
    { title: 'First Name', key: 'firstName' },
    { title: 'Last Name', key: 'lastName' },
    { title: 'Phone', key: 'phoneNumber' },
    { title: 'Category', key: 'categoryName' },
    { title: 'Instructor', key: 'instructorName' },
    { title: 'Vehicle Type', key: 'vehicleType' },
    { title: 'Practical Hours', key: 'practicalHours' },
    { title: 'Total Amount', key: 'totalServiceAmount' },
    { title: 'Actions', key: 'actions', sortable: false }
]

// Instructor headers (no address, amount, schedule, practicalHours; add counts)
const headersInstructor = [
    { title: 'Serial Number', key: 'serialNumber' },
    { title: 'First Name', key: 'firstName' },
    { title: 'Last Name', key: 'lastName' },
    { title: 'Phone', key: 'phoneNumber' },
    { title: 'Category', key: 'categoryName' },
    { title: 'Vehicle Type', key: 'vehicleType' },
    { title: 'Practical Lessons', key: 'practicalLessonCount' },
    { title: 'Completed Hours', key: 'completedHours' },
    { title: 'Remaining Hours', key: 'remainingHours' },
    { title: 'Actions', key: 'actions', sortable: false }
]

const headers = computed(() => isInstructor.value ? headersInstructor : headersAdmin)

const headersExcelAdmin = {
    'Serial Number': 'serialNumber',
    'First Name': 'firstName',
    'Last Name': 'lastName',
    'Phone': 'phoneNumber',
    'Category': 'categoryName',
    'Instructor': 'instructorName',
    'Vehicle Type': 'vehicleType',
    'Practical Hours': 'practicalHours',
    'Total Amount': 'totalServiceAmount',
}

const headersExcelInstructor = {
    'Serial Number': 'serialNumber',
    'First Name': 'firstName',
    'Last Name': 'lastName',
    'Phone': 'phoneNumber',
    'Category': 'categoryName',
    'Vehicle Type': 'vehicleType',
    'Practical Lessons': 'practicalLessonCount',
    'Completed Hours': 'completedHours',
    'Remaining Hours': 'remainingHours',
}

const headersExcel = computed(() => isInstructor.value ? headersExcelInstructor : headersExcelAdmin)

const headersPdfAdmin = ['Serial Number', 'First Name', 'Last Name', 'Phone', 'Category', 'Instructor', 'Vehicle Type', 'Practical Hours', 'Total Amount']
const headersPdfInstructor = ['Serial Number', 'First Name', 'Last Name', 'Phone', 'Category', 'Vehicle Type', 'Practical Lessons', 'Completed Hours', 'Remaining Hours']

const items = ref([])
const categories = ref([])
const years = ref([])
const searchText = ref('')
const selectedCategory = ref(null)
const selectedYear = ref(null)
const dialog = ref(false)
const viewDialog = ref(false)
const viewingCandidateId = ref(null)
const instructorEditDialog = ref(false)
const instructorEditCandidateId = ref(null)
const editedIndex = ref(-1)
const editedCandidate = ref(null)
const editedCandidateId = ref(null)

// Debounce search
let searchTimeout = null
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout)
    searchTimeout = setTimeout(() => {
        loadCandidates()
    }, 500)
}

const handleFilter = () => {
    loadCandidates()
}

// Export functions

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' })
    doc.text('Candidates', 14, 10)
    const pdfHeaders = isInstructor.value ? headersPdfInstructor : headersPdfAdmin
    const bodyRows = items.value.map((row) => {
        if (isInstructor.value) {
            return [row.serialNumber, row.firstName, row.lastName, row.phoneNumber, row.categoryName, row.vehicleType, row.practicalLessonCount, row.completedHours, row.remainingHours]
        }
        return [row.serialNumber, row.firstName, row.lastName, row.phoneNumber, row.categoryName, row.instructorName, row.vehicleType, row.practicalHours, row.totalServiceAmount]
    })
    autoTable(doc, { head: [pdfHeaders], body: bodyRows })
    doc.save('candidates.pdf')
}

// Normalize one row: camelCase keys + id for v-data-table (default item-value is "id")
function normalizeCandidate(row) {
    if (!row || typeof row !== 'object') return null
    return {
        id: row.candidateId ?? row.CandidateId,
        candidateId: row.candidateId ?? row.CandidateId,
        serialNumber: row.serialNumber ?? row.SerialNumber ?? '',
        firstName: row.firstName ?? row.FirstName ?? '',
        lastName: row.lastName ?? row.LastName ?? '',
        phoneNumber: row.phoneNumber ?? row.PhoneNumber ?? '',
        categoryName: row.categoryName ?? row.CategoryName ?? '',
        instructorName: row.instructorName ?? row.InstructorName ?? '',
        vehicleType: row.vehicleType ?? row.VehicleType ?? '',
        practicalHours: row.practicalHours ?? row.PracticalHours ?? 0,
        totalServiceAmount: row.totalServiceAmount ?? row.TotalServiceAmount ?? 0,
        practicalLessonCount: row.practicalLessonCount ?? row.PracticalLessonCount ?? 0,
        completedHours: row.completedHours ?? row.CompletedHours ?? 0,
        remainingHours: row.remainingHours ?? row.RemainingHours ?? 0
    }
}

// Load data – support { data: [...] } or { Data: [...] }; treat { status:'error', responseMsg } as error
const loadCandidates = () => {
    candidateStore.getCandidatesList(searchText.value, selectedCategory.value, selectedYear.value)
        .then((response) => {
            const body = response?.data
            if (body && (body.status === 'error' || body.responseMsg)) {
                const msg = body.responseMsg || 'Error loading candidates'
                console.warn('candidates API error response:', msg)
                settingStore.toggleSnackbar({ status: true, msg })
                return
            }
            const dataCamel = body?.data
            const dataPascal = body?.Data
            let arr
            if (body && Array.isArray(dataCamel)) {
                arr = dataCamel
            } else if (body && Array.isArray(dataPascal)) {
                arr = dataPascal
            } else if (body?.data?.data && Array.isArray(body.data.data)) {
                arr = body.data.data
            } else if (Array.isArray(body)) {
                arr = body
            } else {
                arr = []
            }
            const mapped = arr.map(normalizeCandidate).filter(Boolean)
            items.value = [...mapped]
        })
        .catch((error) => {
            console.error('candidates load error', error?.response?.status, error?.response?.data)
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Error loading candidates' })
        })
}

const loadCategories = () => {
    const apiUrl = import.meta.env.VITE_API_URL + '/api/Candidates/GetCategories'
    candidateStore.getCategories()
        .then((response) => {
            if (response.data && (response.data.status === 'error' || response.data.responseMsg)) {
                console.warn('[Categories] API returned error body:', response.data)
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg || 'Error loading categories' })
                return
            }
            const raw = (response.data && response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? String(c.categoryId ?? c.CategoryId ?? '')
            })).filter((c) => c.categoryId != null)
            console.log('[Categories] Loaded', categories.value.length, 'categories from', apiUrl, '→', categories.value)
        })
        .catch((error) => {
            console.error('[Categories] Request failed:', import.meta.env.VITE_API_URL + '/api/Candidates/GetCategories', error?.response?.status, error?.response?.data, error)
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Error loading categories' })
        })
}

const loadYears = () => {
    candidateStore.getAvailableYears()
        .then((response) => {
            const raw = response?.data?.data
            years.value = Array.isArray(raw) ? raw : []
        })
        .catch(() => {
            years.value = []
        })
}

// Actions
const viewItem = (item) => {
    viewingCandidateId.value = item.candidateId
    viewDialog.value = true
}

const editItem = (item) => {
    if (isInstructor.value) {
        instructorEditCandidateId.value = item.candidateId
        instructorEditDialog.value = true
        return
    }
    editedCandidateId.value = item.candidateId
    candidateStore.getCandidateDetails(item.candidateId)
        .then((response) => {
            editedCandidate.value = response.data
            editedIndex.value = items.value.indexOf(item)
            dialog.value = true
        })
}

const handleSaved = () => {
    dialog.value = false
    editedIndex.value = -1
    editedCandidate.value = null
    editedCandidateId.value = null
    searchText.value = ''
    selectedCategory.value = null
    selectedYear.value = null
    loadCandidates()
}

const closeInstructorEdit = () => {
    instructorEditDialog.value = false
    instructorEditCandidateId.value = null
    loadCandidates()
}

// Initialize
onMounted(() => {
    loadCandidates()
    loadCategories()
    loadYears()
})

watch(dialog, (newVal) => {
    if (!newVal) {
        editedIndex.value = -1
        editedCandidate.value = null
        editedCandidateId.value = null
    }
})
</script>

<style scoped>
.candidates-container {
    width: 100%;
    padding: 16px 20px;
    margin: 0;
    background: linear-gradient(180deg, #f8f9fa 0%, #fff 100%);
    min-height: 100%;
}

.candidates-table {
    width: 100%;
    border-radius: 12px;
    overflow: hidden;
}

.candidates-toolbar {
    padding: 16px 24px !important;
    min-height: 64px !important;
    gap: 12px;
    background: rgba(255, 255, 255, 0.95);
    border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.candidates-filters {
    gap: 16px;
    padding-left: 8px;
}

.candidates-filter-search {
    min-width: 260px;
}

.candidates-filter-category {
    min-width: 160px;
}

.candidates-filter-year {
    min-width: 120px;
}

.candidates-table :deep(.v-data-table__wrapper) {
    width: 100%;
}

.candidates-table :deep(thead th) {
    font-weight: 600;
    font-size: 13px;
    padding: 14px 16px !important;
    background: #f5f5f5;
    color: #424242;
}

.candidates-table :deep(tbody td) {
    padding: 12px 16px !important;
    font-size: 14px;
}

.candidates-table :deep(.v-toolbar) {
    padding: 16px 24px !important;
    min-height: 64px !important;
}

@media (max-width: 600px) {
    .candidates-container {
        padding: 8px !important;
    }
    .candidates-table :deep(thead th),
    .candidates-table :deep(tbody td) {
        padding: 6px 8px !important;
        font-size: 0.75rem !important;
    }
    .candidates-table :deep(.v-toolbar) {
        padding: 8px !important;
        flex-wrap: wrap;
    }
}
</style>
