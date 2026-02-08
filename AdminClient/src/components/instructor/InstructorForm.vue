<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>{{ isEdit ? 'Edit Instructor' : 'Add Instructor' }}</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="instructor-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="saveInstructor">
                <h3 class="form-section-title">Personal information</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.firstName"
                            label="First Name"
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
                            v-model="form.parentName"
                            label="Parent Name"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.lastName"
                            label="Last Name"
                            :rules="[rules.required]"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-menu v-model="dobMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="dobDisplay"
                                    label="Date of Birth (dd.MM.yyyy)"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    :rules="[rules.required]"
                                    hide-details="auto"
                                    clearable
                                    required
                                    v-bind="menuProps"
                                    @click:clear="clearDob"
                                ></v-text-field>
                            </template>
                            <v-date-picker
                                v-model="dobModel"
                                color="primary"
                                @update:model-value="handleDobChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.personalNumber"
                            label="Personal Number (max 10 digits)"
                            :rules="[rules.required, rules.personalNumber]"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.phoneNumber"
                            label="Phone Number"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>

                <h3 class="form-section-title">Account & schedule</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.email"
                            label="Email (used for login)"
                            :rules="[rules.required]"
                            variant="outlined"
                            type="email"
                            :disabled="!!isEdit"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4" v-if="!isEdit">
                        <v-text-field
                            v-model="form.initialPassword"
                            label="Initial Password"
                            :rules="isEdit ? [] : [rules.required, rules.password]"
                            variant="outlined"
                            type="password"
                            clearable
                            :required="!isEdit"
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-select
                            v-model="form.scheduleType"
                            :items="scheduleOptions"
                            label="Schedule Type"
                            variant="outlined"
                            clearable
                        ></v-select>
                    </v-col>
                </v-row>

                <h3 class="form-section-title">License</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="form.licenseNumber"
                            label="License Number"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-menu v-model="licenseValidityMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="licenseValidityDisplay"
                                    label="License Validity (dd.MM.yyyy)"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    hide-details="auto"
                                    v-bind="menuProps"
                                    @click:clear="clearLicenseValidity"
                                ></v-text-field>
                            </template>
                            <v-date-picker
                                v-model="licenseValidityModel"
                                color="primary"
                                @update:model-value="handleLicenseValidityChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            :model-value="form.licensePhotoPath ? 'File selected' : ''"
                            label="License Photo"
                            variant="outlined"
                            readonly
                            hint="Upload via Settings or use path"
                        ></v-text-field>
                        <input type="file" ref="licenseFileRef" accept="image/*" class="d-none" @change="onLicenseFileChange" />
                        <v-btn size="small" variant="outlined" class="mt-1" @click="triggerLicenseUpload">Choose file</v-btn>
                    </v-col>
                </v-row>
            </v-form>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="sticky-footer pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Cancel" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
            <v-btn :disabled="!valid" :loading="loading" text="Save" color="grey-darken-4" class="text-capitalize" @click="saveInstructor"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useInstructorStore } from '@/store/InstructorStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, watch, onMounted, nextTick } from 'vue';
import API from '@/store/API';

const props = defineProps({
    modelValue: Boolean,
    instructor: Object,
    isEdit: Boolean,
    instructorId: Number
})

const emit = defineEmits(['update:modelValue', 'saved'])
const instructorStore = useInstructorStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(instructorStore)

const valid = ref(false)
const formRef = ref(null)
const licenseFileRef = ref(null)
const scheduleOptions = ['Full-time', 'Part-time']

const dobMenu = ref(false)
const dobModel = ref(null)
const dobDisplay = ref('')
const licenseValidityMenu = ref(false)
const licenseValidityModel = ref(null)
const licenseValidityDisplay = ref('')

const form = ref({
    firstName: '',
    parentName: '',
    lastName: '',
    dateOfBirth: '',
    personalNumber: '',
    phoneNumber: '',
    email: '',
    initialPassword: '',
    scheduleType: '',
    licenseNumber: '',
    licenseValidityDate: '',
    licensePhotoPath: ''
})

const rules = {
    required: (v) => !!v || 'Required',
    personalNumber: (v) => {
        if (!v) return true
        return /^\d{1,10}$/.test(v) || 'Numbers only, max 10 digits'
    },
    password: (v) => (v && v.length >= 6) || 'At least 6 characters'
}

// Convert dd.MM.yyyy (or Date) to YYYY-MM-DD for v-date-picker model
function parseDate(dateStr) {
    if (!dateStr) return null
    if (typeof dateStr === 'object' && dateStr instanceof Date) {
        const d = dateStr
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    }
    const parts = String(dateStr).trim().split('.')
    if (parts.length === 3) {
        return `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`
    }
    const iso = String(dateStr).split('T')[0]
    if (iso && /^\d{4}-\d{2}-\d{2}$/.test(iso)) return iso
    return null
}

// Convert to dd.MM.yyyy for display and backend - accepts Date, ISO string, or dd.MM.yyyy
function formatDate(value) {
    if (!value) return ''
    if (value instanceof Date) {
        return `${String(value.getDate()).padStart(2, '0')}.${String(value.getMonth() + 1).padStart(2, '0')}.${value.getFullYear()}`
    }
    const str = String(value).trim()
    const isoPart = str.split('T')[0]
    const parts = isoPart.split('-')
    if (parts.length === 3 && parts[0].length === 4) {
        return `${parts[2]}.${parts[1]}.${parts[0]}`
    }
    if (str.includes('.')) return str
    return str
}

