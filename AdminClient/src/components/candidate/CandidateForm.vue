<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>{{ isEdit ? 'Ndrysho të dhënat e kandidatit' : 'Regjistro kandidatë' }}</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="candidate-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="saveCandidate">
                <!-- Personal info -->
                <h3 class="form-section-title">Të dhënat personale</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.serialNumber"
                            label="Nr. Rendor"
                            :rules="[rules.serialNumber]"
                            variant="outlined"
                            density="comfortable"
                            hide-details="auto"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.firstName"
                            label="Emri"
                            :rules="[rules.required]"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.parentName"
                            label="Emri i prindit"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.lastName"
                            label="Mbiemri"
                            :rules="[rules.required]"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-menu v-model="dateOfBirthMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    :model-value="dateOfBirthDisplay"
                                    label="Data e lindjes (dd.MM.yyyy)"
                                    variant="outlined"
                                    density="comfortable"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    :rules="[rules.required]"
                                    clearable
                                    required
                                    v-bind="menuProps"
                                    @click:clear="clearDateOfBirth"
                                ></v-text-field>
                            </template>
                            <v-date-picker 
                                v-model="dateOfBirthModel" 
                                color="primary"
                                @update:model-value="handleDateOfBirthChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.personalNumber"
                            label="Personal Number"
                            :rules="[rules.required, rules.personalNumber]"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                </v-row>
                <!-- Contact -->
                <h3 class="form-section-title">Numri i telefonit & adresa</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.phoneNumber"
                            label="Numri i telefonit"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.placeOfBirth"
                            label="Vendi i lindjes"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model="candidateForm.address"
                            label="Adresa"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                </v-row>
                <!-- Category, instructor, vehicle -->
                <h3 class="form-section-title">Të dhënat tjera</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="4">
                        <v-select
                            v-model="categorySelect"
                            :items="categories"
                            item-title="categoryName"
                            item-value="categoryId"
                            label="Kategoria"
                            :rules="[rules.required]"
                            variant="outlined"
                            return-object
                            clearable
                            required
                        ></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-select
                            v-model="instructorSelect"
                            :items="instructors"
                            item-title="fullName"
                            item-value="userId"
                            label="Instruktori"
                            variant="outlined"
                            return-object
                            clearable
                        ></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-select
                            v-model="candidateForm.vehicleType"
                            :items="['Manual', 'Automatic']"
                            label="Lloji i vetures"
                            variant="outlined"
                            clearable
                        ></v-select>
                    </v-col>
                </v-row>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-select
                            v-model="candidateForm.paymentMethod"
                            :items="['Bank', 'Cash']"
                            label="Metoda e pagesës"
                            variant="outlined"
                            clearable
                        ></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model.number="candidateForm.practicalHours"
                            label="Orët praktike"
                            type="number"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field
                            v-model.number="candidateForm.totalServiceAmount"
                            label="Pagesa e shërbimit"
                            :rules="[rules.required]"
                            type="number"
                            variant="outlined"
                            clearable
                            required
                        ></v-text-field>
                    </v-col>
                </v-row>

                <!-- Installments -->
                <h3 class="form-section-title">Pagesat</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field
                            v-model.number="installments[0].amount"
                            label="Pagesa - Kësti 1"
                            type="number"
                            variant="outlined"
                            :disabled="installmentsDisabled[0]"
                            clearable
                            @update:model-value="validateInstallments"
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="installment1DateMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    v-model="installment1DateDisplay"
                                    label="Data e pagesës - Kësti 1"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    v-bind="menuProps"
                                    variant="outlined"
                                    density="comfortable"
                                    :disabled="installmentsDisabled[0]"
                                    clearable
                                ></v-text-field>
                            </template>
                            <v-date-picker 
                                v-model="installment1DateModel" 
                                color="primary"
                                @update:model-value="handleInstallment1DateChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field
                            v-model.number="installments[1].amount"
                            label="Pagesa - Kësti 2"
                            type="number"
                            variant="outlined"
                            :disabled="installmentsDisabled[1]"
                            clearable
                            @update:model-value="validateInstallments"
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="installment2DateMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    v-model="installment2DateDisplay"
                                    label="Data e pagesës - Kësti 2"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    v-bind="menuProps"
                                    variant="outlined"
                                    density="comfortable"
                                    :disabled="installmentsDisabled[1]"
                                    clearable
                                ></v-text-field>
                            </template>
                            <v-date-picker 
                                v-model="installment2DateModel" 
                                color="primary"
                                @update:model-value="handleInstallment2DateChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field
                            v-model.number="installments[2].amount"
                            label="Pagesa - Kësti 3"
                            type="number"
                            variant="outlined"
                            :disabled="installmentsDisabled[2]"
                            clearable
                            @update:model-value="validateInstallments"
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="installment3DateMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    v-model="installment3DateDisplay"
                                    label="Data e pagesës - Kësti 3"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    v-bind="menuProps"
                                    variant="outlined"
                                    density="comfortable"
                                    :disabled="installmentsDisabled[2]"
                                    clearable
                                ></v-text-field>
                            </template>
                            <v-date-picker 
                                v-model="installment3DateModel" 
                                color="primary"
                                @update:model-value="handleInstallment3DateChange"
                            ></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row v-if="installmentError" class="form-row">
                    <v-col cols="12">
                        <v-alert type="error" density="compact">{{ installmentError }}</v-alert>
                    </v-col>
                </v-row>

                <!-- Additional Payments -->
                <h3 class="form-section-title">Pagesa tjera</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="3">
                        <v-text-field
                            v-model.number="candidateForm.docWithdrawalAmount"
                            label="Pagesa e terheqjes së dokumentëve"
                            type="number"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-menu v-model="docWithdrawalDateMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    v-model="docWithdrawalDateDisplay"
                                    label="Data e pagesës së terheqjes së dokumentëve"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    v-bind="menuProps"
                                    variant="outlined"
                                    density="comfortable"
                                    clearable
                                    @click:clear="clearDocWithdrawalDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="docWithdrawalDateModel" color="primary"
                                @update:model-value="handleDocWithdrawalDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-text-field
                            v-model.number="candidateForm.drivingPaymentAmount"
                            label="Pagesa e vozitjes"
                            type="number"
                            variant="outlined"
                            clearable
                        ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-menu v-model="drivingPaymentDateMenu" :close-on-content-click="false"
                            location="bottom" transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field
                                    v-model="drivingPaymentDateDisplay"
                                    label="Data e pagesës së vozitjes"
                                    prepend-inner-icon="mdi-calendar"
                                    readonly
                                    v-bind="menuProps"
                                    variant="outlined"
                                    density="comfortable"
                                    clearable
                                    @click:clear="clearDrivingPaymentDate"
                                ></v-text-field>
                            </template>
                            <v-date-picker v-model="drivingPaymentDateModel" color="primary"
                                @update:model-value="handleDrivingPaymentDateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>

                <!-- Available Schedule / Free Hours -->
                <h3 class="form-section-title">Shënime shtesë</h3>
                <v-row class="form-row">
                    <v-col cols="12">
                        <v-textarea
                            v-model="candidateForm.availableSchedule"
                            label="Shënime shtesë rreth orarit të lirë të kandidatit"
                            placeholder="psh. Hënë 14:00–18:00, shtunë 14:00–18:00, javën e parë 14:00–18:00, javën e dytë 14:00–18:00"
                            variant="outlined"
                            rows="3"
                            auto-grow
                            hide-details="auto"
                            clearable
                        ></v-textarea>
                    </v-col>
                </v-row>
            </v-form>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="sticky-footer pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Cancel" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
            <v-btn :disabled="!valid || !!installmentError" :loading="loading" text="Regjistro" color="grey-darken-4"
                class="text-capitalize" @click="saveCandidate"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, watch, onMounted, computed, nextTick } from 'vue';
