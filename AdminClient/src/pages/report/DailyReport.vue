<template>
    <div class="page-container">
        <div class="page-header mb-4">
            <div class="page-title">Raporti Ditor</div>
        </div>
        <!-- ─── Stats Cards — horizontally scrollable on mobile ─── -->
        <div class="stats-scroll-wrap mb-4">
            <div class="stats-scroll">
                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar color="primary" size="44">
                            <v-icon icon="mdi-calendar-today" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ selectedDateDisplay }}</div>
                            <div class="text-caption text-medium-emphasis">Data e zgjedhur</div>
                        </div>
                    </v-card-text>
                </v-card>

                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar color="success" size="44">
                            <v-icon icon="mdi-cash-plus" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ formatCurrency(totals.incomeTotal) }}</div>
                            <div class="text-caption text-medium-emphasis">Hyrjet</div>
                        </div>
                    </v-card-text>
                </v-card>

                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar color="error" size="44">
                            <v-icon icon="mdi-cash-minus" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ formatCurrency(totals.expenseTotal) }}</div>
                            <div class="text-caption text-medium-emphasis">Daljet</div>
                        </div>
                    </v-card-text>
                </v-card>

                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar :color="totals.balance >= 0 ? 'info' : 'warning'" size="44">
                            <v-icon icon="mdi-scale-balance" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ formatCurrency(totals.balance) }}</div>
                            <div class="text-caption text-medium-emphasis">Balanca</div>
                        </div>
                    </v-card-text>
                </v-card>
            </div>
        </div>

        <!-- ─── Filters Row ─── -->
        <v-card elevation="2" rounded="lg" class="mb-4">
            <v-card-text>
                <v-row align="center" dense>
                    <!-- Date picker -->
                    <v-col cols="12" sm="2">
                        <v-menu v-model="dateMenu" :close-on-content-click="false" transition="scale-transition">
                            <template v-slot:activator="{ props }">
                                <v-text-field v-bind="props" v-model="selectedDateDisplay" label="Data"
                                    readonly prepend-inner-icon="mdi-calendar" variant="outlined" density="compact"
                                    hide-details clearable @click:clear="clearDateFilter"></v-text-field>
                            </template>
                            <v-date-picker v-model="calendarDate" color="primary" @update:model-value="handleDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                    <!-- Month -->
                    <v-col cols="6" sm="2">
                        <v-select v-model="filterMonth" :items="monthOptions" label="Muaji" variant="outlined"
                            density="compact" hide-details clearable></v-select>
                    </v-col>
                    <!-- Year -->
                    <v-col cols="6" sm="2">
                        <v-select v-model="filterYear" :items="yearOptions" label="Viti" variant="outlined"
                            density="compact" hide-details clearable></v-select>
                    </v-col>
                    <!-- Search -->
                    <v-col cols="12" sm="3">
                        <v-text-field v-model="searchText" label="Kërko sipas emrit" prepend-inner-icon="mdi-magnify"
                            variant="outlined" density="compact" hide-details clearable
                            @keyup.enter="loadEntries"></v-text-field>
                    </v-col>
                    <!-- Apply -->
                    <v-col cols="12" sm="3" class="d-flex ga-2 align-center">
                        <v-btn color="primary" variant="elevated" @click="loadEntries" prepend-icon="mdi-filter">
                            Filtro
                        </v-btn>
                        <!-- Day status -->
                        <v-chip :color="dayStatus === 'Closed' ? 'error' : 'success'" variant="tonal" size="small" class="ml-2">
                            <v-icon start :icon="dayStatus === 'Closed' ? 'mdi-lock' : 'mdi-lock-open'" size="14"></v-icon>
                            {{ dayStatus === 'Closed' ? 'Mbyllur' : 'Hapur' }}
                        </v-chip>
                        <v-btn v-if="selectedDateDisplay" :color="dayStatus === 'Closed' ? 'success' : 'warning'" variant="tonal" size="small"
                            @click="toggleDayStatus" :prepend-icon="dayStatus === 'Closed' ? 'mdi-lock-open-variant' : 'mdi-lock'">
                            {{ dayStatus === 'Closed' ? 'Rihap' : 'Mbyll' }}
                        </v-btn>
                    </v-col>
                </v-row>
            </v-card-text>
        </v-card>

        <!-- ─── Tabs: Income / Expenses ─── -->
        <v-card elevation="2" rounded="lg">
            <v-tabs v-model="activeTab" color="primary" grow>
                <v-tab value="Income">
                    <v-icon start icon="mdi-cash-plus"></v-icon>
                    Hyrjet
                    <v-chip size="x-small" color="success" variant="tonal" class="ml-2">{{ incomeEntries.length }}</v-chip>
                </v-tab>
                <v-tab value="Expense">
                    <v-icon start icon="mdi-cash-minus"></v-icon>
                    Daljet
                    <v-chip size="x-small" color="error" variant="tonal" class="ml-2">{{ expenseEntries.length }}</v-chip>
                </v-tab>
            </v-tabs>
            <v-divider></v-divider>

            <!-- Add entry button -->
            <div class="d-flex align-center justify-end pa-3 ga-2">
                <v-btn color="primary" variant="elevated" prepend-icon="mdi-plus"
                    @click="openCreateDialog" :disabled="dayStatus === 'Closed' && !!selectedDateDisplay">
                    Regjistro hyrje
                </v-btn>
                <v-btn color="success" variant="tonal" prepend-icon="mdi-file-excel" @click="exportExcel">
                    Excel
                </v-btn>
                <v-btn color="error" variant="tonal" prepend-icon="mdi-file-pdf-box" @click="exportPdf">
                    PDF
                </v-btn>
            </div>

            <!-- Income Tab -->
            <v-window v-model="activeTab">
                <v-window-item value="Income">
                    <v-data-table :headers="tableHeaders" :items="incomeEntries" :loading="loading"
                        class="ledger-table" density="comfortable" items-per-page="50"
                        :items-per-page-options="[25, 50, 100, -1]" hover>
                        <template v-slot:item.serialNumber="{ item }">
                            <span class="font-weight-bold">{{ item.serialNumber }}</span>
                        </template>
                        <template v-slot:item.amount="{ item }">
                            <span :class="item.amount < 0 ? 'text-error' : 'text-success'" class="font-weight-bold">
                                {{ formatCurrency(item.amount) }}
                            </span>
                        </template>
                        <template v-slot:item.sourceType="{ item }">
                            <v-chip size="x-small" :color="sourceColor(item.sourceType)" variant="tonal">
                                {{ sourceLabel(item.sourceType) }}
                            </v-chip>
                        </template>
                        <template v-slot:item.actions="{ item }">
                            <div class="d-flex align-center ga-1">
                                <v-btn v-if="!item.reversalOfEntryId && item.sourceType !== 'Reversal'"
                                    icon variant="tonal" size="36" color="warning"
                                    @click="openReverseDialog(item)" :disabled="dayStatus === 'Closed' && !!selectedDateDisplay">
                                    <v-icon size="20">mdi-undo-variant</v-icon>
                                    <v-tooltip activator="parent" location="top">E kthyer</v-tooltip>
                                </v-btn>
                                <v-chip v-if="item.reversalOfEntryId" size="x-small" color="warning" variant="tonal">
                                    E kthyer
                                </v-chip>
                            </div>
                        </template>
                    </v-data-table>
                </v-window-item>

                <!-- Expenses Tab -->
                <v-window-item value="Expense">
                    <v-data-table :headers="tableHeaders" :items="expenseEntries" :loading="loading"
                        class="ledger-table" density="comfortable" items-per-page="50"
                        :items-per-page-options="[25, 50, 100, -1]" hover>
                        <template v-slot:item.serialNumber="{ item }">
                            <span class="font-weight-bold">{{ item.serialNumber }}</span>
                        </template>
                        <template v-slot:item.amount="{ item }">
                            <span :class="item.amount < 0 ? 'text-success' : 'text-error'" class="font-weight-bold">
                                {{ formatCurrency(item.amount) }}
                            </span>
                        </template>
                        <template v-slot:item.sourceType="{ item }">
                            <v-chip size="x-small" :color="sourceColor(item.sourceType)" variant="tonal">
                                {{ sourceLabel(item.sourceType) }}
                            </v-chip>
                        </template>
                        <template v-slot:item.actions="{ item }">
                            <div class="d-flex align-center ga-1">
                                <v-btn v-if="!item.reversalOfEntryId && item.sourceType !== 'Reversal'"
                                    icon variant="tonal" size="36" color="warning"
                                    @click="openReverseDialog(item)" :disabled="dayStatus === 'Closed' && !!selectedDateDisplay">
                                    <v-icon size="20">mdi-undo-variant</v-icon>
                                    <v-tooltip activator="parent" location="top">E kthyer</v-tooltip>
                                </v-btn>
                                <v-chip v-if="item.reversalOfEntryId" size="x-small" color="warning" variant="tonal">
                                    E kthyer
                                </v-chip>
                            </div>
                        </template>
                    </v-data-table>
                </v-window-item>
            </v-window>
        </v-card>

        <!-- ─── Create Entry Dialog ─── -->
        <v-dialog v-model="createDialog" max-width="600" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-plus-circle" color="primary"></v-icon>
                    <span>Regjistro {{ activeTab === 'Income' ? 'hyrje' : 'dalje' }}</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-5">
                    <v-alert v-if="formError" type="error" variant="tonal" density="compact" class="mb-4">{{ formError }}</v-alert>
                    <v-row dense>
                        <v-col cols="12" sm="6">
                            <v-menu v-model="formDateMenu" :close-on-content-click="false" transition="scale-transition">
                                <template v-slot:activator="{ props }">
                                    <v-text-field v-bind="props" v-model="formDate" label="Data"
                                        readonly prepend-inner-icon="mdi-calendar" variant="outlined"
                                        density="compact"></v-text-field>
                                </template>
                                <v-date-picker v-model="formDateModel" color="primary"
                                    @update:model-value="handleFormDateChange"></v-date-picker>
                            </v-menu>
                        </v-col>
                        <v-col cols="12" sm="6">
                            <v-text-field v-model="formFullName" label="Emri i plotë" variant="outlined"
                                density="compact" prepend-inner-icon="mdi-account"></v-text-field>
                        </v-col>
                        <v-col cols="12" sm="6">
                            <v-text-field v-model.number="formAmount" label="Shuma" type="number"
                                variant="outlined" density="compact" prepend-inner-icon="mdi-cash"
                                min="0.01" step="0.01"></v-text-field>
                        </v-col>
                        <v-col cols="12" sm="6">
                            <v-select v-model="formEntryType" :items="['Hyrje', 'Dalje']" label="Lloji"
                                variant="outlined" density="compact" prepend-inner-icon="mdi-tag"></v-select>
                        </v-col>
                        <v-col cols="12">
                            <v-text-field v-model="formDescription" label="Përshkrimi" variant="outlined"
                                density="compact" prepend-inner-icon="mdi-text"></v-text-field>
                        </v-col>
                    </v-row>
                </v-card-text>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="createDialog = false">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="saving" @click="saveEntry">Ruaj</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- ─── Reverse Entry Dialog ─── -->
        <v-dialog v-model="reverseDialog" max-width="500">
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-undo-variant" color="warning"></v-icon>
                    <span>E kthyer #{{ reverseTarget?.serialNumber }}</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-5">
                    <v-alert type="warning" variant="tonal" density="compact" class="mb-4">
                        This will create an adjustment (reversal) entry with a negative amount. The original entry will NOT be deleted.
                    </v-alert>
                    <div class="mb-3">
                        <div class="text-body-2 text-medium-emphasis">Original</div>
                        <div class="font-weight-medium">{{ reverseTarget?.fullName }} — {{ formatCurrency(reverseTarget?.amount) }}</div>
                        <div class="text-body-2">{{ reverseTarget?.description }}</div>
                    </div>
                    <v-text-field v-model="reverseReason" label="Reason for reversal (optional)" variant="outlined"
                        density="compact" prepend-inner-icon="mdi-text"></v-text-field>
                </v-card-text>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="reverseDialog = false">Cancel</v-btn>
                    <v-btn color="warning" variant="elevated" :loading="reversing" @click="doReverse">Konfirmo rikthimin</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useDailyReportStore } from '@/store/DailyReportStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const store = useDailyReportStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(store);

