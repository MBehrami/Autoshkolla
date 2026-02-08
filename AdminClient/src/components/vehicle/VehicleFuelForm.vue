<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>Add Fuel Entry</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="fuel-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="saveFuel">
                <h3 class="form-section-title">Fuel information (Derivatet)</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-menu v-model="fillDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="fillDateDisplay"
                                    label="Fill Date (Data e mbushjes)"
                                    :rules="[rules.required]"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    hide-details="auto"
                                    v-bind="menuProps"
                                    clearable
                                    @click:clear="clearFillDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="fillDateModel" color="primary" @update:model-value="handleFillDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-select
                            v-model="form.vehicleId"
                            :items="vehicles"
                            item-title="label"
                            item-value="vehicleId"
                            label="Vehicle (Automjeti)"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-select>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" :md="isInstructor ? 6 : 4">
                        <v-text-field
                            v-model.number="form.fuelAmount"
                            label="Fuel Amount (Shuma e derivates)"
                            :rules="[rules.required, rules.positive]"
                            type="number"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" :md="isInstructor ? 6 : 4">
                        <v-select
                            v-model="form.fuelType"
                            :items="fuelTypes"
                            label="Fuel Type (Lloji i derivatit)"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                        ></v-select>
                    </v-col>
                    <!-- Staff dropdown: Admin only -->
                    <v-col v-if="!isInstructor" cols="12" md="4">
                        <v-select
                            v-model="form.staffUserId"
                            :items="staffList"
                            item-title="label"
                            item-value="userId"
                            label="Staff (Stafi)"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-select>
                    </v-col>
                </v-row>

                <v-divider class="my-4"></v-divider>
                <div class="d-flex justify-end ga-3">
                    <v-btn variant="text" class="text-capitalize" @click="closeDialog">Cancel</v-btn>
                    <v-btn type="submit" color="primary" variant="elevated" class="text-capitalize" :loading="saving" :disabled="!valid">
                        Save
                    </v-btn>
                </div>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, computed, onMounted, nextTick } from 'vue';

const emit = defineEmits(['update:modelValue', 'saved']);

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || '';
        return role === 'Instructor';
    } catch { return false; }
});

const valid = ref(false);
const saving = ref(false);
const formRef = ref(null);
const vehicles = ref([]);
const staffList = ref([]);
const fuelTypes = ['Diesel', 'Benzin'];

const rules = {
    required: (v) => !!v || v === 0 ? true : 'This field is required',
    positive: (v) => (v !== null && v !== '' && Number(v) > 0) || 'Must be greater than 0',
};

const form = ref({
    fillDate: '',
    vehicleId: null,
    fuelAmount: null,
    fuelType: null,
    staffUserId: null,
});

// Date picker
const fillDateMenu = ref(false);
const fillDateModel = ref(null);
const fillDateDisplay = ref('');

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

const handleFillDateChange = (v) => {
    fillDateDisplay.value = formatDate(v);
    form.value.fillDate = fillDateDisplay.value;
    nextTick(() => { fillDateMenu.value = false; });
};
const clearFillDate = () => {
    fillDateDisplay.value = '';
    fillDateModel.value = null;
    form.value.fillDate = '';
};

const closeDialog = () => {
    emit('update:modelValue', false);
};

const loadDropdowns = async () => {
    try {
        // Always load vehicles
        const vRes = await vehicleStore.getVehiclesDropdown();
        const rawVehicles = vRes?.data?.data || [];
        vehicles.value = rawVehicles.map(v => ({
            vehicleId: v.vehicleId,
            label: `${v.plateNumber}${v.brand ? ' – ' + v.brand : ''}`
        }));

        // Only load staff dropdown for Admin
        if (!isInstructor.value) {
            const sRes = await vehicleStore.getStaffDropdown();
            const rawStaff = sRes?.data?.data || [];
            staffList.value = rawStaff.map(s => ({
                userId: s.userId,
                label: `${s.fullName}${s.roleName ? ' (' + s.roleName + ')' : ''}`
            }));
        }
    } catch {
        vehicles.value = [];
        staffList.value = [];
    }
};

const saveFuel = async () => {
    if (!valid.value) return;
    saving.value = true;
    try {
        // For Instructor, staffUserId is ignored by backend (auto-set server-side),
        // but we still send 0 to pass any client-side checks
        const payload = { ...form.value };
        if (isInstructor.value) {
            payload.staffUserId = 0; // backend overrides with logged-in user
        }

        const response = await vehicleStore.createVehicleFuel(payload);
        const body = response?.data;
        if (body && body.status === 'error') {
            settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'Error saving fuel entry' });
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: body?.responseMsg || 'Fuel entry saved successfully' });
        emit('saved');
        closeDialog();
    } catch (err) {
        settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error saving fuel entry' });
    } finally {
        saving.value = false;
    }
};

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
.fuel-form-body {
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
