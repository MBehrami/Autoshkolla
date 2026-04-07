<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>{{ isEdit ? 'Ndrysho regjistrimin' : 'Regjistro për orë shtesë' }}</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="additional-form-body">
            <v-form v-model="valid" ref="formRef" @submit.prevent="save">
                <!-- Personal info -->
                <h3 class="form-section-title">Të dhënat personale</h3>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-text-field v-model="form.firstName" label="Emri" :rules="[rules.required]"
                            variant="outlined" clearable required></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field v-model="form.lastName" label="Mbiemri" :rules="[rules.required]"
                            variant="outlined" clearable required></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field v-model="form.personalNumber" label="Numri personal (opsional)"
                            variant="outlined" clearable></v-text-field>
                    </v-col>
                </v-row>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-text-field v-model="form.contactNumber" label="Numri i kontaktit"
                            :rules="[rules.required]" variant="outlined" clearable required></v-text-field>
                    </v-col>
                </v-row>

                <!-- Category, instructor, vehicle -->
                <h3 class="form-section-title">Të dhënat tjera</h3>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-select v-model="categorySelect" :items="categories" item-title="categoryName"
                            item-value="categoryId" label="Kategoria" :rules="[rules.required]"
                            variant="outlined" return-object clearable required></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-select v-model="instructorSelect" :items="instructors" item-title="fullName"
                            item-value="userId" label="Instruktori" variant="outlined"
                            return-object clearable></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-select v-model="form.vehicleType" :items="['Manual', 'Automatic']"
                            label="Lloji i vetures" variant="outlined" clearable></v-select>
                    </v-col>
                </v-row>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-select v-model="form.paymentMethod" :items="['Bank', 'Cash']"
                            label="Metoda e pagesës" variant="outlined" clearable></v-select>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field v-model.number="form.practicalHours" label="Orët praktike"
                            type="number" variant="outlined" clearable></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field v-model.number="form.servicePayment" label="Pagesa e shërbimit"
                            :rules="[rules.required]" type="number" variant="outlined"
                            clearable required></v-text-field>
                    </v-col>
                </v-row>

                <!-- Installments -->
                <h3 class="form-section-title">Pagesat</h3>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field v-model.number="installments[0].amount" label="Pagesa - Kësti 1"
                            type="number" variant="outlined" :disabled="installmentsDisabled[0]"
                            clearable @update:model-value="validateInstallments"></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="inst1DateMenu" :close-on-content-click="false" location="bottom"
                            transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field v-model="inst1DateDisplay" label="Data e pagesës - Kësti 1"
                                    prepend-inner-icon="mdi-calendar" readonly v-bind="menuProps"
                                    variant="outlined" density="comfortable"
                                    :disabled="installmentsDisabled[0]" clearable></v-text-field>
                            </template>
                            <v-date-picker v-model="inst1DateModel" color="primary"
                                @update:model-value="handleInst1DateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field v-model.number="installments[1].amount" label="Pagesa - Kësti 2"
                            type="number" variant="outlined" :disabled="installmentsDisabled[1]"
                            clearable @update:model-value="validateInstallments"></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="inst2DateMenu" :close-on-content-click="false" location="bottom"
                            transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field v-model="inst2DateDisplay" label="Data e pagesës - Kësti 2"
                                    prepend-inner-icon="mdi-calendar" readonly v-bind="menuProps"
                                    variant="outlined" density="comfortable"
                                    :disabled="installmentsDisabled[1]" clearable></v-text-field>
                            </template>
                            <v-date-picker v-model="inst2DateModel" color="primary"
                                @update:model-value="handleInst2DateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row class="form-row">
                    <v-col cols="12" md="6">
                        <v-text-field v-model.number="installments[2].amount" label="Pagesa - Kësti 3"
                            type="number" variant="outlined" :disabled="installmentsDisabled[2]"
                            clearable @update:model-value="validateInstallments"></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-menu v-model="inst3DateMenu" :close-on-content-click="false" location="bottom"
                            transition="scale-transition" min-width="auto">
                            <template v-slot:activator="{ props: menuProps }">
                                <v-text-field v-model="inst3DateDisplay" label="Data e pagesës - Kësti 3"
                                    prepend-inner-icon="mdi-calendar" readonly v-bind="menuProps"
                                    variant="outlined" density="comfortable"
                                    :disabled="installmentsDisabled[2]" clearable></v-text-field>
                            </template>
                            <v-date-picker v-model="inst3DateModel" color="primary"
                                @update:model-value="handleInst3DateChange"></v-date-picker>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-row v-if="installmentError" class="form-row">
                    <v-col cols="12">
                        <v-alert type="error" density="compact">{{ installmentError }}</v-alert>
                    </v-col>
                </v-row>

                <!-- Notes -->
                <h3 class="form-section-title">Shënime</h3>
                <v-row dense>
                    <v-col cols="12">
                        <v-textarea v-model="form.additionalNotes" label="Shënime shtesë"
                            variant="outlined" rows="3" clearable></v-textarea>
                    </v-col>
                </v-row>
            </v-form>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="sticky-footer pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Anulo" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
            <v-btn :disabled="!valid || !!installmentError" :loading="store.loading"
                text="Regjistro" color="grey-darken-4" class="text-capitalize" @click="save"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useAdditionalLessonStore } from '@/store/AdditionalLessonStore';
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, watch, onMounted, nextTick } from 'vue';