import { useDate } from 'vuetify';

const props = defineProps({
    modelValue: Boolean,
    candidate: Object,
    isEdit: Boolean,
    candidateId: Number
})

const emit = defineEmits(['update:modelValue', 'saved'])

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)
const date = useDate()

const valid = ref(false)
const formRef = ref(null)
const categories = ref([])
const instructors = ref([])
const categorySelect = ref(null)
const instructorSelect = ref(null)
const installmentError = ref('')
const installmentsDisabled = ref([false, false, false])

// Date picker states
const dateOfBirthMenu = ref(false)
const dateOfBirthModel = ref(null)
const dateOfBirthDisplay = ref('')
const installment1DateMenu = ref(false)
const installment1DateModel = ref(null)
const installment1DateDisplay = ref('')
const installment2DateMenu = ref(false)
const installment2DateModel = ref(null)
const installment2DateDisplay = ref('')
const installment3DateMenu = ref(false)
const installment3DateModel = ref(null)
const installment3DateDisplay = ref('')

const docWithdrawalDateMenu = ref(false)
const docWithdrawalDateModel = ref(null)
const docWithdrawalDateDisplay = ref('')
const drivingPaymentDateMenu = ref(false)
const drivingPaymentDateModel = ref(null)
const drivingPaymentDateDisplay = ref('')

