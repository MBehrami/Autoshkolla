<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>{{ isEdit ? 'Ndrysho Servisin' : 'Regjistro Servisin e Automjetit' }}</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="service-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="saveService">
                <h3 class="form-section-title">Informatat e servisit</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-select
                            v-model="form.vehicleId"
                            :items="vehicles"
                            item-title="label"
                            item-value="vehicleId"
                            label="Automjeti *"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-select>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-text-field
                            v-model="form.serviceCompany"
                            label="Kompania / Servisi *"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                            placeholder="p.sh. Auto Servis Prishtina"
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-menu v-model="serviceDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="serviceDateDisplay"
                                    label="Data e servisit *"
                                    :rules="[rules.required]"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    hide-details="auto"
                                    v-bind="menuProps"
                                    clearable
                                    @click:clear="clearServiceDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="serviceDateModel" color="primary" @update:model-value="handleServiceDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-text-field
                            v-model.number="form.cost"
                            label="Shuma e pageses (€) *"
                            :rules="[rules.required, rules.positive]"
                            type="number"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-select
                            v-model="form.staffUserId"
                            :items="staffList"
                            item-title="label"
                            item-value="userId"
                            label="Stafi qe ka derguar automjetin *"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-select>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-textarea
                            v-model="form.description"
                            label="Pershkrimi i servisit *"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            rows="3"
                            auto-grow
                            clearable
                            placeholder="Cfare u riparua ose u servisua..."
                        ></v-textarea>
                    </v-col>
                </v-row>

                <v-alert v-if="!isEdit" type="info" variant="tonal" density="compact" class="mt-4 mb-2" icon="mdi-information-outline">
                    Shuma e pageses do te regjistrohet automatikisht si shpenzim ne Raportin Ditor.
                </v-alert>

                <v-divider class="my-4"></v-divider>
                <div class="d-flex justify-end ga-3">
                    <v-btn variant="text" class="text-capitalize" @click="closeDialog">Anulo</v-btn>
                    <v-btn type="submit" color="primary" variant="elevated" class="text-capitalize" :loading="saving" :disabled="!valid">
                        {{ isEdit ? 'Përditëso' : 'Regjistro Servisin' }}
                    </v-btn>
                </div>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, watch, onMounted, nextTick } from 'vue';

const props = defineProps({
    modelValue: Boolean,
    service: { type: Object, default: null },
    serviceId: { type: Number, default: null },
    isEdit: { type: Boolean, default: false },
});

const emit = defineEmits(['update:modelValue', 'saved']);

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();

const valid = ref(false);
const saving = ref(false);
const formRef = ref(null);
const vehicles = ref([]);
const staffList = ref([]);

const rules = {
    required: (v) => !!v || v === 0 ? true : 'Kjo fushe eshte e detyrueshme',
    positive: (v) => (v !== null && v !== '' && Number(v) > 0) || 'Duhet te jete me e madhe se 0',
};

const defaultForm = () => ({
    vehicleId: null,
    serviceCompany: '',
    serviceDate: '',
    cost: null,
    description: '',
    staffUserId: null,
});

const form = ref(defaultForm());

const serviceDateMenu = ref(false);
const serviceDateModel = ref(null);
const serviceDateDisplay = ref('');

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

const handleServiceDateChange = (v) => {
    serviceDateDisplay.value = formatDate(v);
    form.value.serviceDate = serviceDateDisplay.value;
    nextTick(() => { serviceDateMenu.value = false; });
};

const clearServiceDate = () => {
    serviceDateDisplay.value = '';
    serviceDateModel.value = null;
    form.value.serviceDate = '';
};

const closeDialog = () => {
    emit('update:modelValue', false);
};

const resetForm = () => {
    form.value = defaultForm();
    serviceDateDisplay.value = '';
    serviceDateModel.value = null;
};

const loadDropdowns = async () => {
    try {
        const vRes = await vehicleStore.getVehiclesDropdown();
        const rawVehicles = vRes?.data?.data || [];
        vehicles.value = rawVehicles.map(v => ({
            vehicleId: v.vehicleId,
            label: `${v.plateNumber}${v.brand ? ' – ' + v.brand : ''}`
        }));

        const sRes = await vehicleStore.getStaffDropdown();
        const rawStaff = sRes?.data?.data || [];
        staffList.value = rawStaff.map(s => ({
            userId: s.userId,
            label: `${s.fullName}${s.roleName ? ' (' + s.roleName + ')' : ''}`
        }));
    } catch {
        vehicles.value = [];
        staffList.value = [];
    }
};

const saveService = async () => {
    if (!valid.value) return;
    saving.value = true;
    try {
        let response;
        if (props.isEdit && props.serviceId) {
            response = await vehicleStore.updateVehicleService(props.serviceId, { ...form.value });
        } else {
            response = await vehicleStore.createVehicleService({ ...form.value });
        }
        const body = response?.data;
        if (body && body.status === 'error') {
            settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'Gabim gjate ruajtjes se servisit' });
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: body?.responseMsg || 'Servisi u ruajt me sukses' });
        resetForm();
        emit('saved');
        closeDialog();
    } catch (err) {
        settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Gabim gjate ruajtjes se servisit' });
    } finally {
        saving.value = false;
    }
};

watch(() => props.service, (val) => {
    if (val && props.isEdit) {
        form.value = {
            vehicleId: val.vehicleId ?? null,
            serviceCompany: val.serviceCompany ?? '',
            serviceDate: val.serviceDate ?? '',
            cost: val.cost ?? null,
            description: val.description ?? '',
            staffUserId: val.staffUserId ?? null,
        };
        serviceDateDisplay.value = val.serviceDate ?? '';
    } else if (!props.isEdit) {
        resetForm();
    }
}, { immediate: true });

onMounted(() => {
    loadDropdowns();
});
</script>

<style scoped>
.sticky-header {
    position: sticky;
    top: 0;
    background: rgb(var(--v-theme-surface));
    z-index: 1;
}
.service-form-body {
    padding: 24px;
}
.form-section-title {
    font-size: 15px;
    font-weight: 600;
    color: #424242;
    margin-bottom: 16px;
}
.form-row {
    margin-bottom: 4px;
}
</style>
