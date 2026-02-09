<template>
    <div class="vehicles-container">
        <v-data-table
            :headers="headers"
            :items="items"
            :loading="loading"
            class="elevation-2 vehicles-table"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="vehicles-toolbar">
                    <template v-slot:prepend>
                        <div class="d-flex flex-row align-center ga-2">
                            <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                name="vehicle-fuel.xlsx">
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
                        <div class="vehicles-filters d-flex flex-wrap align-center ga-4">
                            <v-text-field
                                v-model="searchText"
                                :label="isInstructor ? 'Search (Plate, Fuel Type)' : 'Search (Plate, Staff, Fuel Type)'"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="fuel-filter-search"
                                @update:model-value="handleSearch"
                            ></v-text-field>
                            <v-select
                                v-model="filterVehicleId"
                                :items="vehicleOptions"
                                item-title="label"
                                item-value="vehicleId"
                                label="Vehicle"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="fuel-filter-vehicle"
                                @update:model-value="loadFuelEntries"
                            ></v-select>
                            <v-menu v-model="fromDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                <template v-slot:activator="{ props: menuProps }">
                                    <v-text-field
                                        :model-value="fromDateDisplay"
                                        label="From date"
                                        variant="outlined"
                                        density="compact"
                                        prepend-inner-icon="mdi-calendar"
                                        readonly
                                        hide-details
                                        clearable
                                        class="fuel-filter-date"
                                        v-bind="menuProps"
                                        @click:clear="clearFromDate"
                                    ></v-text-field>
                                </template>
                                <v-date-picker v-model="fromDateModel" color="primary" @update:model-value="handleFromDate"></v-date-picker>
                            </v-menu>
                            <v-menu v-model="toDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                <template v-slot:activator="{ props: menuProps }">
                                    <v-text-field
                                        :model-value="toDateDisplay"
                                        label="To date"
                                        variant="outlined"
                                        density="compact"
                                        prepend-inner-icon="mdi-calendar"
                                        readonly
                                        hide-details
                                        clearable
                                        class="fuel-filter-date"
                                        v-bind="menuProps"
                                        @click:clear="clearToDate"
                                    ></v-text-field>
                                </template>
                                <v-date-picker v-model="toDateModel" color="primary" @update:model-value="handleToDate"></v-date-picker>
                            </v-menu>
                            <v-dialog v-model="dialog" max-width="800" scrollable>
                                <template v-slot:activator="{ props: activatorProps }">
                                    <v-btn v-bind="activatorProps" color="primary" variant="elevated"
                                        class="text-capitalize" prepend-icon="mdi-plus">
                                        Add Fuel Entry
                                    </v-btn>
                                </template>
                                <VehicleFuelForm
                                    v-model="dialog"
                                    @saved="handleSaved"
                                />
                            </v-dialog>
                        </div>
                    </template>
                </v-toolbar>
            </template>
        </v-data-table>
    </div>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, nextTick } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';
import VehicleFuelForm from '@/components/vehicle/VehicleFuelForm.vue';

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(vehicleStore);

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || '';
        return role === 'Instructor';
    } catch { return false; }
});

const headersAdmin = [
    { title: 'Fill Date', key: 'fillDate' },
    { title: 'Vehicle', key: 'vehicleDisplay' },
    { title: 'Fuel Amount', key: 'fuelAmount' },
    { title: 'Fuel Type', key: 'fuelType' },
    { title: 'Staff', key: 'staffName' },
];
const headersInstructor = [
    { title: 'Fill Date', key: 'fillDate' },
    { title: 'Vehicle', key: 'vehicleDisplay' },
    { title: 'Fuel Amount', key: 'fuelAmount' },
    { title: 'Fuel Type', key: 'fuelType' },
];
const headers = computed(() => isInstructor.value ? headersInstructor : headersAdmin);

const headersExcelAdmin = { 'Fill Date': 'fillDate', 'Vehicle': 'vehicleDisplay', 'Fuel Amount': 'fuelAmount', 'Fuel Type': 'fuelType', 'Staff': 'staffName' };
const headersExcelInstructor = { 'Fill Date': 'fillDate', 'Vehicle': 'vehicleDisplay', 'Fuel Amount': 'fuelAmount', 'Fuel Type': 'fuelType' };
const headersExcel = computed(() => isInstructor.value ? headersExcelInstructor : headersExcelAdmin);

