<template>
    <div class="page-container">
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Automjetet</div>
                <div class="page-subtitle">Menaxhoni listën e automjeteve</div>
            </div>
            <v-dialog v-model="dialog" max-width="900" scrollable>
                <template v-slot:activator="{ props: activatorProps }">
                    <v-btn v-bind="activatorProps" color="primary" variant="flat"
                        class="text-none" prepend-icon="mdi-plus" size="default">
                        Regjistro Automjet
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

        <v-card class="table-card">
            <div class="filter-bar">
                <div class="export-group">
                    <v-btn variant="tonal" color="success" size="small" class="text-none" prepend-icon="mdi-file-excel">
                        <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                            name="vehicles.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportPdf">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-text-field
                        v-model="searchText"
                        label="Kërko (Tabelat ose Modelin)"
                        prepend-inner-icon="mdi-magnify"
                        clearable
                        class="filter-field filter-field--search"
                        @update:model-value="handleSearch"
                    ></v-text-field>
                    <v-select
                        v-model="statusFilter"
                        :items="statusOptions"
                        item-title="label"
                        item-value="value"
                        label="Statusi"
                        clearable
                        class="filter-field filter-field--status"
                        @update:model-value="loadVehicles"
                    ></v-select>
                </div>
            </div>

            <v-data-table
                :headers="headers"
                :items="items"
                :loading="loading"
            >
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
                        <v-btn icon variant="text" color="secondary" class="action-btn" @click.stop="editItem(item)">
                            <v-icon size="18">mdi-pencil-outline</v-icon>
                            <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                        </v-btn>
                        <v-btn icon variant="text" :color="item.isActive ? 'warning' : 'success'" class="action-btn"
                            :loading="togglingId === item.vehicleId" @click.stop="toggleStatus(item)">
                            <v-icon size="18">{{ item.isActive ? 'mdi-close-circle-outline' : 'mdi-check-circle-outline' }}</v-icon>
                            <v-tooltip activator="parent" location="top">{{ item.isActive ? 'Deactivate' : 'Activate' }}</v-tooltip>
                        </v-btn>
                    </div>
                </template>
            </v-data-table>
        </v-card>
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
    { title: 'Tabela', key: 'plateNumber' },
    { title: 'Modeli', key: 'type' },
    { title: 'Marka', key: 'brand' },
    { title: 'Data e Regjistrimit', key: 'registrationDate' },
    { title: 'Data e Skadimit', key: 'expiryDate' },
    { title: 'Ngjyra', key: 'color' },
    { title: 'Atesti Nr.', key: 'certificateNumber' },
    { title: 'Statusi', key: 'statusDisplay', sortable: false },
    { title: 'Veprimet', key: 'actions', sortable: false }
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
.filter-inputs {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.filter-field--search {
    min-width: 240px;
}

.filter-field--status {
    min-width: 140px;
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
    .filter-field--status {
        min-width: 0;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search,
    .filter-field--status {
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
