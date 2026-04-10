<template>
    <div class="page-container">
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3">
            <div>
                <div class="page-title">Serviset e Automjeteve</div>
                <div class="page-subtitle">Historia e serviseve të automjeteve</div>
            </div>
            <v-dialog v-model="dialog" max-width="900" scrollable>
                <template v-slot:activator="{ props: activatorProps }">
                    <v-btn v-bind="activatorProps" color="primary" variant="flat"
                        class="text-none" prepend-icon="mdi-plus" size="default">
                        Regjistro Servis
                    </v-btn>
                </template>
                <VehicleServiceForm
                    v-model="dialog"
                    :service="editedService"
                    :service-id="editedServiceId"
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
                            name="vehicle-services.xlsx">Excel</download-excel>
                    </v-btn>
                    <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                        @click.stop="exportPdf">PDF</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="filter-inputs">
                    <v-select
                        v-model="filterVehicleId"
                        :items="vehiclesDropdown"
                        item-title="label"
                        item-value="vehicleId"
                        label="Filtro sipas automjetit"
                        clearable
                        class="filter-field filter-field--vehicle"
                        @update:model-value="loadServices"
                    ></v-select>
                    <v-text-field
                        v-model="searchText"
                        label="Kerko (Targa, Kompania, Pershkrimi, Stafi)"
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
                class="services-table"
            >
                <template v-slot:item.cost="{ item }">
                    <span class="font-weight-medium">{{ item.cost != null ? `€${Number(item.cost).toFixed(2)}` : '' }}</span>
                </template>
                <template v-slot:item.description="{ item }">
                    <div class="description-cell" :title="item.description">{{ item.description }}</div>
                </template>
                <template v-slot:item.actions="{ item }">
                    <v-btn icon variant="text" color="secondary" size="small" @click.stop="editItem(item)">
                        <v-icon size="18">mdi-pencil-outline</v-icon>
                        <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                    </v-btn>
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
import VehicleServiceForm from '@/components/vehicle/VehicleServiceForm.vue';

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(vehicleStore);

const dialog = ref(false);
const editedIndex = ref(-1);
const editedService = ref(null);
const editedServiceId = ref(null);

const headers = [
    { title: 'Automjeti', key: 'vehicleDisplay', minWidth: '140px' },
    { title: 'Kompania / Servisi', key: 'serviceCompany', minWidth: '150px' },
    { title: 'Data', key: 'serviceDate', width: '115px' },
    { title: 'Shuma', key: 'cost', width: '100px', align: 'end' },
    { title: 'Pershkrimi', key: 'description', minWidth: '200px' },
    { title: 'Stafi', key: 'staffName', minWidth: '140px' },
    { title: 'Veprimet', key: 'actions', sortable: false, width: '80px', align: 'center' },
];

const headersExcel = {
    'Automjeti': 'vehicleDisplay',
    'Kompania / Servisi': 'serviceCompany',
    'Data': 'serviceDate',
    'Shuma': 'cost',
    'Pershkrimi': 'description',
    'Stafi': 'staffName',
};

const headersPdf = ['Automjeti', 'Kompania', 'Data', 'Shuma', 'Pershkrimi', 'Stafi'];

const items = ref([]);
const searchText = ref('');
const filterVehicleId = ref(null);
const vehiclesDropdown = ref([]);

let searchTimeout = null;
const handleSearch = () => {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => { loadServices(); }, 500);
};

const handleSaved = () => {
    dialog.value = false;
    editedIndex.value = -1;
    editedService.value = null;
    editedServiceId.value = null;
    loadServices();
};

const editItem = (item) => {
    editedIndex.value = items.value.indexOf(item);
    editedService.value = { ...item };
    editedServiceId.value = item.vehicleServiceId;
    dialog.value = true;
};

function normalizeService(row) {
    if (!row || typeof row !== 'object') return null;
    const plate = row.vehiclePlate ?? row.VehiclePlate ?? '';
    const brand = row.vehicleBrand ?? row.VehicleBrand ?? '';
    return {
        vehicleServiceId: row.vehicleServiceId ?? row.VehicleServiceId,
        vehicleId: row.vehicleId ?? row.VehicleId,
        vehicleDisplay: plate + (brand ? ` – ${brand}` : ''),
        serviceCompany: row.serviceCompany ?? row.ServiceCompany ?? '',
        serviceDate: row.serviceDate ?? row.ServiceDate ?? '',
        description: row.description ?? row.Description ?? '',
        cost: row.cost ?? row.Cost ?? 0,
        staffUserId: row.staffUserId ?? row.StaffUserId ?? null,
        staffName: row.staffName ?? row.StaffName ?? '',
    };
}

const loadServices = () => {
    vehicleStore.getVehicleServiceList(
        searchText.value || null,
        filterVehicleId.value || null
    )
        .then((response) => {
            const body = response?.data;
            const rawData = body?.data ?? body?.Data ?? [];
            items.value = rawData.map(normalizeService).filter(Boolean);
        })
        .catch(() => {
            items.value = [];
        });
};

const loadVehiclesDropdown = async () => {
    try {
        const vRes = await vehicleStore.getVehiclesDropdown();
        const raw = vRes?.data?.data || [];
        vehiclesDropdown.value = raw.map(v => ({
            vehicleId: v.vehicleId,
            label: `${v.plateNumber}${v.brand ? ' – ' + v.brand : ''}`
        }));
    } catch {
        vehiclesDropdown.value = [];
    }
};

const exportPdf = () => {
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text('Serviset e Automjeteve', 14, 10);
    const bodyRows = items.value.map((r) => [
        r.vehicleDisplay,
        r.serviceCompany,
        r.serviceDate,
        r.cost != null ? `€${Number(r.cost).toFixed(2)}` : '',
        r.description,
        r.staffName
    ]);
    autoTable(doc, { head: [headersPdf], body: bodyRows });
    doc.save('vehicle-services.pdf');
};

onMounted(() => {
    loadServices();
    loadVehiclesDropdown();
});

watch(dialog, (val) => {
    if (!val) {
        editedIndex.value = -1;
        editedService.value = null;
        editedServiceId.value = null;
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
    min-width: 280px;
}

.filter-field--vehicle {
    min-width: 200px;
    max-width: 260px;
}

.services-table :deep(th) {
    white-space: nowrap !important;
    font-weight: 600 !important;
}

.description-cell {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 320px;
    line-height: 1.4;
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
    .filter-field--vehicle {
        min-width: 0;
        max-width: none;
        flex: 1;
    }
}

@media (max-width: 600px) {
    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--search,
    .filter-field--vehicle {
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
