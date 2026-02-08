<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>{{ isEdit ? 'Edit Vehicle' : 'Add Vehicle' }}</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="vehicle-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="saveVehicle">
                <h3 class="form-section-title">Vehicle information</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.plateNumber"
                            label="Plate Number (Nr. i Tabelave)"
                            :rules="[rules.required]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.chassisNumber"
                            label="Chassis Number (Nr. i Shasis)"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.color"
                            label="Color (Ngjyra)"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.type"
                            label="Type (Tipi)"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.brand"
                            label="Brand (Marka)"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.certificateNumber"
                            label="Certificate Number (Nr. i Atestit)"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-menu v-model="regDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="regDateDisplay"
                                    label="Registration Date (Data e regjistrimit)"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    hide-details="auto"
                                    v-bind="menuProps"
                                    clearable
                                    @click:clear="clearRegDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="regDateModel" color="primary" @update:model-value="handleRegDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-menu v-model="expDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="expDateDisplay"
                                    label="Expiry Date (Data e skadimit)"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    hide-details="auto"
                                    v-bind="menuProps"
                                    clearable
                                    @click:clear="clearExpDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="expDateModel" color="primary" @update:model-value="handleExpDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>

                <v-divider class="my-4"></v-divider>
                <div class="d-flex justify-end ga-3">
                    <v-btn variant="text" class="text-capitalize" @click="closeDialog">Cancel</v-btn>
                    <v-btn type="submit" color="primary" variant="elevated" class="text-capitalize" :loading="saving" :disabled="!valid">
                        {{ isEdit ? 'Update' : 'Save' }}
                    </v-btn>
                </div>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script setup>
import { useVehicleStore } from '@/store/VehicleStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, watch, nextTick } from 'vue';

const props = defineProps({
    modelValue: Boolean,
    vehicle: { type: Object, default: null },
    vehicleId: { type: Number, default: null },
    isEdit: { type: Boolean, default: false }
});
const emit = defineEmits(['update:modelValue', 'saved']);

const vehicleStore = useVehicleStore();
const settingStore = useSettingStore();

const valid = ref(false);
const saving = ref(false);
const formRef = ref(null);

const rules = {
    required: (v) => !!v || 'This field is required',
};

const defaultForm = () => ({
    plateNumber: '',
    chassisNumber: '',
    color: '',
    type: '',
    brand: '',
    registrationDate: '',
    expiryDate: '',
    certificateNumber: ''
});

const form = ref(defaultForm());

// Date pickers
const regDateMenu = ref(false);
const regDateModel = ref(null);
const regDateDisplay = ref('');
const expDateMenu = ref(false);
const expDateModel = ref(null);
const expDateDisplay = ref('');

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

const handleRegDateChange = (v) => {
    regDateDisplay.value = formatDate(v);
    form.value.registrationDate = regDateDisplay.value;
    nextTick(() => { regDateMenu.value = false; });
};
const clearRegDate = () => {
    regDateDisplay.value = '';
    regDateModel.value = null;
    form.value.registrationDate = '';
};
const handleExpDateChange = (v) => {
    expDateDisplay.value = formatDate(v);
    form.value.expiryDate = expDateDisplay.value;
    nextTick(() => { expDateMenu.value = false; });
};
const clearExpDate = () => {
    expDateDisplay.value = '';
    expDateModel.value = null;
    form.value.expiryDate = '';
};

const closeDialog = () => {
    emit('update:modelValue', false);
};

const saveVehicle = async () => {
    if (!valid.value) return;
    saving.value = true;
    try {
        let response;
        if (props.isEdit && props.vehicleId) {
            response = await vehicleStore.updateVehicle(props.vehicleId, form.value);
        } else {
            response = await vehicleStore.createVehicle(form.value);
        }
        const body = response?.data;
        if (body && body.status === 'error') {
            settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'Error saving vehicle' });
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: body?.responseMsg || 'Vehicle saved successfully' });
        emit('saved');
        closeDialog();
    } catch (err) {
        settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error saving vehicle' });
    } finally {
        saving.value = false;
    }
};

// Populate form when editing
watch(() => props.vehicle, (val) => {
    if (val && props.isEdit) {
        form.value = {
            plateNumber: val.plateNumber || '',
            chassisNumber: val.chassisNumber || '',
            color: val.color || '',
            type: val.type || '',
            brand: val.brand || '',
            registrationDate: val.registrationDate || '',
            expiryDate: val.expiryDate || '',
            certificateNumber: val.certificateNumber || ''
        };
        regDateDisplay.value = val.registrationDate || '';
        expDateDisplay.value = val.expiryDate || '';
    } else {
        form.value = defaultForm();
        regDateDisplay.value = '';
        expDateDisplay.value = '';
        regDateModel.value = null;
        expDateModel.value = null;
    }
}, { immediate: true });
</script>

<style scoped>
.sticky-header {
    position: sticky;
    top: 0;
    background: rgb(var(--v-theme-surface));
    z-index: 1;
}
.vehicle-form-body {
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