// Convert date from dd.MM.yyyy to YYYY-MM-DD for date picker model
const parseDate = (dateStr) => {
    if (!dateStr) return null
    if (typeof dateStr === 'object' && dateStr instanceof Date) {
        const d = dateStr
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    }
    const parts = String(dateStr).split('.')
    if (parts.length === 3) {
        return `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`
    }
    return null
}

// Convert to dd.MM.yyyy - accepts string (YYYY-MM-DD or ISO) or Date
const formatDate = (value) => {
    if (!value) return ''
    let str = value
    if (value instanceof Date) {
        str = `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`
    }
    str = String(str)
    const parts = str.split('T')[0].split('-')
    if (parts.length === 3) {
        return `${parts[2]}.${parts[1]}.${parts[0]}`
    }
    return str
}

const clearDateOfBirth = () => {
    candidateForm.value.dateOfBirth = ''
    dateOfBirthDisplay.value = ''
    dateOfBirthModel.value = null
}

const handleDateOfBirthChange = (value) => {
    if (value) {
        const formatted = formatDate(value)
        candidateForm.value.dateOfBirth = formatted
        dateOfBirthDisplay.value = formatted
        dateOfBirthModel.value = typeof value === 'string' ? value : parseDate(formatted)
    } else {
        candidateForm.value.dateOfBirth = ''
        dateOfBirthDisplay.value = ''
        dateOfBirthModel.value = null
    }
    nextTick(() => { dateOfBirthMenu.value = false })
}

const handleInstallment1DateChange = (value) => {
    if (value) {
        installments.value[0].installmentDate = formatDate(value)
        installment1DateDisplay.value = formatDate(value)
    } else {
        installments.value[0].installmentDate = ''
        installment1DateDisplay.value = ''
    }
    nextTick(() => { installment1DateMenu.value = false })
}

const handleInstallment2DateChange = (value) => {
    if (value) {
        installments.value[1].installmentDate = formatDate(value)
        installment2DateDisplay.value = formatDate(value)
    } else {
        installments.value[1].installmentDate = ''
        installment2DateDisplay.value = ''
    }
    nextTick(() => { installment2DateMenu.value = false })
}

const handleInstallment3DateChange = (value) => {
    if (value) {
        installments.value[2].installmentDate = formatDate(value)
        installment3DateDisplay.value = formatDate(value)
    } else {
        installments.value[2].installmentDate = ''
        installment3DateDisplay.value = ''
    }
    nextTick(() => { installment3DateMenu.value = false })
}

const handleDocWithdrawalDateChange = (value) => {
    if (value) {
        candidateForm.value.docWithdrawalDate = formatDate(value)
        docWithdrawalDateDisplay.value = formatDate(value)
    } else {
        candidateForm.value.docWithdrawalDate = ''
        docWithdrawalDateDisplay.value = ''
    }
    nextTick(() => { docWithdrawalDateMenu.value = false })
}

const clearDocWithdrawalDate = () => {
    candidateForm.value.docWithdrawalDate = ''
    docWithdrawalDateDisplay.value = ''
    docWithdrawalDateModel.value = null
}

const handleDrivingPaymentDateChange = (value) => {
    if (value) {
        candidateForm.value.drivingPaymentDate = formatDate(value)
        drivingPaymentDateDisplay.value = formatDate(value)
    } else {
        candidateForm.value.drivingPaymentDate = ''
        drivingPaymentDateDisplay.value = ''
    }
    nextTick(() => { drivingPaymentDateMenu.value = false })
}

const clearDrivingPaymentDate = () => {
    candidateForm.value.drivingPaymentDate = ''
    drivingPaymentDateDisplay.value = ''
    drivingPaymentDateModel.value = null
}

const candidateForm = ref({
    serialNumber: '',
    firstName: '',
    parentName: '',
    lastName: '',
    dateOfBirth: '',
    personalNumber: '',
    phoneNumber: '',
    placeOfBirth: '',
    address: '',
    categoryId: null,
    instructorId: null,
    vehicleType: '',
    paymentMethod: '',
    practicalHours: null,
    totalServiceAmount: 0,
    docWithdrawalAmount: null,
    docWithdrawalDate: '',
    drivingPaymentAmount: null,
    drivingPaymentDate: '',
    availableSchedule: ''
})

const installments = ref([
    { installmentNumber: 1, amount: 0, installmentDate: '' },
    { installmentNumber: 2, amount: 0, installmentDate: '' },
    { installmentNumber: 3, amount: 0, installmentDate: '' }
])

