<template>
    <div class="vehicles-container">
        <!-- ─── Stats Cards ─── -->
        <v-row class="mb-4">
            <v-col cols="12" sm="4">
                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4">
                        <v-avatar color="primary" size="48">
                            <v-icon icon="mdi-calendar-today" color="white" size="24"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ selectedDateDisplay }}</div>
                            <div class="text-body-2 text-medium-emphasis">Selected Date</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="4">
                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4">
                        <v-avatar color="info" size="48">
                            <v-icon icon="mdi-account-group" color="white" size="24"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ stats.totalSessions }}</div>
                            <div class="text-body-2 text-medium-emphasis">Sessions Scheduled</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="4">
                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4">
                        <v-avatar color="success" size="48">
                            <v-icon icon="mdi-cash" color="white" size="24"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ formatCurrency(stats.totalPayments) }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Payments</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Main layout: Calendar + Sessions ─── -->
        <v-row>
            <!-- Left: Calendar -->
            <v-col cols="12" md="4">
                <v-card elevation="2" rounded="lg">
                    <v-card-title class="d-flex align-center ga-2 pa-4">
                        <v-icon icon="mdi-calendar" color="primary"></v-icon>
                        <span>Calendar</span>
                    </v-card-title>
                    <v-divider></v-divider>
                    <v-date-picker
                        v-model="calendarDate"
                        color="primary"
                        show-adjacent-months
                        class="calendar-picker"
                        @update:model-value="handleDateChange"
                    ></v-date-picker>
                    <v-card-text class="text-center text-body-2 text-medium-emphasis py-2">
                        Click a date to view driving sessions
                    </v-card-text>
                </v-card>
            </v-col>

            <!-- Right: Sessions table -->
            <v-col cols="12" md="8">
                <v-data-table
                    :headers="headers"
                    :items="sessions"
                    :loading="loading"
                    class="elevation-2 vehicles-table"
                    no-data-text="No driving sessions for this date"
                >
                    <template v-slot:top>
                        <v-toolbar density="comfortable" flat class="vehicles-toolbar">
                            <template v-slot:prepend>
                                <div class="d-flex flex-row align-center ga-2">
                                    <download-excel :data="sessions" :fields="headersExcel" type="xlsx"
                                        worksheet="all-data" name="driving-sessions.xlsx">
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
                                <div class="d-flex align-center ga-4">
                                    <v-chip color="primary" variant="tonal" size="small" class="font-weight-medium">
                                        {{ selectedDateDisplay }}
                                    </v-chip>
                                    <v-btn
                                        color="primary"
                                        variant="elevated"
                                        class="text-capitalize"
                                        prepend-icon="mdi-plus"
                                        @click="dialog = true"
                                    >
                                        Add Session
                                    </v-btn>
                                </div>
                            </template>
                        </v-toolbar>
                    </template>

                    <!-- Time slot with badge -->
                    <template v-slot:[`item.drivingTime`]="{ item }">
                        <v-chip size="small" color="primary" variant="tonal">
                            <v-icon start icon="mdi-clock-outline" size="14"></v-icon>
                            {{ item.drivingTime }}
                        </v-chip>
                    </template>

                    <!-- Payment amount -->
                    <template v-slot:[`item.paymentAmount`]="{ item }">
                        {{ formatCurrency(item.paymentAmount) }}
                    </template>

                    <!-- Vehicle display -->
                    <template v-slot:[`item.vehicleDisplay`]="{ item }">
                        <v-chip size="small" variant="outlined" color="grey-darken-1">
                            <v-icon start icon="mdi-car" size="14"></v-icon>
                            {{ item.vehicleDisplay }}
                        </v-chip>
                    </template>

                    <!-- Actions -->
                    <template v-slot:[`item.actions`]="{ item }">
                        <v-btn
                            v-if="isAdmin"
                            icon
                            variant="text"
                            size="small"
                            color="error"
                            @click="confirmDelete(item)"
                        >
                            <v-icon icon="mdi-delete-outline" size="18"></v-icon>
                            <v-tooltip activator="parent" location="top">Delete</v-tooltip>
                        </v-btn>
                    </template>
                </v-data-table>
            </v-col>
        </v-row>

        <!-- ─── Create Driving Session Dialog ─── -->
        <v-dialog v-model="dialog" max-width="700" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-car-clock" color="primary"></v-icon>
                    <span>Register Driving Session</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <v-form ref="formRef" @submit.prevent="saveSession">
                        <v-row>
                            <!-- Candidate -->
                            <v-col cols="12" md="6">
                                <v-autocomplete
                                    v-model="form.candidateId"
                                    :items="candidateOptions"
                                    item-title="fullName"
                                    item-value="candidateId"
                                    label="Candidate (Kandidati) *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-account"
                                ></v-autocomplete>
                            </v-col>

                            <!-- Vehicle -->
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="form.vehicleId"
                                    :items="vehicleOptions"
                                    item-title="label"
                                    item-value="vehicleId"
                                    label="Vehicle (Automjeti) *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-car"
                                ></v-select>
                            </v-col>

                            <!-- Driving Date -->
                            <v-col cols="12" md="6">
                                <v-menu v-model="drivingDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="form.drivingDate"
                                            label="Driving Date (Data e vozitjes) *"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            :rules="[v => !!v || 'Required']"
                                            v-bind="menuProps"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="drivingDateModel" color="primary" @update:model-value="handleDrivingDate"></v-date-picker>
                                </v-menu>
                            </v-col>

                            <!-- Driving Time -->
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="form.drivingTime"
                                    :items="timeSlots"
                                    label="Driving Time (Ora e vozitjes) *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-clock-outline"
                                ></v-select>
                            </v-col>

                            <!-- Payment Amount -->
                            <v-col cols="12" md="6">
                                <v-text-field
                                    v-model.number="form.paymentAmount"
                                    label="Payment Amount (Shuma e pages)"
                                    variant="outlined"
                                    density="compact"
                                    type="number"
                                    min="0"
                                    prepend-inner-icon="mdi-cash"
                                ></v-text-field>
                            </v-col>

                            <!-- Payment Date -->
                            <v-col cols="12" md="6">
                                <v-menu v-model="paymentDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="form.paymentDate"
                                            label="Payment Date (Data e pages)"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            clearable
                                            v-bind="menuProps"
                                            @click:clear="form.paymentDate = ''"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="paymentDateModel" color="primary" @update:model-value="handlePaymentDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                        </v-row>

                        <v-alert v-if="formError" type="error" density="compact" class="mt-2 mb-0" closable @click:close="formError = ''">
                            {{ formError }}
                        </v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="dialog = false">Cancel</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="saving" @click="saveSession">
                        Save
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Delete confirm -->
        <v-dialog v-model="deleteDialog" max-width="400">
            <v-card rounded="lg">
                <v-card-title>Confirm Delete</v-card-title>
                <v-card-text>Are you sure you want to delete this driving session?</v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
                    <v-btn color="error" variant="elevated" :loading="deleting" @click="doDelete">Delete</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useDrivingSessionStore } from '@/store/DrivingSessionStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, nextTick } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';