// ─── Date handling ───
const calendarDate = ref(new Date());
const selectedDateStr = ref(formatDateObj(new Date()));
const selectedDateDisplay = computed(() => selectedDateStr.value);
const dateMenu = ref(false);

function formatDateObj(value) {
    if (!value) return '';
    if (value instanceof Date) {
        const dd = String(value.getDate()).padStart(2, '0');
        const mm = String(value.getMonth() + 1).padStart(2, '0');
        const yyyy = value.getFullYear();
        return `${dd}.${mm}.${yyyy}`;
    }
    const str = String(value).split('T')[0];
    const parts = str.split('-');
    if (parts.length === 3) return `${parts[2]}.${parts[1]}.${parts[0]}`;
    return str;
}

function handleDateChange(v) {
    selectedDateStr.value = formatDateObj(v);
    dateMenu.value = false;
    filterMonth.value = null;
    filterYear.value = null;
    loadEntries();
    loadDayStatus();
}

function clearDateFilter() {
    selectedDateStr.value = '';
    calendarDate.value = null;
    loadEntries();
}

function formatCurrency(v) {
    if (v == null || isNaN(v)) return '0.00';
    return Number(v).toFixed(2);
}

// ─── Filters ───
const filterMonth = ref(null);
const filterYear = ref(null);
const searchText = ref('');
const activeTab = ref('Income');

