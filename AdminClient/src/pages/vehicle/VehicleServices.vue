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
                                name="vehicle-services.xlsx">
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
                                label="Search (Plate, Description)"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                                class="vehicles-filter-search"
                                @update:model-value="handleSearch"
                            ></v-text-field>
                            <v-btn color="primary" variant="elevated" class="text-capitalize" prepend-icon="mdi-plus" disabled>
                                Add Service (coming soon)
                            </v-btn>
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
import { ref, onMounted } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(vehicleStore);

const headers = [
    { title: 'Vehicle', key: 'vehicleDisplay' },
    { title: 'Service Date', key: 'serviceDate' },
    { title: 'Description', key: 'description' },
    { title: 'Cost', key: 'cost' },
];

const headersExcel = {
    'Vehicle': 'vehicleDisplay',
    'Service Date': 'serviceDate',
    'Description': 'description',
    'Cost': 'cost',
};

const headersPdf = ['Vehicle', 'Service Date', 'Description', 'Cost'];

const items = ref([]);
const searchText = ref('');

let searchTimeout = null;
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => { loadServices(); }, 500);
};

function normalizeService(row) {
    if (!row || typeof row !== 'object') return null;
    const plate = row.vehiclePlate ?? row.VehiclePlate ?? '';
    const brand = row.vehicleBrand ?? row.VehicleBrand ?? '';
    return {
        id: row.vehicleServiceId ?? row.VehicleServiceId,
        vehicleDisplay: plate + (brand ? ` – ${brand}` : ''),
        serviceDate: row.serviceDate ?? row.ServiceDate ?? '',
        description: row.description ?? row.Description ?? '',
        cost: row.cost ?? row.Cost ?? 0,
    };
}

const loadServices = () => {
    vehicleStore.getVehicleServiceList(searchText.value || null)
        .then((response) => {
            const body = response?.data;
            const rawData = body?.data ?? body?.Data ?? [];
            items.value = rawData.map(normalizeService).filter(Boolean);
        })
        .catch(() => {
            items.value = [];
        });
};

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text('Vehicle Services (Serviset e Automjeteve)', 14, 10);
    const bodyRows = items.value.map((r) => [r.vehicleDisplay, r.serviceDate, r.description, r.cost]);
    autoTable(doc, { head: [headersPdf], body: bodyRows });
    doc.save('vehicle-services.pdf');
};

onMounted(() => { loadServices(); });
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
    min-width: 260px;
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