const props = defineProps({
    modelValue: Boolean,
    item: Object,
    itemId: [Number, String],
    isEdit: Boolean
})
const emit = defineEmits(['update:modelValue', 'saved'])

const store = useAdditionalLessonStore()
const candidateStore = useCandidateStore()
const settingStore = useSettingStore()

const valid = ref(false)
const formRef = ref(null)
const categories = ref([])
const instructors = ref([])
const categorySelect = ref(null)
const instructorSelect = ref(null)
const installmentError = ref('')
const installmentsDisabled = ref([false, false, false])

const inst1DateMenu = ref(false)
const inst1DateModel = ref(null)
const inst1DateDisplay = ref('')
const inst2DateMenu = ref(false)
const inst2DateModel = ref(null)
const inst2DateDisplay = ref('')
const inst3DateMenu = ref(false)
const inst3DateModel = ref(null)
const inst3DateDisplay = ref('')

const rules = {
    required: (v) => !!v || v === 0 || 'Kjo fushë është e detyrueshme',
}

const defaultForm = () => ({
    firstName: '',
    lastName: '',
    personalNumber: '',
    contactNumber: '',
    categoryId: null,
    instructorId: null,
    vehicleType: null,
    paymentMethod: null,
    practicalHours: null,
    servicePayment: 0,
    additionalNotes: '',
})

const form = ref(defaultForm())

const installments = ref([
    { installmentNumber: 1, amount: 0, installmentDate: '' },
    { installmentNumber: 2, amount: 0, installmentDate: '' },
    { installmentNumber: 3, amount: 0, installmentDate: '' }
])

const formatDate = (value) => {
    if (!value) return ''
    if (value instanceof Date) {
        return `${String(value.getDate()).padStart(2, '0')}.${String(value.getMonth() + 1).padStart(2, '0')}.${value.getFullYear()}`
    }
    const str = String(value).split('T')[0]
    const parts = str.split('-')
    if (parts.length === 3) return `${parts[2]}.${parts[1]}.${parts[0]}`
    return str
}

const parseDate = (dateStr) => {
    if (!dateStr) return null
    const parts = String(dateStr).split('.')
    if (parts.length === 3) return `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`
    return null
}

const handleInst1DateChange = (value) => {
    if (value) {
        installments.value[0].installmentDate = formatDate(value)
        inst1DateDisplay.value = formatDate(value)
    } else {
        installments.value[0].installmentDate = ''
        inst1DateDisplay.value = ''
    }
    nextTick(() => { inst1DateMenu.value = false })
}