const monthOptions = [
    { title: 'January', value: 1 }, { title: 'February', value: 2 }, { title: 'March', value: 3 },
    { title: 'April', value: 4 }, { title: 'May', value: 5 }, { title: 'June', value: 6 },
    { title: 'July', value: 7 }, { title: 'August', value: 8 }, { title: 'September', value: 9 },
    { title: 'October', value: 10 }, { title: 'November', value: 11 }, { title: 'December', value: 12 }
];

const currentYear = new Date().getFullYear();
const yearOptions = Array.from({ length: 5 }, (_, i) => currentYear - 2 + i);

// ─── Data ───
const allEntries = ref([]);
const totals = ref({ incomeTotal: 0, expenseTotal: 0, balance: 0 });
const dayStatus = ref('Open');

const incomeEntries = computed(() => allEntries.value.filter(e => e.entryType === 'Income'));
const expenseEntries = computed(() => allEntries.value.filter(e => e.entryType === 'Expense'));

const tableHeaders = [
    { title: '#', key: 'serialNumber', width: '60px' },
    { title: 'Data', key: 'entryDate', width: '110px' },
    { title: 'Emri i plotë', key: 'fullName' },
    { title: 'Shuma', key: 'amount', width: '120px' },
    { title: 'Përshkrimi', key: 'description' },
    { title: 'Burimi', key: 'sourceType', width: '130px' },
    { title: 'Veprimet', key: 'actions', sortable: false, width: '100px' },
];

