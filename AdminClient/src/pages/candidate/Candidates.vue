<template>
    <div class="page-container">
        <!-- Page Header -->
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Kandidatët</div>
                <div class="page-subtitle">Menaxhoni listën e të gjithë kandidatëve</div>
            </div>
            <v-dialog v-model="dialog" max-width="1200" scrollable>
                <template v-slot:activator="{ props: activatorProps }">
                    <v-btn v-if="!isInstructor" v-bind="activatorProps" color="primary" variant="flat"
                        class="text-none" prepend-icon="mdi-plus" size="default">
                        Regjistro kandidatë
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

        <!-- Table Card -->
        <v-card class="table-card">
            <!-- Filter Bar -->
            <div class="filter-bar">
                <div class="export-group">
                    <v-btn variant="tonal" color="success" size="small" class="text-none" prepend-icon="mdi-file-excel">
                        <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                            name="candidates.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportPdf">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-text-field
                        v-model="searchText"
                        label="Kërko (Emri, Numri personal)"
                        prepend-inner-icon="mdi-magnify"
                        clearable
                        class="filter-field filter-field--search"
                        @update:model-value="handleSearch"
                    ></v-text-field>
                    <v-select
                        v-model="selectedCategory"
                        :items="categories"
                        item-title="categoryName"
                        item-value="categoryId"
                        label="Kategoria"
                        clearable
                        class="filter-field filter-field--category"
                        @update:model-value="handleFilter"
                    ></v-select>
                    <v-select
                        v-model="selectedYear"
                        :items="years"
                        label="Viti"
                        clearable
                        class="filter-field filter-field--year"
                        @update:model-value="handleFilter"
                    ></v-select>
                </div>
            </div>

            <!-- Data Table -->
            <v-data-table
                :headers="headers"
                :items="items"
                :loading="loading"
                class="candidates-table"
                items-per-page="15"
            >
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
                    </div>
                </template>
            </v-data-table>
        </v-card>

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
import CandidateInstructorEdit from '@/components/candidate/CandidateInstructorEdit.vue';
import { useRouter } from 'vue-router';

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)
const router = useRouter()

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}')
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || ''
        return role === 'Instructor'
    } catch { return false }
})

// Admin headers
const headersAdmin = [
    { title: 'Nr. Rendor', key: 'serialNumber' },
    { title: 'Emri', key: 'firstName' },
    { title: 'Mbiemri', key: 'lastName' },
    { title: 'Numri i telefonit', key: 'phoneNumber' },
    { title: 'Kategoria', key: 'categoryName' },
    { title: 'Instruktori', key: 'instructorName' },
    { title: 'Lloji i vetures', key: 'vehicleType' },
    { title: 'Orët praktike', key: 'practicalHoursDisplay' },
    { title: 'Pagesa e shërbimit', key: 'servicePaymentDisplay' },
    { title: 'Veprimet', key: 'actions', sortable: false }
]

// Instructor headers (no address, amount, schedule, practicalHours; add counts)
const headersInstructor = [
    { title: 'Nr. Rendor', key: 'serialNumber' },
    { title: 'Emri', key: 'firstName' },
    { title: 'Mbiemri', key: 'lastName' },
    { title: 'Numri i telefonit', key: 'phoneNumber' },
    { title: 'Kategoria', key: 'categoryName' },
    { title: 'Lloji i vetures', key: 'vehicleType' },
    { title: 'Orët praktike', key: 'practicalLessonCount' },
    { title: 'Orët të kryera', key: 'completedHours' },
    { title: 'Orët të mbetura', key: 'remainingHours' },
    { title: 'Veprimet', key: 'actions', sortable: false }
]

const headers = computed(() => isInstructor.value ? headersInstructor : headersAdmin)

const headersExcelAdmin = {
    'Nr. Rendor': 'serialNumber',
    'Emri': 'firstName',
    'Mbiemri': 'lastName',
    'Numri i telefonit': 'phoneNumber',
    'Kategoria': 'categoryName',
    'Instruktori': 'instructorName',
    'Lloji i vetures': 'vehicleType',
    'Orët praktike': 'practicalHoursDisplay',
    'Pagesa e shërbimit': 'servicePaymentDisplay',
}