const handleInst2DateChange = (value) => {
    if (value) {
        installments.value[1].installmentDate = formatDate(value)
        inst2DateDisplay.value = formatDate(value)
    } else {
        installments.value[1].installmentDate = ''
        inst2DateDisplay.value = ''
    }
    nextTick(() => { inst2DateMenu.value = false })
}

const handleInst3DateChange = (value) => {
    if (value) {
        installments.value[2].installmentDate = formatDate(value)
        inst3DateDisplay.value = formatDate(value)
    } else {
        installments.value[2].installmentDate = ''
        inst3DateDisplay.value = ''
    }
    nextTick(() => { inst3DateMenu.value = false })
}

const validateInstallments = () => {
    installmentError.value = ''
    const total = installments.value.reduce((sum, inst) => sum + (inst.amount || 0), 0)

    if (total > form.value.servicePayment) {
        installmentError.value = 'Shuma e kesteve nuk duhet të tejkalojë shumën totale të shërbimit.'
        return false
    }

    if (installments.value[0].amount === form.value.servicePayment) {
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

const closeDialog = () => {
    emit('update:modelValue', false)
    resetForm()
}

const resetForm = () => {
    form.value = defaultForm()
    categorySelect.value = null
    instructorSelect.value = null
    installments.value = [
        { installmentNumber: 1, amount: 0, installmentDate: '' },
        { installmentNumber: 2, amount: 0, installmentDate: '' },
        { installmentNumber: 3, amount: 0, installmentDate: '' }
    ]
    installmentError.value = ''
    installmentsDisabled.value = [false, false, false]
    inst1DateModel.value = null
    inst1DateDisplay.value = ''
    inst2DateModel.value = null
    inst2DateDisplay.value = ''
    inst3DateModel.value = null
    inst3DateDisplay.value = ''
    if (formRef.value) formRef.value.resetValidation()
}

const populateForm = (data) => {
    const al = data.additionalLesson || data
    form.value = {
        firstName: al.firstName ?? al.FirstName ?? '',
        lastName: al.lastName ?? al.LastName ?? '',
        personalNumber: al.personalNumber ?? al.PersonalNumber ?? '',
        contactNumber: al.contactNumber ?? al.ContactNumber ?? '',
        categoryId: al.categoryId ?? al.CategoryId ?? null,
        instructorId: al.instructorId ?? al.InstructorId ?? null,
        vehicleType: al.vehicleType ?? al.VehicleType ?? null,
        paymentMethod: al.paymentMethod ?? al.PaymentMethod ?? null,
        practicalHours: al.practicalHours ?? al.PracticalHours ?? null,
        servicePayment: al.servicePayment ?? al.ServicePayment ?? 0,
        additionalNotes: al.additionalNotes ?? al.AdditionalNotes ?? '',
    }

    const catId = form.value.categoryId
    if (catId && categories.value.length) {
        categorySelect.value = categories.value.find(c => c.categoryId === catId) || null
    }

    const instrId = form.value.instructorId
    if (instrId && instructors.value.length) {
        instructorSelect.value = instructors.value.find(i => i.userId === instrId) || null
    }

    const rawInst = data.installments || []
    installments.value = [
        rawInst.find(i => (i.installmentNumber ?? i.InstallmentNumber) === 1) || { installmentNumber: 1, amount: 0, installmentDate: '' },
        rawInst.find(i => (i.installmentNumber ?? i.InstallmentNumber) === 2) || { installmentNumber: 2, amount: 0, installmentDate: '' },
        rawInst.find(i => (i.installmentNumber ?? i.InstallmentNumber) === 3) || { installmentNumber: 3, amount: 0, installmentDate: '' }
    ]
    installments.value.forEach((inst, index) => {
        inst.amount = inst.amount ?? inst.Amount ?? 0
        inst.installmentDate = inst.installmentDate ?? inst.InstallmentDate ?? ''
        inst.installmentNumber = index + 1

        if (inst.installmentDate) {
            const dm = parseDate(inst.installmentDate)
            if (index === 0) { inst1DateModel.value = dm; inst1DateDisplay.value = inst.installmentDate }
            else if (index === 1) { inst2DateModel.value = dm; inst2DateDisplay.value = inst.installmentDate }
            else if (index === 2) { inst3DateModel.value = dm; inst3DateDisplay.value = inst.installmentDate }
        }
    })

    validateInstallments()
}

watch(categorySelect, (val) => {
    form.value.categoryId = val?.categoryId ?? null
})

watch(instructorSelect, (val) => {
    form.value.instructorId = val?.userId ?? null
})

watch(() => form.value.servicePayment, () => {
    validateInstallments()
})

watch(() => props.modelValue, (visible) => {
    if (visible && props.isEdit && props.itemId) {
        store.getById(props.itemId)
            .then((response) => {
                const data = response?.data
                if (data && !data.responseMsg) {
                    populateForm(data)
                }
            })
            .catch(() => {})
    } else if (!visible) {
        resetForm()
    }
})

const save = () => {
    if (!validateInstallments()) return

    const payload = {
        ...form.value,
        servicePayment: Number(form.value.servicePayment) || 0,
        practicalHours: form.value.practicalHours ? Number(form.value.practicalHours) : null,
        installments: installments.value
            .filter(inst => inst.amount > 0)
            .map(inst => ({
                installmentNumber: inst.installmentNumber,
                amount: inst.amount,
                installmentDate: inst.installmentDate
            }))
    }

    const action = props.isEdit
        ? store.update(props.itemId, payload)
        : store.create(payload)

    action
        .then((response) => {
            const body = response?.data
            if (body?.status === 'success') {
                settingStore.toggleSnackbar({ status: true, msg: body.responseMsg || 'U ruajt me sukses' })
                emit('saved')
            } else if (body?.status === 'error') {
                settingStore.toggleSnackbar({ status: true, msg: body.responseMsg })
            }
        })
        .catch((error) => {
            settingStore.toggleSnackbar({ status: true, msg: error?.response?.data?.responseMsg || 'Ndodhi një gabim' })
        })
}

const loadDropdowns = () => {
    candidateStore.getCategories()
        .then((response) => {
            const raw = response.data?.data ?? (Array.isArray(response.data) ? response.data : [])
            categories.value = raw.map((c) => ({
                categoryId: c.categoryId ?? c.CategoryId,
                categoryName: c.categoryName ?? c.CategoryName ?? ''
            })).filter((c) => c.categoryId != null)
        })
        .catch(() => {})

    candidateStore.getActiveInstructors()
        .then((response) => {
            const raw = response.data?.data ?? (Array.isArray(response.data) ? response.data : [])
            instructors.value = raw.map((i) => ({
                userId: i.userId ?? i.UserId,
                fullName: i.fullName ?? i.FullName ?? ''
            })).filter((i) => i.userId != null)
        })
        .catch(() => {})
}

onMounted(() => {
    loadDropdowns()
})
</script>

<style scoped>
.form-section-title {
    font-size: 0.8125rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #64748b;
    margin-bottom: 12px;
    margin-top: 16px;
    padding-bottom: 8px;
    border-bottom: 1px solid #f1f5f9;
}

.form-section-title:first-child {
    margin-top: 0;
}

.sticky-header {
    position: sticky;
    top: 0;
    z-index: 1;
    background: #fff;
    padding: 16px 24px;
}

.sticky-footer {
    position: sticky;
    bottom: 0;
    z-index: 1;
    background: #fff;
    border-top: 1px solid #e2e8f0;
}

.additional-form-body {
    max-height: 72vh;
    overflow-y: auto;
    padding: 20px 24px;
}

.form-row {
    margin: 0 -4px;
}

.form-row :deep(.v-col) {
    padding: 6px 10px;
}
</style>
