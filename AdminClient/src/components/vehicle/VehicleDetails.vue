<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>Detajet e Automjetit</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>

        <v-card-text class="pa-5" v-if="detailsLoading">
            <v-progress-linear indeterminate color="primary"></v-progress-linear>
        </v-card-text>

        <v-card-text class="pa-5" v-else-if="vehicle">
            <!-- Vehicle Info -->
            <h3 class="section-title mb-3">
                <v-icon size="20" class="mr-1">mdi-car</v-icon>
                Informatat e Automjetit
            </h3>
            <v-row dense>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Tabela</div>
                    <div class="detail-value">{{ vehicle.plateNumber || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Numri i Shasis</div>
                    <div class="detail-value">{{ vehicle.chassisNumber || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Marka</div>
                    <div class="detail-value">{{ vehicle.brand || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Modeli / Tipi</div>
                    <div class="detail-value">{{ vehicle.type || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Ngjyra</div>
                    <div class="detail-value">{{ vehicle.color || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Atesti Nr.</div>
                    <div class="detail-value">{{ vehicle.certificateNumber || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Data e Regjistrimit</div>
                    <div class="detail-value">{{ vehicle.registrationDate || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Data e Skadimit</div>
                    <div class="detail-value">{{ vehicle.expiryDate || '—' }}</div>
                </v-col>
                <v-col cols="12" sm="6" md="4">
                    <div class="detail-label">Statusi</div>
                    <div class="detail-value">
                        <v-chip :color="vehicle.isActive ? 'success' : 'grey'" size="small" variant="elevated">
                            {{ vehicle.isActive ? 'Aktiv' : 'Joaktiv' }}
                        </v-chip>
                    </div>
                </v-col>
            </v-row>

            <v-divider class="my-5"></v-divider>

            <!-- Service History -->
            <div class="d-flex align-center justify-space-between mb-3">
                <h3 class="section-title mb-0">
                    <v-icon size="20" class="mr-1">mdi-wrench</v-icon>
                    Historia e Serviseve
                </h3>
                <v-chip size="small" color="primary" variant="tonal">
                    {{ services.length }} {{ services.length === 1 ? 'servis' : 'servise' }}
                </v-chip>
            </div>

            <div v-if="services.length === 0" class="text-medium-emphasis text-body-2 pa-4 text-center">
                Nuk ka servise te regjistruara per kete automjet.
            </div>

            <v-table v-else density="comfortable" class="service-history-table">
                <thead>
                    <tr>
                        <th>Data</th>
                        <th>Kompania</th>
                        <th>Pershkrimi</th>
                        <th class="text-end">Shuma</th>
                        <th>Stafi</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="svc in services" :key="svc.vehicleServiceId">
                        <td class="text-no-wrap">{{ svc.serviceDate }}</td>
                        <td>{{ svc.serviceCompany }}</td>
                        <td>
                            <div class="svc-description">{{ svc.description }}</div>
                        </td>
                        <td class="text-end text-no-wrap font-weight-medium">
                            {{ svc.cost != null ? `€${Number(svc.cost).toFixed(2)}` : '' }}
                        </td>
                        <td>{{ svc.staffName || '—' }}</td>
                    </tr>
                </tbody>
                <tfoot v-if="services.length > 0">
                    <tr>
                        <td colspan="3" class="text-end font-weight-bold">Totali:</td>
                        <td class="text-end font-weight-bold text-no-wrap">€{{ totalCost.toFixed(2) }}</td>
                        <td></td>
                    </tr>
                </tfoot>
            </v-table>
        </v-card-text>

        <v-divider></v-divider>
        <v-card-actions class="pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Mbyll" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, computed, watch, onMounted } from 'vue';

const props = defineProps({
    vehicleId: { type: Number, required: true }
});

const emit = defineEmits(['close']);
const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();

const detailsLoading = ref(false);
const vehicle = ref(null);
const services = ref([]);

const totalCost = computed(() =>
    services.value.reduce((sum, s) => sum + (Number(s.cost) || 0), 0)
);

function normalizeService(row) {
    return {
        vehicleServiceId: row.vehicleServiceId ?? row.VehicleServiceId,
        serviceCompany: row.serviceCompany ?? row.ServiceCompany ?? '',
        serviceDate: row.serviceDate ?? row.ServiceDate ?? '',
        description: row.description ?? row.Description ?? '',
        cost: row.cost ?? row.Cost ?? 0,
        staffUserId: row.staffUserId ?? row.StaffUserId ?? null,
        staffName: row.staffName ?? row.StaffName ?? '',
    };
}

const loadDetails = async () => {
    detailsLoading.value = true;
    try {
        const response = await vehicleStore.getVehicleDetails(props.vehicleId);
        const d = response?.data;
        if (!d) {
            vehicle.value = null;
            services.value = [];
            return;
        }
        vehicle.value = {
            plateNumber: d.plateNumber ?? d.PlateNumber ?? '',
            chassisNumber: d.chassisNumber ?? d.ChassisNumber ?? '',
            brand: d.brand ?? d.Brand ?? '',
            type: d.type ?? d.Type ?? '',
            color: d.color ?? d.Color ?? '',
            certificateNumber: d.certificateNumber ?? d.CertificateNumber ?? '',
            registrationDate: d.registrationDate ?? d.RegistrationDate ?? '',
            expiryDate: d.expiryDate ?? d.ExpiryDate ?? '',
            isActive: d.isActive ?? d.IsActive ?? true,
        };
        const rawServices = d.services ?? d.Services ?? [];
        services.value = rawServices.map(normalizeService);
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjate ngarkimit te detajeve' });
        vehicle.value = null;
        services.value = [];
    } finally {
        detailsLoading.value = false;
    }
};

const closeDialog = () => emit('close');

watch(() => props.vehicleId, () => { if (props.vehicleId) loadDetails(); }, { immediate: true });
onMounted(() => { if (props.vehicleId) loadDetails(); });
</script>

<style scoped>
.sticky-header {
    position: sticky;
    top: 0;
    background: rgb(var(--v-theme-surface));
    z-index: 1;
}

.section-title {
    font-size: 15px;
    font-weight: 600;
    color: #334155;
    display: flex;
    align-items: center;
}

.detail-label {
    font-size: 0.75rem;
    font-weight: 500;
    color: #64748b;
    margin-bottom: 2px;
    text-transform: uppercase;
    letter-spacing: 0.03em;
}

.detail-value {
    font-size: 0.9375rem;
    font-weight: 500;
    color: #1e293b;
    padding-bottom: 12px;
}

.service-history-table {
    border: 1px solid #e2e8f0;
    border-radius: 8px;
}

.service-history-table th {
    background: #f8fafc !important;
    font-weight: 600 !important;
    font-size: 0.8125rem !important;
    color: #475569 !important;
    white-space: nowrap;
}

.service-history-table td {
    font-size: 0.875rem;
    vertical-align: top;
    padding-top: 10px !important;
    padding-bottom: 10px !important;
}

.service-history-table tfoot td {
    background: #f8fafc;
    border-top: 2px solid #e2e8f0;
}

.svc-description {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 280px;
    line-height: 1.4;
}
</style>