function loadEntries() {
    const date = selectedDateStr.value || null;
    const month = filterMonth.value || null;
    const year = filterYear.value || null;
    const search = searchText.value || null;

    store.getEntries(date, month, year, null, search)
        .then((res) => {
            const d = res?.data;
            allEntries.value = d?.data || [];
            totals.value = {
                incomeTotal: d?.incomeTotal ?? 0,
                expenseTotal: d?.expenseTotal ?? 0,
                balance: d?.balance ?? 0,
            };
        })
        .catch(() => {
            allEntries.value = [];
            totals.value = { incomeTotal: 0, expenseTotal: 0, balance: 0 };
        });
}

function loadDayStatus() {
    if (!selectedDateStr.value) {
        dayStatus.value = 'Open';
        return;
    }
    store.getDayStatus(selectedDateStr.value)
        .then((res) => {
            dayStatus.value = res?.data?.status || 'Open';
        })
        .catch(() => { dayStatus.value = 'Open'; });
}

function toggleDayStatus() {
    if (!selectedDateStr.value) return;
    store.toggleDayStatus(selectedDateStr.value)
        .then((res) => {
            dayStatus.value = res?.data?.dayStatus || 'Open';
            settingStore.alertMessage({ type: 'success', message: res?.data?.responseMsg || 'Status updated.' });
        })
        .catch((err) => {
            settingStore.alertMessage({ type: 'error', message: err?.response?.data?.responseMsg || 'Failed to toggle status.' });
        });
}