const headersExcelInstructor = {
    'Nr. Rendor': 'serialNumber',
    'First Name': 'firstName',
    'Mbiemri': 'lastName',
    'Numri i telefonit': 'phoneNumber',
    'Kategoria': 'categoryName',
    'Lloji i vetures': 'vehicleType',
    'Orët praktike': 'practicalLessonCount',
    'Orët të kryera': 'completedHours',
    'Orët të mbetura': 'remainingHours',
}

const headersExcel = computed(() => isInstructor.value ? headersExcelInstructor : headersExcelAdmin)

const headersPdfAdmin = ['Serial Number', 'First Name', 'Last Name', 'Phone', 'Category', 'Instructor', 'Vehicle Type', 'Practical Hours', 'Total Amount']
const headersPdfInstructor = ['Serial Number', 'First Name', 'Last Name', 'Phone', 'Category', 'Vehicle Type', 'Practical Lessons', 'Completed Hours', 'Remaining Hours']

const items = ref([])
const categories = ref([])
const years = ref([])
const searchText = ref('')
const selectedCategory = ref(null)
const selectedYear = ref(new Date().getFullYear())
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
        return [row.serialNumber, row.firstName, row.lastName, row.phoneNumber, row.categoryName, row.instructorName, row.vehicleType, row.practicalHoursDisplay, row.servicePaymentDisplay]
    })
    autoTable(doc, { head: [pdfHeaders], body: bodyRows })
    doc.save('candidates.pdf')
}

function normalizeCandidate(row) {
    if (!row || typeof row !== 'object') return null
    const practicalHours = row.practicalHours ?? row.PracticalHours ?? 0
    const completedHours = row.completedHours ?? row.CompletedHours ?? 0
    const totalServiceAmount = row.totalServiceAmount ?? row.TotalServiceAmount ?? 0
    const totalPaidAmount = row.totalPaidAmount ?? row.TotalPaidAmount ?? 0
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
        practicalHours,
        totalServiceAmount,
        totalPaidAmount,
        practicalHoursDisplay: `${practicalHours} / ${completedHours}`,
        servicePaymentDisplay: `${totalServiceAmount} / ${totalPaidAmount}`,
        practicalLessonCount: row.practicalLessonCount ?? row.PracticalLessonCount ?? 0,
        completedHours,
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
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Error loading candidates' })
        })
}

const loadCategories = () => {
    const apiUrl = import.meta.env.VITE_API_URL + '/api/Candidates/GetCategories'
    candidateStore.getCategories()
        .then((response) => {
            if (response.data && (response.data.status === 'error' || response.data.responseMsg)) {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg || 'Error loading categories' })
                return
            }
            const raw = (response.data && response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? String(c.categoryId ?? c.CategoryId ?? '')
            })).filter((c) => c.categoryId != null)
        })
        .catch((error) => {
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

// Actions — navigate to dedicated pages
const viewItem = (item) => {
    router.push(`/candidates/${item.candidateId}`)
}

const editItem = (item) => {
    if (isInstructor.value) {
        instructorEditCandidateId.value = item.candidateId
        instructorEditDialog.value = true
        return
    }
    router.push(`/candidates/${item.candidateId}/edit`)
}

const handleSaved = () => {
    dialog.value = false
    editedIndex.value = -1
    editedCandidate.value = null
    editedCandidateId.value = null
    searchText.value = ''
    selectedCategory.value = null
    selectedYear.value = new Date().getFullYear()
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
.filter-inputs {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.filter-field--search {
    min-width: 240px;
}

.filter-field--category {
    min-width: 160px;
}

.filter-field--year {
    min-width: 110px;
}

.candidates-table {
    border: none !important;
    box-shadow: none !important;
}

@media (max-width: 960px) {
    .filter-bar {
        flex-direction: column !important;
        align-items: stretch !important;
    }

    .filter-inputs {
        width: 100%;
    }

    .filter-field--search,
    .filter-field--category,
    .filter-field--year {
        min-width: 0;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search,
    .filter-field--category,
    .filter-field--year {
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