const store = useDrivingSessionStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(store);

// ─── Role helpers ───
const isAdmin = computed(() => {
    try {
        const roleId = localStorage.getItem('userRoleId');
        return roleId === '1';
    } catch { return false; }
});

// ─── Calendar ───
const calendarDate = ref(new Date());
const selectedDateStr = ref(formatDate(new Date()));

const selectedDateDisplay = computed(() => selectedDateStr.value);

function formatDate(value) {
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
    selectedDateStr.value = formatDate(v);
    loadSessions();
    loadStats();
}

// ─── Stats ───
const stats = ref({ totalSessions: 0, totalPayments: 0 });

function loadStats() {
    store.getSessionStats(selectedDateStr.value)
        .then((res) => {
            const d = res?.data;
            stats.value = {
                totalSessions: d?.totalSessions ?? 0,
                totalPayments: d?.totalPayments ?? 0,
            };
        })
        .catch(() => { stats.value = { totalSessions: 0, totalPayments: 0 }; });
}

function formatCurrency(v) {
    if (v == null || isNaN(v)) return '0.00';
    return Number(v).toFixed(2);
}

// ─── Sessions table ───
const headers = [
    { title: '#', key: 'index', width: '50px', sortable: false },
    { title: 'Time', key: 'drivingTime', width: '110px' },
    { title: 'Candidate', key: 'candidateName' },
    { title: 'Vehicle', key: 'vehicleDisplay' },
    { title: 'Payment', key: 'paymentAmount', width: '110px' },
    { title: 'Payment Date', key: 'paymentDate', width: '130px' },
    { title: 'Actions', key: 'actions', sortable: false, width: '80px' },
];