// ─── Source helpers ───
function sourceColor(type) {
    if (type === 'CandidateInstallment') return 'primary';
    if (type === 'DrivingSession') return 'orange';
    if (type === 'Reversal') return 'warning';
    return 'grey';
}

function sourceLabel(type) {
    if (type === 'CandidateInstallment') return 'Candidate';
    if (type === 'DrivingSession') return 'Driving Session';
    if (type === 'Reversal') return 'Reversal';
    return 'Manual';
}

// ─── Create Entry Dialog ───
const createDialog = ref(false);
const formDateMenu = ref(false);
const formDateModel = ref(null);
const formDate = ref('');
const formFullName = ref('');
const formAmount = ref(0);
const formEntryType = ref('Income');
const formDescription = ref('');
const formError = ref('');
const saving = ref(false);

function openCreateDialog() {
    formDate.value = selectedDateStr.value || formatDateObj(new Date());
    formDateModel.value = calendarDate.value || new Date();
    formFullName.value = '';
    formAmount.value = 0;
    formEntryType.value = activeTab.value;
    formDescription.value = '';
    formError.value = '';
    createDialog.value = true;
}

function handleFormDateChange(v) {
    formDate.value = formatDateObj(v);
    formDateMenu.value = false;
}

function saveEntry() {
    formError.value = '';
    if (!formDate.value || !formFullName.value || !formAmount.value || !formDescription.value) {
        formError.value = 'All fields are required.';
        return;
    }
    saving.value = true;
    store.createEntry({
        entryDate: formDate.value,
        entryType: formEntryType.value,
        fullName: formFullName.value,
        amount: formAmount.value,
        description: formDescription.value,
    })
        .then((res) => {
            saving.value = false;
            if (res?.data?.status === 'success') {
                createDialog.value = false;
                settingStore.alertMessage({ type: 'success', message: res.data.responseMsg || 'Entry created.' });
                loadEntries();
            } else {
                formError.value = res?.data?.responseMsg || 'Failed to create entry.';
            }
        })
        .catch((err) => {
            saving.value = false;
            formError.value = err?.response?.data?.responseMsg || 'Failed to create entry.';
        });
}

// ─── Reverse Entry Dialog ───
const reverseDialog = ref(false);
const reverseTarget = ref(null);
const reverseReason = ref('');
const reversing = ref(false);

function openReverseDialog(item) {
    reverseTarget.value = item;
    reverseReason.value = '';
    reverseDialog.value = true;
}

function doReverse() {
    if (!reverseTarget.value) return;
    reversing.value = true;
    store.reverseEntry({
        originalEntryId: reverseTarget.value.dailyReportEntryId,
        reason: reverseReason.value || null,
    })
        .then((res) => {
            reversing.value = false;
            if (res?.data?.status === 'success') {
                reverseDialog.value = false;
                settingStore.alertMessage({ type: 'success', message: res.data.responseMsg || 'Reversal created.' });
                loadEntries();
            } else {
                settingStore.alertMessage({ type: 'error', message: res?.data?.responseMsg || 'Reversal failed.' });
            }
        })
        .catch((err) => {
            reversing.value = false;
            settingStore.alertMessage({ type: 'error', message: err?.response?.data?.responseMsg || 'Reversal failed.' });
        });
}