const rules = {
    required: (v) => !!v || 'Required',
    serialNumber: (v) => {
        if (!v) return true
        return /^\d{1,4}$/.test(v) || 'Must be numbers only, max 4 digits'
    },
    personalNumber: (v) => {
        if (!v) return true
        return /^\d{1,10}$/.test(v) || 'Must be numbers only, max 10 digits'
    }
}

const validateInstallments = () => {
    installmentError.value = ''
    const total = installments.value.reduce((sum, inst) => sum + (inst.amount || 0), 0)
    
    if (total > candidateForm.value.totalServiceAmount) {
        installmentError.value = 'Sum of installments must not exceed Total service amount.'
        return false
    }
    
    // If Installment1 == Total service amount, disable Installment2/3
    if (installments.value[0].amount === candidateForm.value.totalServiceAmount) {
        installmentsDisabled.value[1] = true
        installmentsDisabled.value[2] = true
        installments.value[1].amount = 0
        installments.value[2].amount = 0
    } else {
        installmentsDisabled.value[1] = false
        installmentsDisabled.value[2] = false
    }
    
    return true
}

const loadCategories = () => {
    candidateStore.getCategories()
        .then((response) => {
            if (response.data && (response.data.status === 'error' || response.data.responseMsg)) {
                console.warn('[CandidateForm Categories] API returned error body:', response.data)
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg || 'Error loading categories' })
                return
            }
            const raw = (response.data && response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? String(c.categoryId ?? c.CategoryId ?? '')
            })).filter((c) => c.categoryId != null)
            console.log('[CandidateForm Categories] Loaded', categories.value.length, 'categories →', categories.value)
        })
        .catch((error) => {
            console.error('[CandidateForm Categories] Request failed:', error?.response?.status, error?.response?.data, error)
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Error loading categories' })
        })
}

const loadInstructors = () => {
    candidateStore.getActiveInstructors()
        .then((response) => {
            instructors.value = response.data.data
        })
}