const headersExcel = {
    '#': 'index',
    'Time': 'drivingTime',
    'Candidate': 'candidateName',
    'Vehicle': 'vehicleDisplay',
    'Payment': 'paymentAmount',
    'Payment Date': 'paymentDate',
};

const sessions = ref([]);

function normalizeSession(row, idx) {
    if (!row || typeof row !== 'object') return null;
    const plate = row.vehiclePlate ?? row.VehiclePlate ?? '';
    const brand = row.vehicleBrand ?? row.VehicleBrand ?? '';
    return {
        index: idx + 1,
        drivingSessionId: row.drivingSessionId ?? row.DrivingSessionId,
        candidateId: row.candidateId ?? row.CandidateId,
        candidateName: row.candidateName ?? row.CandidateName ?? '',
        vehicleId: row.vehicleId ?? row.VehicleId,
        vehicleDisplay: plate + (brand ? ` – ${brand}` : ''),
        drivingDate: row.drivingDate ?? row.DrivingDate ?? '',
        drivingTime: row.drivingTime ?? row.DrivingTime ?? '',
        paymentAmount: row.paymentAmount ?? row.PaymentAmount ?? 0,
        paymentDate: row.paymentDate ?? row.PaymentDate ?? '',
    };
}

function loadSessions() {
    store.getSessionsByDate(selectedDateStr.value)
        .then((res) => {
            const body = res?.data;
            const rawData = body?.data ?? body?.Data ?? [];
            sessions.value = rawData.map(normalizeSession).filter(Boolean);
        })
        .catch(() => {
            sessions.value = [];
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading sessions' });
        });
}

// ─── Create dialog ───
const dialog = ref(false);
const saving = ref(false);
const formRef = ref(null);
const formError = ref('');

const form = ref({
    candidateId: null,
    vehicleId: null,
    drivingDate: '',
    drivingTime: null,
    paymentAmount: 0,
    paymentDate: '',
});

const drivingDateMenu = ref(false);
const drivingDateModel = ref(null);
const paymentDateMenu = ref(false);
const paymentDateModel = ref(null);

function handleDrivingDate(v) {
    form.value.drivingDate = formatDate(v);
    nextTick(() => { drivingDateMenu.value = false; });
}
function handlePaymentDate(v) {
    form.value.paymentDate = formatDate(v);
    nextTick(() => { paymentDateMenu.value = false; });
}