// ─── Export ───
function exportPdf() {
    const entries = activeTab.value === 'Income' ? incomeEntries.value : expenseEntries.value;
    const doc = new jsPDF();
    const title = `Daily Report - ${activeTab.value} - ${selectedDateStr.value || 'All dates'}`;
    doc.setFontSize(14);
    doc.text(title, 14, 18);

    autoTable(doc, {
        startY: 26,
        head: [['#', 'Date', 'Full Name', 'Amount', 'Description', 'Source']],
        body: entries.map(e => [
            e.serialNumber,
            e.entryDate,
            e.fullName,
            formatCurrency(e.amount),
            e.description,
            sourceLabel(e.sourceType),
        ]),
        styles: { fontSize: 9 },
        headStyles: { fillColor: [25, 118, 210] },
    });

    const totalAmt = entries.reduce((s, e) => s + (e.amount || 0), 0);
    doc.setFontSize(11);
    doc.text(`Total: ${formatCurrency(totalAmt)}`, 14, doc.lastAutoTable.finalY + 10);

    doc.save(`daily-report-${activeTab.value.toLowerCase()}-${selectedDateStr.value || 'all'}.pdf`);
}

function exportExcel() {
    const entries = activeTab.value === 'Income' ? incomeEntries.value : expenseEntries.value;
    // Build CSV manually
    const headers = ['#', 'Date', 'Full Name', 'Amount', 'Description', 'Source'];
    const rows = entries.map(e => [
        e.serialNumber,
        e.entryDate,
        e.fullName,
        e.amount,
        e.description,
        sourceLabel(e.sourceType),
    ]);

    let csv = headers.join(',') + '\n';
    rows.forEach(r => {
        csv += r.map(v => `"${String(v ?? '').replace(/"/g, '""')}"`).join(',') + '\n';
    });

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `daily-report-${activeTab.value.toLowerCase()}-${selectedDateStr.value || 'all'}.csv`;
    link.click();
    URL.revokeObjectURL(url);
}

// ─── Init ───
onMounted(() => {
    loadEntries();
    loadDayStatus();
});
</script>

<style scoped>
/* ─── Stats horizontal scroll ─── */
.stats-scroll-wrap {
    width: 100%;
}

.stats-scroll {
    display: flex;
    gap: 16px;
}

.stat-card {
    border-radius: 12px;
    transition: transform 0.15s ease;
    flex: 1 1 0;
    min-width: 0;
}

.stat-card:hover {
    transform: translateY(-2px);
}

.ledger-table :deep(th) {
    font-weight: 700 !important;
    text-transform: uppercase;
    font-size: 0.75rem !important;
    letter-spacing: 0.05em;
}

.ledger-table :deep(td) {
    font-size: 0.875rem;
}

@media (max-width: 600px) {
    .stats-scroll-wrap {
        overflow-x: auto;
        -webkit-overflow-scrolling: touch;
        margin-left: -12px;
        margin-right: -12px;
        padding: 0 12px;
    }

    .stats-scroll {
        min-width: max-content;
        gap: 10px;
        padding-bottom: 4px;
    }

    .stat-card {
        flex: 0 0 auto;
        min-width: 160px;
    }

    .stat-card :deep(.v-card-text) {
        padding: 10px 12px !important;
    }

    .stat-card :deep(.text-h6) {
        font-size: 0.95rem !important;
    }

    .stat-card :deep(.v-avatar) {
        width: 36px !important;
        height: 36px !important;
    }

    .stat-card :deep(.v-avatar .v-icon) {
        font-size: 18px !important;
    }

    .ledger-table :deep(thead th),
    .ledger-table :deep(td) {
        padding: 6px 8px !important;
        font-size: 0.75rem !important;
    }
}
</style>
