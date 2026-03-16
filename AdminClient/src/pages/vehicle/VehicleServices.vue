<template>
    <div class="page-container">
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Serviset e Automjeteve</div>
                <div class="page-subtitle">Historia e serviseve të automjeteve</div>
            </div>
        </div>

        <v-card class="table-card">
            <div class="filter-bar">
                <div class="export-group">
                    <v-btn variant="tonal" color="success" size="small" class="text-none" prepend-icon="mdi-file-excel">
                        <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                            name="vehicle-services.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportPdf">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-text-field
                        v-model="searchText"
                        label="Search (Plate, Description)"
                        prepend-inner-icon="mdi-magnify"
                        clearable
                        class="filter-field filter-field--search"
                        @update:model-value="handleSearch"
                    ></v-text-field>
                </div>
            </div>

            <v-data-table
                :headers="headers"
                :items="items"
                :loading="loading"
            >
            </v-data-table>
        </v-card>
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
.filter-inputs {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.filter-field--search {
    min-width: 260px;
}

@media (max-width: 960px) {
    .filter-bar {
        flex-direction: column !important;
        align-items: stretch !important;
    }

    .filter-inputs {
        width: 100%;
    }

    .filter-field--search {
        min-width: 0;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search {
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
