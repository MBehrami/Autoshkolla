<template>
    <div class="page-container">
        <!-- Page Header -->
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Orë Shtesë</div>
                <div class="page-subtitle">Menaxhoni listën e kandidatëve për orë shtesë</div>
            </div>
            <v-dialog v-model="dialog" max-width="900" scrollable>
                <template v-slot:activator="{ props: activatorProps }">
                    <v-btn v-bind="activatorProps" color="primary" variant="flat"
                        class="text-none" prepend-icon="mdi-plus" size="default">
                        Regjistro
                    </v-btn>
                </template>
                <AdditionalLessonForm
                    v-model="dialog"
                    :item="editedItem"
                    :item-id="editedItemId"
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
                            name="additional-lessons.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportPdf">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-text-field
                        v-model="searchText"
                        label="Kërko (Emri, Numri personal, Kontakti)"
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

        <!-- View Dialog -->
        <v-dialog v-model="viewDialog" max-width="700" scrollable>
            <v-card v-if="viewingItem">
                <v-card-title class="d-flex justify-space-between align-center">
                    <span>{{ viewingItem.firstName }} {{ viewingItem.lastName }}</span>
                    <v-btn icon="mdi-close" variant="text" @click="viewDialog = false"></v-btn>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-row dense>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.firstName" label="Emri" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.lastName" label="Mbiemri" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.personalNumber" label="Numri personal" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.contactNumber" label="Kontakti" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.categoryName" label="Kategoria" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.instructorName" label="Instruktori" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.vehicleType" label="Lloji i vetures" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.paymentMethod" label="Metoda e pagesës" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.practicalHours" label="Orët praktike" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12" md="6"><v-text-field :model-value="viewingItem.servicePaymentDisplay" label="Pagesa e shërbimit" variant="outlined" density="compact" readonly hide-details></v-text-field></v-col>
                        <v-col cols="12"><v-textarea :model-value="viewingItem.additionalNotes" label="Shënime" variant="outlined" density="compact" readonly hide-details rows="2" auto-grow></v-textarea></v-col>
                    </v-row>
                </v-card-text>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useAdditionalLessonStore } from '@/store/AdditionalLessonStore';
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, onMounted, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';
import AdditionalLessonForm from '@/components/additional-lessons/AdditionalLessonForm.vue';

const store = useAdditionalLessonStore()
const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(store)

const headers = [
    { title: 'Emri', key: 'firstName' },
    { title: 'Mbiemri', key: 'lastName' },
    { title: 'Numri personal', key: 'personalNumber' },
    { title: 'Kontakti', key: 'contactNumber' },
    { title: 'Kategoria', key: 'categoryName' },
    { title: 'Instruktori', key: 'instructorName' },
    { title: 'Lloji i vetures', key: 'vehicleType' },
    { title: 'Orët praktike', key: 'practicalHours' },
    { title: 'Pagesa (Total / Paguar)', key: 'servicePaymentDisplay' },
    { title: 'Veprimet', key: 'actions', sortable: false }
]

const headersExcel = {
    'Emri': 'firstName',
    'Mbiemri': 'lastName',
    'Numri personal': 'personalNumber',
    'Kontakti': 'contactNumber',
    'Kategoria': 'categoryName',
    'Instruktori': 'instructorName',
    'Lloji i vetures': 'vehicleType',
    'Orët praktike': 'practicalHours',
    'Pagesa (Total / Paguar)': 'servicePaymentDisplay',
}

const headersPdf = ['First Name', 'Last Name', 'Personal Number', 'Contact', 'Category', 'Instructor', 'Vehicle Type', 'Practical Hours', 'Payment']

const items = ref([])
const categories = ref([])
const searchText = ref('')
const selectedCategory = ref(null)
const dialog = ref(false)
const editedIndex = ref(-1)
const editedItem = ref(null)
const editedItemId = ref(null)
const viewDialog = ref(false)
const viewingItem = ref(null)

let searchTimeout = null
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout)
    searchTimeout = setTimeout(() => loadItems(), 500)
}

const handleFilter = () => loadItems()

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' })
    doc.text('Additional Lessons', 14, 10)
    const bodyRows = items.value.map((row) => [
        row.firstName, row.lastName, row.personalNumber, row.contactNumber,
        row.categoryName, row.instructorName, row.vehicleType,
        row.practicalHours, row.servicePaymentDisplay
    ])
    autoTable(doc, { head: [headersPdf], body: bodyRows })
    doc.save('additional-lessons.pdf')
}

function normalizeItem(row) {
    if (!row || typeof row !== 'object') return null
    const sp = row.servicePayment ?? row.ServicePayment ?? 0
    const paid = row.totalPaidAmount ?? row.TotalPaidAmount ?? 0
    return {
        id: row.additionalLessonId ?? row.AdditionalLessonId,
        additionalLessonId: row.additionalLessonId ?? row.AdditionalLessonId,
        firstName: row.firstName ?? row.FirstName ?? '',
        lastName: row.lastName ?? row.LastName ?? '',
        personalNumber: row.personalNumber ?? row.PersonalNumber ?? '',
        contactNumber: row.contactNumber ?? row.ContactNumber ?? '',
        categoryId: row.categoryId ?? row.CategoryId ?? 0,
        categoryName: row.categoryName ?? row.CategoryName ?? '',
        instructorId: row.instructorId ?? row.InstructorId ?? null,
        instructorName: row.instructorName ?? row.InstructorName ?? '',
        vehicleType: row.vehicleType ?? row.VehicleType ?? '',
        paymentMethod: row.paymentMethod ?? row.PaymentMethod ?? '',
        practicalHours: row.practicalHours ?? row.PracticalHours ?? 0,
        servicePayment: sp,
        totalPaidAmount: paid,
        servicePaymentDisplay: `${sp} / ${paid}`,
        additionalNotes: row.additionalNotes ?? row.AdditionalNotes ?? '',
    }
}

const loadItems = () => {
    store.getList(searchText.value, selectedCategory.value)
        .then((response) => {
            const body = response?.data
            if (body && (body.status === 'error' || body.responseMsg)) {
                settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'Error loading data' })
                return
            }
            const dataCamel = body?.data
            const dataPascal = body?.Data
            let arr
            if (body && Array.isArray(dataCamel)) arr = dataCamel
            else if (body && Array.isArray(dataPascal)) arr = dataPascal
            else if (Array.isArray(body)) arr = body
            else arr = []
            items.value = arr.map(normalizeItem).filter(Boolean)
        })
        .catch((error) => {
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Error loading data' })
        })
}

const loadCategories = () => {
    candidateStore.getCategories()
        .then((response) => {
            const raw = (response.data && response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? String(c.categoryId ?? c.CategoryId ?? '')
            })).filter((c) => c.categoryId != null)
        })
        .catch(() => {})
}

const viewItem = (item) => {
    viewingItem.value = item
    viewDialog.value = true
}

const editItem = (item) => {
    editedIndex.value = 1
    editedItemId.value = item.additionalLessonId
    editedItem.value = { ...item }
    dialog.value = true
}

const handleSaved = () => {
    dialog.value = false
    editedIndex.value = -1
    editedItem.value = null
    editedItemId.value = null
    searchText.value = ''
    selectedCategory.value = null
    loadItems()
}

onMounted(() => {
    loadItems()
    loadCategories()
})

watch(dialog, (newVal) => {
    if (!newVal) {
        editedIndex.value = -1
        editedItem.value = null
        editedItemId.value = null
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

.filter-field--category {
    min-width: 160px;
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
    .filter-field--category {
        min-width: 0;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search,
    .filter-field--category {
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
