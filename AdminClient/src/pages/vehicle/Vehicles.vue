<template>
    <div class="vehicles-container">
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Vehicles</div>
        </div>
        <v-data-table
            :headers="headers"
            :items="items"
            :loading="loading"
            class="elevation-2 vehicles-table"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="vehicles-toolbar">
                    <div class="vehicles-actions-wrap">
                        <div class="vehicles-export-group">
                            <v-btn class="vehicles-action-btn text-none" variant="outlined" color="success" prepend-icon="mdi-file-excel">
                                <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="vehicles.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="vehicles-action-btn text-none" variant="outlined" color="info" prepend-icon="mdi-file-delimited">
                                <download-excel :data="items" :fields="headersExcel" type="csv"
                                    name="vehicles.csv">CSV</download-excel>
                            </v-btn>
                            <v-btn class="vehicles-action-btn text-none" variant="outlined" color="error" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <div class="vehicles-filters-wrap">
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
                                        class="vehicles-add-btn text-none" prepend-icon="mdi-plus">
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
                    </div>
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
                <div class="d-flex align-center ga-1">
                    <v-btn icon variant="tonal" color="secondary" size="36" @click.stop="editItem(item)">
                        <v-icon size="20">mdi-pencil</v-icon>
                        <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                    </v-btn>
                    <v-btn icon variant="tonal" :color="item.isActive ? 'warning' : 'success'" size="36"
                        :loading="togglingId === item.vehicleId" @click.stop="toggleStatus(item)">
                        <v-icon size="20">{{ item.isActive ? 'mdi-close-circle-outline' : 'mdi-check-circle-outline' }}</v-icon>
                        <v-tooltip activator="parent" location="top">{{ item.isActive ? 'Deactivate' : 'Activate' }}</v-tooltip>
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

.vehicles-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.vehicles-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.vehicles-action-btn {
    border-radius: 4px !important;
}

.vehicles-filters-wrap {
    margin-left: auto;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 10px;
}

.vehicles-add-btn {
    min-height: 40px;
    border-radius: 4px !important;
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

.vehicles-toolbar :deep(.v-toolbar__content) {
    height: auto !important;
    min-height: 0 !important;
    overflow: visible !important;
}

.vehicles-toolbar :deep(.v-btn),
.vehicles-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 1024px) and (min-width: 601px) {
    .vehicles-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
    }

    .vehicles-filters-wrap {
        margin-left: 0;
        width: 100%;
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 10px;
    }

    .vehicles-filter-search,
    .vehicles-filter-status {
        width: 100%;
        min-width: 0;
    }

    .vehicles-add-btn {
        width: 100%;
        grid-column: 1 / -1;
    }
}

@media (max-width: 600px) {
    .vehicles-container {
        padding: 8px !important;
    }

    .vehicles-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        width: 100%;
    }

    .vehicles-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .vehicles-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .vehicles-filters-wrap {
        margin-left: 0;
        width: 100%;
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
    }

    .vehicles-filter-search,
    .vehicles-filter-status {
        width: 100%;
        min-width: 0;
    }

    .vehicles-add-btn {
        width: 100%;
        min-height: 44px;
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