// Time slots: 08:00 – 15:00 every 15 min
const timeSlots = (() => {
    const slots = [];
    for (let h = 8; h <= 15; h++) {
        for (let m = 0; m < 60; m += 15) {
            if (h === 15 && m > 0) break;
            slots.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`);
        }
    }
    return slots;
})();

// Dropdowns
const candidateOptions = ref([]);
const vehicleOptions = ref([]);

function loadDropdowns() {
    store.getCandidatesDropdown()
        .then((res) => {
            candidateOptions.value = (res?.data?.data ?? []).map(c => ({
                candidateId: c.candidateId ?? c.CandidateId,
                fullName: c.fullName ?? c.FullName ?? '',
            }));
        })
        .catch(() => { candidateOptions.value = []; });

    store.getVehiclesDropdown()
        .then((res) => {
            vehicleOptions.value = (res?.data?.data ?? []).map(v => ({
                vehicleId: v.vehicleId ?? v.VehicleId,
                label: `${v.plateNumber ?? v.PlateNumber}${v.brand ? ' – ' + v.brand : ''}`,
            }));
        })
        .catch(() => { vehicleOptions.value = []; });
}

async function saveSession() {
    formError.value = '';
    const valid = await formRef.value?.validate();
    if (!valid?.valid) return;

    saving.value = true;
    try {
        const res = await store.createDrivingSession({
            candidateId: form.value.candidateId,
            vehicleId: form.value.vehicleId,
            drivingDate: form.value.drivingDate,
            drivingTime: form.value.drivingTime,
            paymentAmount: form.value.paymentAmount || 0,
            paymentDate: form.value.paymentDate || null,
        });
        const body = res?.data;
        if (body?.status === 'error') {
            formError.value = body.responseMsg || 'An error occurred';
            saving.value = false;
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Driving session created successfully!' });
        dialog.value = false;
        // Reset form
        form.value = { candidateId: null, vehicleId: null, drivingDate: '', drivingTime: null, paymentAmount: 0, paymentDate: '' };
        drivingDateModel.value = null;
        paymentDateModel.value = null;
        loadSessions();
        loadStats();
    } catch (err) {
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Failed to create session';
        formError.value = msg;
    } finally {
        saving.value = false;
    }
}

// ─── Delete ───
const deleteDialog = ref(false);
const deleting = ref(false);
const deleteTarget = ref(null);

function confirmDelete(item) {
    deleteTarget.value = item;
    deleteDialog.value = true;
}

async function doDelete() {
    if (!deleteTarget.value) return;
    deleting.value = true;
    try {
        await store.deleteDrivingSession(deleteTarget.value.drivingSessionId);
        settingStore.toggleSnackbar({ status: true, msg: 'Session deleted' });
        deleteDialog.value = false;
        loadSessions();
        loadStats();
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Error deleting session' });
    } finally {
        deleting.value = false;
    }
}

// ─── PDF Export ───
function exportPdf() {
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text(`Driving Sessions – ${selectedDateStr.value}`, 14, 10);
    const head = ['#', 'Time', 'Candidate', 'Vehicle', 'Payment', 'Payment Date'];
    const body = sessions.value.map(r => [r.index, r.drivingTime, r.candidateName, r.vehicleDisplay, formatCurrency(r.paymentAmount), r.paymentDate]);
    autoTable(doc, { head: [head], body });
    doc.save('driving-sessions.pdf');
}

// ─── Init ───
onMounted(() => {
    loadSessions();
    loadStats();
    loadDropdowns();
});
</script>

<style scoped>
.vehicles-container {
    width: 100%;
    padding: 16px 20px;
    margin: 0;
    background: linear-gradient(180deg, #f8f9fa 0%, #fff 100%);
    min-height: 100%;
}

.stat-card {
    border-radius: 12px;
    transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12) !important;
}

.calendar-picker {
    width: 100%;
}

.calendar-picker :deep(.v-date-picker) {
    width: 100%;
}

.vehicles-table {
    width: 100%;
    border-radius: 12px;
    overflow: hidden;
}

.vehicles-toolbar {
    padding: 16px 24px !important;
    min-height: 64px !important;
    gap: 12px;
    background: rgba(255, 255, 255, 0.95);
    border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.vehicles-table :deep(.v-data-table__wrapper) {
    width: 100%;
}

.vehicles-table :deep(thead th) {
    font-weight: 600;
    font-size: 13px;
    padding: 14px 16px !important;
    background: #f5f5f5;
    color: #424242;
}

.vehicles-table :deep(tbody td) {
    padding: 12px 16px !important;
    font-size: 14px;
}

.vehicles-table :deep(.v-toolbar) {
    padding: 16px 24px !important;
    min-height: 64px !important;
}
</style>