const saveCandidate = async () => {
    if (!validateInstallments()) {
        return
    }

    const formData = {
        ...candidateForm.value,
        categoryId: categorySelect.value?.categoryId || candidateForm.value.categoryId,
        instructorId: instructorSelect.value?.userId || candidateForm.value.instructorId,
        availableSchedule: candidateForm.value.availableSchedule || undefined,
        installments: installments.value
            .filter(inst => inst.amount > 0)
            .map(inst => ({
                installmentNumber: inst.installmentNumber,
                amount: inst.amount,
                installmentDate: inst.installmentDate
            }))
    }

    try {
        if (props.isEdit && (props.candidateId || props.candidate?.candidate?.candidateId)) {
            const id = props.candidateId || props.candidate?.candidate?.candidateId
            await candidateStore.updateCandidate(id, formData)
        } else {
            await candidateStore.createCandidate(formData)
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Successfully Saved' })
        emit('saved')
        closeDialog()
    } catch (error) {
        settingStore.toggleSnackbar({ status: true, msg: error.response?.data?.responseMsg || 'Error saving candidate' })
    }
}

const closeDialog = () => {
    emit('update:modelValue', false)
    resetForm()
}

const resetForm = () => {
    candidateForm.value = {
        serialNumber: '',
        firstName: '',
        parentName: '',
        lastName: '',
        dateOfBirth: '',
        personalNumber: '',
        phoneNumber: '',
        placeOfBirth: '',
        address: '',
        categoryId: null,
        instructorId: null,
        vehicleType: '',
        paymentMethod: '',
        practicalHours: null,
        totalServiceAmount: 0,
        docWithdrawalAmount: null,
        docWithdrawalDate: '',
        drivingPaymentAmount: null,
        drivingPaymentDate: '',
        availableSchedule: ''
    }
    installments.value = [
        { installmentNumber: 1, amount: 0, installmentDate: '' },
        { installmentNumber: 2, amount: 0, installmentDate: '' },
        { installmentNumber: 3, amount: 0, installmentDate: '' }
    ]
    categorySelect.value = null
    instructorSelect.value = null
    installmentError.value = ''
    installmentsDisabled.value = [false, false, false]
    
    // Reset date pickers
    dateOfBirthModel.value = null
    dateOfBirthDisplay.value = ''
    installment1DateModel.value = null
    installment1DateDisplay.value = ''
    installment2DateModel.value = null
    installment2DateDisplay.value = ''
    installment3DateModel.value = null
    installment3DateDisplay.value = ''
    docWithdrawalDateModel.value = null
    docWithdrawalDateDisplay.value = ''
    drivingPaymentDateModel.value = null
    drivingPaymentDateDisplay.value = ''
}

// Watch for candidate prop changes (edit mode)
watch(() => props.candidate, (newVal) => {
    if (newVal && props.isEdit) {
        const candidate = newVal.candidate
        candidateForm.value = {
            serialNumber: candidate.serialNumber || '',
            firstName: candidate.firstName || '',
            parentName: candidate.parentName || '',
            lastName: candidate.lastName || '',
            dateOfBirth: candidate.dateOfBirth || '',
            personalNumber: candidate.personalNumber || '',
            phoneNumber: candidate.phoneNumber || '',
            placeOfBirth: candidate.placeOfBirth || '',
            address: candidate.address || '',
            categoryId: candidate.categoryId,
            instructorId: candidate.instructorId,
            vehicleType: candidate.vehicleType || '',
            paymentMethod: candidate.paymentMethod || '',
            practicalHours: candidate.practicalHours,
            totalServiceAmount: candidate.totalServiceAmount || 0,
            docWithdrawalAmount: candidate.docWithdrawalAmount ?? null,
            docWithdrawalDate: candidate.docWithdrawalDate || '',
            drivingPaymentAmount: candidate.drivingPaymentAmount ?? null,
            drivingPaymentDate: candidate.drivingPaymentDate || '',
            availableSchedule: candidate.availableSchedule || ''
        }

        // Set additional payment date pickers
        if (candidate.docWithdrawalDate) {
            docWithdrawalDateModel.value = parseDate(candidate.docWithdrawalDate)
            docWithdrawalDateDisplay.value = candidate.docWithdrawalDate
        }
        if (candidate.drivingPaymentDate) {
            drivingPaymentDateModel.value = parseDate(candidate.drivingPaymentDate)
            drivingPaymentDateDisplay.value = candidate.drivingPaymentDate
        }
        
        if (candidate.categoryName) {
            categorySelect.value = { categoryId: candidate.categoryId, categoryName: candidate.categoryName }
        }
        if (candidate.instructorName) {
            instructorSelect.value = { userId: candidate.instructorId, fullName: candidate.instructorName }
        }
        
        // Set date picker values
        if (candidate.dateOfBirth) {
            dateOfBirthModel.value = parseDate(candidate.dateOfBirth)
            dateOfBirthDisplay.value = candidate.dateOfBirth
        }
        
        // Load installments
        if (newVal.installments && newVal.installments.length > 0) {
            installments.value = [
                newVal.installments.find(i => i.installmentNumber === 1) || { installmentNumber: 1, amount: 0, installmentDate: '' },
                newVal.installments.find(i => i.installmentNumber === 2) || { installmentNumber: 2, amount: 0, installmentDate: '' },
                newVal.installments.find(i => i.installmentNumber === 3) || { installmentNumber: 3, amount: 0, installmentDate: '' }
            ]
            installments.value.forEach((inst, index) => {
                inst.amount = inst.amount || 0
                inst.installmentDate = inst.installmentDate || ''
                
                // Set date picker values for installments
                if (inst.installmentDate) {
                    const dateModel = parseDate(inst.installmentDate)
                    if (index === 0) {
                        installment1DateModel.value = dateModel
                        installment1DateDisplay.value = inst.installmentDate
                    } else if (index === 1) {
                        installment2DateModel.value = dateModel
                        installment2DateDisplay.value = inst.installmentDate
                    } else if (index === 2) {
                        installment3DateModel.value = dateModel
                        installment3DateDisplay.value = inst.installmentDate
                    }
                }
            })
        }
        
        validateInstallments()
    }
}, { immediate: true, deep: true })

watch(() => candidateForm.value.totalServiceAmount, () => {
    validateInstallments()
})

// Sync date picker model when menu opens so picker shows current value
watch(dateOfBirthMenu, (isOpen) => {
    if (isOpen && candidateForm.value.dateOfBirth) {
        dateOfBirthModel.value = parseDate(candidateForm.value.dateOfBirth)
    }
})

onMounted(() => {
    loadCategories()
    loadInstructors()
})
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

.candidate-form-body {
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

.form-section-title:first-child {
    margin-top: 0;
}

.form-row {
    margin: 0 -6px;
}

.form-row :deep(.v-col) {
    padding: 6px 12px;
}

:deep(.v-card-text) {
    padding: 24px 28px;
}

:deep(.v-text-field),
:deep(.v-textarea),
:deep(.v-select) {
    margin-bottom: 0;
}

:deep(.v-row) {
    margin: 0;
}

:deep(.v-col) {
    padding: 8px 12px;
}
</style>