const headersPdfAdmin = ['Fill Date', 'Vehicle', 'Fuel Amount', 'Fuel Type', 'Staff'];
const headersPdfInstructor = ['Fill Date', 'Vehicle', 'Fuel Amount', 'Fuel Type'];

const items = ref([]);
const searchText = ref('');
const dialog = ref(false);

// Vehicle filter dropdown
const vehicleOptions = ref([]);
const filterVehicleId = ref(null);

// Date range filters
const fromDateMenu = ref(false);
const fromDateModel = ref(null);
const fromDateDisplay = ref('');
const toDateMenu = ref(false);
const toDateModel = ref(null);
const toDateDisplay = ref('');

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

const handleFromDate = (v) => {
    fromDateDisplay.value = formatDate(v);
    nextTick(() => { fromDateMenu.value = false; });
    loadFuelEntries();
};
const clearFromDate = () => {
    fromDateDisplay.value = '';
    fromDateModel.value = null;
    loadFuelEntries();
};
const handleToDate = (v) => {
    toDateDisplay.value = formatDate(v);
    nextTick(() => { toDateMenu.value = false; });
    loadFuelEntries();
};
const clearToDate = () => {
    toDateDisplay.value = '';
    toDateModel.value = null;
    loadFuelEntries();
};

let searchTimeout = null;
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => { loadFuelEntries(); }, 500);
};

function normalizeFuel(row) {
    if (!row || typeof row !== 'object') return null;
    const plate = row.vehiclePlate ?? row.VehiclePlate ?? '';
    const brand = row.vehicleBrand ?? row.VehicleBrand ?? '';
    return {
        id: row.vehicleFuelId ?? row.VehicleFuelId,
        fillDate: row.fillDate ?? row.FillDate ?? '',
        vehicleId: row.vehicleId ?? row.VehicleId,
        vehicleDisplay: plate + (brand ? ` – ${brand}` : ''),
        fuelAmount: row.fuelAmount ?? row.FuelAmount ?? 0,
        fuelType: row.fuelType ?? row.FuelType ?? '',
        staffName: row.staffName ?? row.StaffName ?? '',
    };
}

const loadFuelEntries = () => {
    vehicleStore.getVehicleFuelList(
        searchText.value || null,
        filterVehicleId.value || null,
        fromDateDisplay.value || null,
        toDateDisplay.value || null
    )
        .then((response) => {
            const body = response?.data;
            const rawData = body?.data ?? body?.Data ?? [];
            items.value = rawData.map(normalizeFuel).filter(Boolean);
        })
        .catch(() => {
            items.value = [];
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading fuel entries' });
        });
};

const loadVehicleOptions = () => {
    vehicleStore.getVehiclesDropdown()
        .then((res) => {
            const raw = res?.data?.data || [];
            vehicleOptions.value = raw.map(v => ({
                vehicleId: v.vehicleId,
                label: `${v.plateNumber}${v.brand ? ' – ' + v.brand : ''}`
            }));
        })
        .catch(() => { vehicleOptions.value = []; });
};

const handleSaved = () => {
    dialog.value = false;
    loadFuelEntries();
};

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text('Vehicle Fuel (Derivatet e Automjeteve)', 14, 10);
    const pdfHeaders = isInstructor.value ? headersPdfInstructor : headersPdfAdmin;
    const bodyRows = items.value.map((r) => {
        if (isInstructor.value) return [r.fillDate, r.vehicleDisplay, r.fuelAmount, r.fuelType];
        return [r.fillDate, r.vehicleDisplay, r.fuelAmount, r.fuelType, r.staffName];
    });
    autoTable(doc, { head: [pdfHeaders], body: bodyRows });
    doc.save('vehicle-fuel.pdf');
};

onMounted(() => {
    loadFuelEntries();
    loadVehicleOptions();
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

.vehicles-filters {
    gap: 16px;
    padding-left: 8px;
}

.fuel-filter-search {
    min-width: 220px;
}

.fuel-filter-vehicle {
    min-width: 180px;
}

.fuel-filter-date {
    min-width: 140px;
    max-width: 160px;
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

@media (max-width: 600px) {
    .vehicles-container {
        padding: 8px !important;
    }
    .vehicles-table :deep(thead th),
    .vehicles-table :deep(tbody td) {
        padding: 6px 8px !important;
        font-size: 0.75rem !important;
    }
    .vehicles-table :deep(.v-toolbar) {
        padding: 8px !important;
        flex-wrap: wrap;
    }
}
</style>
