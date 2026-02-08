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
                                name="vehicles.xlsx">
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
                                label="Search (Plate, Brand, Type)"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="vehicles-filter-search"
                                @update:model-value="handleSearch"
                            ></v-text-field>
                            <v-select
                                v-model="statusFilter"
                                :items="statusOptions"
                                item-title="label"
                                item-value="value"
                                label="Status"
                                variant="outlined"
                                density="compact"
                                hide-details
                                class="vehicles-filter-status"
                                @update:model-value="loadVehicles"
                            ></v-select>
                            <v-dialog v-model="dialog" max-width="900" scrollable>
                                <template v-slot:activator="{ props: activatorProps }">
                                    <v-btn v-bind="activatorProps" color="primary" variant="elevated"
                                        class="text-capitalize" prepend-icon="mdi-plus">
                                        Add Vehicle
                                    </v-btn>
                                </template>
                                <VehicleForm
                                    v-model="dialog"
                                    :vehicle="editedVehicle"
                                    :vehicle-id="editedVehicleId"
                                    :is-edit="editedIndex > -1"
                                    @saved="handleSaved"
                                />
                            </v-dialog>
                        </div>
                    </template>
                </v-toolbar>
            </template>
            <template v-slot:item.statusDisplay="{ item }">
                <v-chip
                    :color="item.isActive ? 'success' : 'grey'"
                    size="small"
                    variant="elevated"
                >
                    {{ item.isActive ? 'Active' : 'Inactive' }}
                </v-chip>
            </template>
            <template v-slot:item.actions="{ item }">
                <div class="d-flex align-center ga-2">
                    <v-btn size="small" variant="tonal" color="secondary" class="text-capitalize" density="comfortable"
                        prepend-icon="mdi-pencil" @click.stop="editItem(item)">
                        Edit
                    </v-btn>
                    <v-btn size="small" variant="tonal"
                        :color="item.isActive ? 'warning' : 'success'"
                        class="text-capitalize" density="comfortable"
                        :prepend-icon="item.isActive ? 'mdi-close-circle-outline' : 'mdi-check-circle-outline'"
                        :loading="togglingId === item.vehicleId"
                        @click.stop="toggleStatus(item)">
                        {{ item.isActive ? 'Deactivate' : 'Activate' }}
                    </v-btn>
                </div>
            </template>
        </v-data-table>
    </div>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, onMounted, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';
import VehicleForm from '@/components/vehicle/VehicleForm.vue';

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(vehicleStore);

const headers = [
    { title: 'Plate Number', key: 'plateNumber' },
    { title: 'Chassis Number', key: 'chassisNumber' },
    { title: 'Color', key: 'color' },
    { title: 'Type', key: 'type' },
    { title: 'Brand', key: 'brand' },
    { title: 'Registration Date', key: 'registrationDate' },
    { title: 'Expiry Date', key: 'expiryDate' },
    { title: 'Certificate Nr.', key: 'certificateNumber' },
    { title: 'Status', key: 'statusDisplay', sortable: false },
    { title: 'Actions', key: 'actions', sortable: false }
];

const headersExcel = {
    'Plate Number': 'plateNumber',
    'Chassis Number': 'chassisNumber',
    'Color': 'color',
    'Type': 'type',
    'Brand': 'brand',
    'Registration Date': 'registrationDate',
    'Expiry Date': 'expiryDate',
    'Certificate Nr.': 'certificateNumber',
    'Status': 'statusLabel',
};

const headersPdf = ['Plate Number', 'Chassis Nr.', 'Color', 'Type', 'Brand', 'Reg. Date', 'Expiry Date', 'Cert. Nr.', 'Status'];

const statusOptions = [
    { label: 'Active', value: 'active' },
    { label: 'Inactive', value: 'inactive' },
    { label: 'All', value: 'all' },
];

const items = ref([]);
const searchText = ref('');
const statusFilter = ref('active');
const dialog = ref(false);
const editedIndex = ref(-1);
const editedVehicle = ref(null);
const editedVehicleId = ref(null);
const togglingId = ref(null);

let searchTimeout = null;
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => { loadVehicles(); }, 500);
};

function normalizeVehicle(row) {
    if (!row || typeof row !== 'object') return null;
    const active = row.isActive ?? row.IsActive ?? true;
    return {
        id: row.vehicleId ?? row.VehicleId,
        vehicleId: row.vehicleId ?? row.VehicleId,
        plateNumber: row.plateNumber ?? row.PlateNumber ?? '',
        chassisNumber: row.chassisNumber ?? row.ChassisNumber ?? '',
        color: row.color ?? row.Color ?? '',
        type: row.type ?? row.Type ?? '',
        brand: row.brand ?? row.Brand ?? '',
        registrationDate: row.registrationDate ?? row.RegistrationDate ?? '',
        expiryDate: row.expiryDate ?? row.ExpiryDate ?? '',
        certificateNumber: row.certificateNumber ?? row.CertificateNumber ?? '',
        isActive: active,
        statusLabel: active ? 'Active' : 'Inactive',
    };
}

const loadVehicles = () => {
    vehicleStore.getVehiclesList(searchText.value || null, statusFilter.value || 'active')
        .then((response) => {
            const body = response?.data;
            const rawData = body?.data ?? body?.Data ?? [];
            items.value = rawData.map(normalizeVehicle).filter(Boolean);
        })
        .catch(() => {
            items.value = [];
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading vehicles' });
        });
};

const editItem = (item) => {
    editedIndex.value = items.value.indexOf(item);
    editedVehicle.value = { ...item };
    editedVehicleId.value = item.vehicleId;
    dialog.value = true;
};

const handleSaved = () => {
    dialog.value = false;
    editedIndex.value = -1;
    editedVehicle.value = null;
    editedVehicleId.value = null;
    loadVehicles();
};

const toggleStatus = async (item) => {
    togglingId.value = item.vehicleId;
    try {
        const response = await vehicleStore.toggleVehicleStatus(item.vehicleId);
        const body = response?.data;
        settingStore.toggleSnackbar({ status: true, msg: body?.responseMsg || body?.ResponseMsg || 'Status updated' });
        loadVehicles();
    } catch (err) {
        settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error toggling status' });
    } finally {
        togglingId.value = null;
    }
};

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text('Vehicles (Automjetet)', 14, 10);
    const bodyRows = items.value.map((r) => [
        r.plateNumber, r.chassisNumber, r.color, r.type, r.brand, r.registrationDate, r.expiryDate, r.certificateNumber, r.statusLabel
    ]);
    autoTable(doc, { head: [headersPdf], body: bodyRows });
    doc.save('vehicles.pdf');
};

onMounted(() => { loadVehicles(); });

watch(dialog, (val) => {
    if (!val) {
        editedIndex.value = -1;
        editedVehicle.value = null;
        editedVehicleId.value = null;
    }
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

.vehicles-filter-search {
    min-width: 240px;
}

.vehicles-filter-status {
    min-width: 140px;
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
