<template>
    <div class="page-container">
        <!-- Header -->
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3 mb-6">
            <div class="d-flex align-center ga-3">
                <v-btn icon variant="text" @click="goBack">
                    <v-icon>mdi-arrow-left</v-icon>
                </v-btn>
                <div>
                    <div class="page-title">{{ candidateForm.firstName ? 'Ndrysho kandidatin' : 'Ngarkohet...' }}</div>
                    <div class="page-subtitle" v-if="candidateForm.firstName">
                        {{ candidateForm.firstName }} {{ candidateForm.lastName }}
                    </div>
                </div>
            </div>
            <div class="d-flex ga-2">
                <v-btn variant="tonal" color="secondary" class="text-none" prepend-icon="mdi-close" @click="goBack">
                    Anulo
                </v-btn>
                <v-btn variant="flat" color="primary" class="text-none" prepend-icon="mdi-content-save"
                    :disabled="!valid || !!installmentError" :loading="loading" @click="saveCandidate">
                    Ruaj ndryshimet
                </v-btn>
            </div>
        </div>

        <v-progress-linear v-if="pageLoading" indeterminate color="primary" class="mb-4"></v-progress-linear>

        <v-form v-if="!pageLoading" v-model="valid" ref="formRef" @submit.prevent="saveCandidate">
            <v-row>
                <!-- Left Column -->
                <v-col cols="12" lg="8">
                    <!-- Personal Information -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Të dhënat personale</div>
                            <v-row>
                                <v-col cols="12" sm="6" md="4">
                                    <v-text-field v-model="candidateForm.serialNumber" label="Nr. Rendor"
                                        :rules="[rules.serialNumber]" variant="outlined" density="comfortable"
                                        hide-details="auto" clearable></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <v-text-field v-model="candidateForm.firstName" label="Emri"
                                        :rules="[rules.required]" variant="outlined" density="comfortable"
                                        hide-details="auto" clearable required></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <v-text-field v-model="candidateForm.parentName" label="Emri i prindit"
                                        variant="outlined" density="comfortable" hide-details="auto"
                                        clearable></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <v-text-field v-model="candidateForm.lastName" label="Mbiemri"
                                        :rules="[rules.required]" variant="outlined" density="comfortable"
                                        hide-details="auto" clearable required></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <v-menu v-model="dateOfBirthMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field :model-value="dateOfBirthDisplay"
                                                label="Data e lindjes (dd.MM.yyyy)" variant="outlined"
                                                density="comfortable" prepend-inner-icon="mdi-calendar" readonly
                                                :rules="[rules.required]" clearable required v-bind="menuProps"
                                                @click:clear="clearDateOfBirth" hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="dateOfBirthModel" color="primary"
                                            @update:model-value="handleDateOfBirthChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <v-text-field v-model="candidateForm.personalNumber" label="Numri personal"
                                        :rules="[rules.required, rules.personalNumber]" variant="outlined"
                                        density="comfortable" hide-details="auto" clearable
                                        required></v-text-field>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Contact & Address -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Kontakti & Adresa</div>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field v-model="candidateForm.phoneNumber" label="Numri i telefonit"
                                        variant="outlined" density="comfortable" hide-details="auto"
                                        clearable></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-text-field v-model="candidateForm.placeOfBirth" label="Vendi i lindjes"
                                        variant="outlined" density="comfortable" hide-details="auto"
                                        clearable></v-text-field>
                                </v-col>
                                <v-col cols="12">
                                    <v-text-field v-model="candidateForm.address" label="Adresa" variant="outlined"
                                        density="comfortable" hide-details="auto" clearable></v-text-field>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Installments -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Këstet e pagesës</div>
                            <!-- Installment 1 -->
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field v-model.number="installments[0].amount" label="Pagesa - Kësti 1"
                                        type="number" variant="outlined" density="comfortable" hide-details="auto"
                                        :disabled="installmentsDisabled[0]" clearable
                                        @update:model-value="validateInstallments"></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-menu v-model="installment1DateMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field v-model="installment1DateDisplay"
                                                label="Data e pagesës - Kësti 1" prepend-inner-icon="mdi-calendar"
                                                readonly v-bind="menuProps" variant="outlined" density="comfortable"
                                                :disabled="installmentsDisabled[0]" clearable
                                                hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="installment1DateModel" color="primary"
                                            @update:model-value="handleInstallment1DateChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                            </v-row>
                            <!-- Installment 2 -->
                            <v-row class="mt-1">
                                <v-col cols="12" sm="6">
                                    <v-text-field v-model.number="installments[1].amount" label="Pagesa - Kësti 2"
                                        type="number" variant="outlined" density="comfortable" hide-details="auto"
                                        :disabled="installmentsDisabled[1]" clearable
                                        @update:model-value="validateInstallments"></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-menu v-model="installment2DateMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field v-model="installment2DateDisplay"
                                                label="Data e pagesës - Kësti 2" prepend-inner-icon="mdi-calendar"
                                                readonly v-bind="menuProps" variant="outlined" density="comfortable"
                                                :disabled="installmentsDisabled[1]" clearable
                                                hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="installment2DateModel" color="primary"
                                            @update:model-value="handleInstallment2DateChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                            </v-row>
                            <!-- Installment 3 -->
                            <v-row class="mt-1">
                                <v-col cols="12" sm="6">
                                    <v-text-field v-model.number="installments[2].amount" label="Pagesa - Kësti 3"
                                        type="number" variant="outlined" density="comfortable" hide-details="auto"
                                        :disabled="installmentsDisabled[2]" clearable
                                        @update:model-value="validateInstallments"></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-menu v-model="installment3DateMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field v-model="installment3DateDisplay"
                                                label="Data e pagesës - Kësti 3" prepend-inner-icon="mdi-calendar"
                                                readonly v-bind="menuProps" variant="outlined" density="comfortable"
                                                :disabled="installmentsDisabled[2]" clearable
                                                hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="installment3DateModel" color="primary"
                                            @update:model-value="handleInstallment3DateChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                            </v-row>
                            <v-row v-if="installmentError" class="mt-2">
                                <v-col cols="12">
                                    <v-alert type="error" density="compact">{{ installmentError }}</v-alert>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Additional Payments -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Pagesa tjera</div>
                            <v-row>
                                <v-col cols="12" sm="6" md="3">
                                    <v-text-field v-model.number="candidateForm.docWithdrawalAmount"
                                        label="Pagesa e terheqjes së dok." type="number" variant="outlined"
                                        density="comfortable" hide-details="auto" clearable></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-menu v-model="docWithdrawalDateMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field v-model="docWithdrawalDateDisplay"
                                                label="Data e pagesës (dok.)" prepend-inner-icon="mdi-calendar"
                                                readonly v-bind="menuProps" variant="outlined" density="comfortable"
                                                clearable @click:clear="clearDocWithdrawalDate"
                                                hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="docWithdrawalDateModel" color="primary"
                                            @update:model-value="handleDocWithdrawalDateChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-text-field v-model.number="candidateForm.drivingPaymentAmount"
                                        label="Pagesa e vozitjes" type="number" variant="outlined"
                                        density="comfortable" hide-details="auto" clearable></v-text-field>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-menu v-model="drivingPaymentDateMenu" :close-on-content-click="false"
                                        location="bottom" transition="scale-transition" min-width="auto">
                                        <template v-slot:activator="{ props: menuProps }">
                                            <v-text-field v-model="drivingPaymentDateDisplay"
                                                label="Data e pagesës (vozitje)" prepend-inner-icon="mdi-calendar"
                                                readonly v-bind="menuProps" variant="outlined" density="comfortable"
                                                clearable @click:clear="clearDrivingPaymentDate"
                                                hide-details="auto"></v-text-field>
                                        </template>
                                        <v-date-picker v-model="drivingPaymentDateModel" color="primary"
                                            @update:model-value="handleDrivingPaymentDateChange"></v-date-picker>
                                    </v-menu>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Notes -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Shënime shtesë</div>
                            <v-textarea v-model="candidateForm.availableSchedule"
                                label="Shënime shtesë rreth orarit të lirë të kandidatit"
                                placeholder="psh. Hënë 14:00–18:00, shtunë 14:00–18:00" variant="outlined" rows="3"
                                auto-grow hide-details="auto" clearable></v-textarea>
                        </v-card-text>
                    </v-card>
                </v-col>

                <!-- Right Column — Summary Sidebar -->
                <v-col cols="12" lg="4">
                    <!-- Category & Vehicle -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Kategoria & Vetura</div>
                            <v-select v-model="categorySelect" :items="categories" item-title="categoryName"
                                item-value="categoryId" label="Kategoria" :rules="[rules.required]" variant="outlined"
                                density="comfortable" return-object clearable required hide-details="auto"
                                class="mb-3"></v-select>
                            <v-select v-model="candidateForm.vehicleType" :items="['Manual', 'Automatic']"
                                label="Lloji i vetures" variant="outlined" density="comfortable" clearable
                                hide-details="auto" class="mb-3"></v-select>
                            <v-select v-model="instructorSelect" :items="instructors" item-title="fullName"
                                item-value="userId" label="Instruktori" variant="outlined" density="comfortable"
                                return-object clearable hide-details="auto" class="mb-3"></v-select>
                            <v-text-field v-model.number="candidateForm.practicalHours" label="Orët praktike"
                                type="number" variant="outlined" density="comfortable" clearable
                                hide-details="auto"></v-text-field>
                        </v-card-text>
                    </v-card>

                    <!-- Payment Info -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Pagesat</div>
                            <v-text-field v-model.number="candidateForm.totalServiceAmount"
                                label="Pagesa e shërbimit" :rules="[rules.required]" type="number" variant="outlined"
                                density="comfortable" clearable required hide-details="auto"
                                class="mb-3"></v-text-field>
                            <v-select v-model="candidateForm.paymentMethod" :items="['Bank', 'Cash']"
                                label="Metoda e pagesës" variant="outlined" density="comfortable" clearable
                                hide-details="auto"></v-select>
                        </v-card-text>
                    </v-card>

                    <!-- Save Actions (sticky on scroll) -->
                    <v-card class="mb-4 save-card">
                        <v-card-text class="pa-5">
                            <v-btn block variant="flat" color="primary" class="text-none mb-2"
                                prepend-icon="mdi-content-save" :disabled="!valid || !!installmentError"
                                :loading="loading" @click="saveCandidate">
                                Ruaj ndryshimet
                            </v-btn>
                            <v-btn block variant="tonal" color="secondary" class="text-none" @click="goBack">
                                Anulo
                            </v-btn>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>
        </v-form>
    </div>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const route = useRoute()