const clearDob = () => { form.value.dateOfBirth = ''; dobDisplay.value = ''; dobModel.value = null }
const clearLicenseValidity = () => { form.value.licenseValidityDate = ''; licenseValidityDisplay.value = ''; licenseValidityModel.value = null }

const handleDobChange = (value) => {
    if (value) {
        const formatted = formatDate(value)
        form.value.dateOfBirth = formatted
        dobDisplay.value = formatted
        dobModel.value = typeof value === 'string' ? value : parseDate(formatted)
    } else {
        form.value.dateOfBirth = ''
        dobDisplay.value = ''
        dobModel.value = null
    }
    nextTick(() => { dobMenu.value = false })
}

const handleLicenseValidityChange = (value) => {
    if (value) {
        const formatted = formatDate(value)
        form.value.licenseValidityDate = formatted
        licenseValidityDisplay.value = formatted
        licenseValidityModel.value = typeof value === 'string' ? value : parseDate(formatted)
    } else {
        form.value.licenseValidityDate = ''
        licenseValidityDisplay.value = ''
        licenseValidityModel.value = null
    }
    nextTick(() => { licenseValidityMenu.value = false })
}

const triggerLicenseUpload = () => { licenseFileRef.value?.click() }

const onLicenseFileChange = (e) => {
    const file = e.target?.files?.[0]
    if (!file) return
    const fd = new FormData()
    fd.append('file', file)
    API.post(import.meta.env.VITE_API_URL + '/api/Users/Upload', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
        .then((res) => {
            const path = res.data?.dbPath || res.data
            if (path) form.value.licensePhotoPath = path
        })
        .catch(() => settingStore.toggleSnackbar({ status: true, msg: 'Upload failed' }))
}

const saveInstructor = async () => {
    const payload = { ...form.value }
    if (props.isEdit) {
        delete payload.initialPassword
        await instructorStore.updateInstructor(props.instructorId, payload)
    } else {
        await instructorStore.createInstructor(payload)
    }
    settingStore.toggleSnackbar({ status: true, msg: 'Successfully Saved' })
    emit('saved')
    closeDialog()
}

const closeDialog = () => {
    emit('update:modelValue', false)
    form.value = {
        firstName: '', parentName: '', lastName: '', dateOfBirth: '', personalNumber: '',
        phoneNumber: '', email: '', initialPassword: '', scheduleType: '', licenseNumber: '',
        licenseValidityDate: '', licensePhotoPath: ''
    }
    dobDisplay.value = ''; dobModel.value = null
    licenseValidityDisplay.value = ''; licenseValidityModel.value = null
}

watch(() => props.instructor, (val) => {
    if (val && props.isEdit) {
        form.value = {
            firstName: val.firstName ?? '',
            parentName: val.parentName ?? '',
            lastName: val.lastName ?? '',
            dateOfBirth: val.dateOfBirth ?? '',
            personalNumber: val.personalNumber ?? '',
            phoneNumber: val.phoneNumber ?? '',
            email: val.email ?? '',
            initialPassword: '',
            scheduleType: val.scheduleType ?? '',
            licenseNumber: val.licenseNumber ?? '',
            licenseValidityDate: val.licenseValidityDate ?? '',
            licensePhotoPath: val.licensePhotoPath ?? ''
        }
        dobDisplay.value = form.value.dateOfBirth
        dobModel.value = parseDate(form.value.dateOfBirth)
        licenseValidityDisplay.value = form.value.licenseValidityDate
        licenseValidityModel.value = parseDate(form.value.licenseValidityDate)
    }
}, { immediate: true, deep: true })

// Sync date picker model when menu opens so picker shows current value
watch(dobMenu, (isOpen) => {
    if (isOpen && form.value.dateOfBirth) {
        dobModel.value = parseDate(form.value.dateOfBirth)
    }
})

watch(licenseValidityMenu, (isOpen) => {
    if (isOpen && form.value.licenseValidityDate) {
        licenseValidityModel.value = parseDate(form.value.licenseValidityDate)
    }
})

onMounted(() => {})
</script>

<style scoped>
.sticky-header {
    position: sticky;
    top: 0;
    z-index: 10;
    background: white;
    padding: 16px 24px;
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

.sticky-footer {
    position: sticky;
    bottom: 0;
    z-index: 10;
    background: white;
    padding: 16px 24px;
    border-top: 1px solid rgba(0, 0, 0, 0.12);
}

.instructor-form-body {
    max-height: 70vh;
    overflow-y: auto;
    padding: 24px 28px;
}

.form-section-title {
    font-size: 0.95rem;
    font-weight: 600;
    color: rgba(0, 0, 0, 0.7);
    margin: 20px 0 12px 0;
    padding-bottom: 6px;
    border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.form-section-title:first-child { margin-top: 0; }

.form-row { margin: 0 -6px; }

.form-row :deep(.v-col) { padding: 6px 12px; }
</style>