const router = useRouter()
const numericId = computed(() => Number(route.params.id))

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)

const pageLoading = ref(true)
const valid = ref(false)
const formRef = ref(null)
const categories = ref([])
const instructors = ref([])
const categorySelect = ref(null)
const instructorSelect = ref(null)
const installmentError = ref('')
const installmentsDisabled = ref([false, false, false])

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

const goBack = () => {
    router.push('/candidates')
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

const validateInstallments = () => {
    installmentError.value = ''
    const total = installments.value.reduce((sum, inst) => sum + (inst.amount || 0), 0)
    if (total > candidateForm.value.totalServiceAmount) {
        installmentError.value = 'Sum of installments must not exceed Total service amount.'
        return false
    }
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
            if (response.data && (response.data.status === 'error' || response.data.responseMsg)) return
            const raw = (response.data && response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? String(c.categoryId ?? c.CategoryId ?? '')
            })).filter((c) => c.categoryId != null)
        })
        .catch(() => {})
}

const loadInstructors = () => {
    candidateStore.getActiveInstructors()
        .then((response) => {
            instructors.value = response.data.data
        })
        .catch(() => {})
}

const loadCandidate = () => {
    pageLoading.value = true
    candidateStore.getCandidateDetails(numericId.value)
        .then((response) => {
            const data = response.data
            const c = data.candidate

            candidateForm.value = {
                serialNumber: c.serialNumber || '',
                firstName: c.firstName || '',
                parentName: c.parentName || '',
                lastName: c.lastName || '',
                dateOfBirth: c.dateOfBirth || '',
                personalNumber: c.personalNumber || '',
                phoneNumber: c.phoneNumber || '',
                placeOfBirth: c.placeOfBirth || '',
                address: c.address || '',
                categoryId: c.categoryId,
                instructorId: c.instructorId,
                vehicleType: c.vehicleType || '',
                paymentMethod: c.paymentMethod || '',
                practicalHours: c.practicalHours,
                totalServiceAmount: c.totalServiceAmount || 0,
                docWithdrawalAmount: c.docWithdrawalAmount ?? null,
                docWithdrawalDate: c.docWithdrawalDate || '',
                drivingPaymentAmount: c.drivingPaymentAmount ?? null,
                drivingPaymentDate: c.drivingPaymentDate || '',
                availableSchedule: c.availableSchedule || ''
            }

            if (c.docWithdrawalDate) {
                docWithdrawalDateModel.value = parseDate(c.docWithdrawalDate)
                docWithdrawalDateDisplay.value = c.docWithdrawalDate
            }
            if (c.drivingPaymentDate) {
                drivingPaymentDateModel.value = parseDate(c.drivingPaymentDate)
                drivingPaymentDateDisplay.value = c.drivingPaymentDate
            }

            if (c.categoryName) {
                categorySelect.value = { categoryId: c.categoryId, categoryName: c.categoryName }
            }
            if (c.instructorName) {
                instructorSelect.value = { userId: c.instructorId, fullName: c.instructorName }
            }

            if (c.dateOfBirth) {
                dateOfBirthModel.value = parseDate(c.dateOfBirth)
                dateOfBirthDisplay.value = c.dateOfBirth
            }

            if (data.installments && data.installments.length > 0) {
                installments.value = [
                    data.installments.find(i => i.installmentNumber === 1) || { installmentNumber: 1, amount: 0, installmentDate: '' },
                    data.installments.find(i => i.installmentNumber === 2) || { installmentNumber: 2, amount: 0, installmentDate: '' },
                    data.installments.find(i => i.installmentNumber === 3) || { installmentNumber: 3, amount: 0, installmentDate: '' }
                ]
                installments.value.forEach((inst, index) => {
                    inst.amount = inst.amount || 0
                    inst.installmentDate = inst.installmentDate || ''
                    if (inst.installmentDate) {
                        const dateModel = parseDate(inst.installmentDate)
                        if (index === 0) { installment1DateModel.value = dateModel; installment1DateDisplay.value = inst.installmentDate }
                        else if (index === 1) { installment2DateModel.value = dateModel; installment2DateDisplay.value = inst.installmentDate }
                        else if (index === 2) { installment3DateModel.value = dateModel; installment3DateDisplay.value = inst.installmentDate }
                    }
                })
            }

            validateInstallments()
        })
        .catch(() => {
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading candidate' })
            router.push('/candidates')
        })
        .finally(() => {
            pageLoading.value = false
        })
}

const saveCandidate = async () => {
    if (!validateInstallments()) return

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
        await candidateStore.updateCandidate(numericId.value, formData)
        settingStore.toggleSnackbar({ status: true, msg: 'Kandidati u ndryshua me sukses!' })
        router.push(`/candidates/${numericId.value}`)
    } catch (error) {
        settingStore.toggleSnackbar({ status: true, msg: error.response?.data?.responseMsg || 'Error saving candidate' })
    }
}

watch(() => candidateForm.value.totalServiceAmount, () => {
    validateInstallments()
})

watch(dateOfBirthMenu, (isOpen) => {
    if (isOpen && candidateForm.value.dateOfBirth) {
        dateOfBirthModel.value = parseDate(candidateForm.value.dateOfBirth)
    }
})

onMounted(() => {
    loadCategories()
    loadInstructors()
    if (numericId.value) loadCandidate()
})
</script>

<style scoped>
.save-card {
    position: sticky;
    top: 80px;
}

@media (max-width: 1279px) {
    .save-card {
        position: static;
    }
}
</style>
