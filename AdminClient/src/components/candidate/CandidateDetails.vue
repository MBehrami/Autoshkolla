<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center">
            <span>Candidate Details</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4" v-if="loading">
            <v-progress-linear indeterminate></v-progress-linear>
        </v-card-text>
        <v-card-text class="pa-4" v-else-if="candidate">
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.serialNumber"
                        label="Serial Number (Nr. Rendor)"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.firstName"
                        label="First Name"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.parentName"
                        label="Parent Name"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.lastName"
                        label="Last Name"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.dateOfBirth"
                        label="Date of Birth"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.personalNumber"
                        label="Personal Number"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.phoneNumber"
                        label="Phone Number"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.placeOfBirth"
                        label="Place of Birth"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.address"
                        label="Address"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.categoryName"
                        label="Category"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.instructorName"
                        label="Instructor"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.vehicleType"
                        label="Vehicle Type"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.paymentMethod"
                        label="Payment Method"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.practicalHours"
                        label="Practical Hours"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field
                        :model-value="candidate.totalServiceAmount"
                        label="Total Service Amount"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
            </v-row>

            <!-- Installments Section -->
            <v-divider class="my-4"></v-divider>
            <v-row>
                <v-col cols="12">
                    <h3 class="mb-4">Installments</h3>
                </v-col>
            </v-row>
            <v-row v-if="installments && installments.length > 0">
                <v-col cols="12">
                    <v-table>
                        <thead>
                            <tr>
                                <th>Installment Number</th>
                                <th>Amount</th>
                                <th>Date</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="installment in installments" :key="installment.installmentId">
                                <td>{{ installment.installmentNumber }}</td>
                                <td>{{ installment.amount }}</td>
                                <td>{{ installment.installmentDate }}</td>
                            </tr>
                        </tbody>
                    </v-table>
                </v-col>
            </v-row>
            <v-row v-else>
                <v-col cols="12">
                    <v-alert type="info" density="compact">No installments found</v-alert>
                </v-col>
            </v-row>

            <!-- Practical Lessons Section -->
            <v-divider class="my-4"></v-divider>
            <v-row>
                <v-col cols="12">
                    <h3 class="mb-4">Practical Lessons</h3>
                </v-col>
            </v-row>
            <v-row v-if="canAddLesson">
                <v-col cols="12" md="3">
                    <v-menu v-model="lessonDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                        <template v-slot:activator="{ props: menuProps }">
                            <v-text-field
                                :model-value="lessonDateDisplay"
                                label="Date (dd.MM.yyyy)"
                                variant="outlined"
                                density="compact"
                                prepend-inner-icon="mdi-calendar"
                                readonly
                                hide-details
                                v-bind="menuProps"
                                @click:clear="clearLessonDate"
                            ></v-text-field>
                        </template>
                        <v-date-picker v-model="lessonDateModel" color="primary" @update:model-value="handleLessonDateChange"></v-date-picker>
                    </v-menu>
                </v-col>
                <v-col cols="12" md="3">
                    <v-select
                        v-model="newLessonTime"
                        :items="timeSlots"
                        label="Time"
                        variant="outlined"
                        density="compact"
                        hide-details
                        clearable
                    ></v-select>
                </v-col>
                <v-col cols="12" md="3">
                    <v-select
                        v-model="newLessonVehicle"
                        :items="vehicleOptions"
                        label="Vehicle"
                        variant="outlined"
                        density="compact"
                        hide-details
                        clearable
                    ></v-select>
                </v-col>
                <v-col cols="12" md="3" class="d-flex align-center">
                    <v-btn color="primary" variant="elevated" size="small" :loading="addingLesson" @click="addLesson">
                        Add lesson
                    </v-btn>
                </v-col>
            </v-row>
            <v-row v-if="practicalLessons && practicalLessons.length > 0">
                <v-col cols="12">
                    <v-table>
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Time</th>
                                <th>Instructor</th>
                                <th>Vehicle</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="lesson in practicalLessons" :key="lesson.practicalLessonId || lesson.lessonId">
                                <td>{{ lesson.lessonDate || lesson.date }}</td>
                                <td>{{ lesson.time }}</td>
                                <td>{{ lesson.instructorName }}</td>
                                <td>{{ lesson.vehicle }}</td>
                            </tr>
                        </tbody>
                    </v-table>
                </v-col>
            </v-row>
            <v-row v-else>
                <v-col cols="12">
                    <v-alert type="info" density="compact">No practical lessons found</v-alert>
                </v-col>
            </v-row>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Close" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, watch, nextTick } from 'vue';

const props = defineProps({
    candidateId: {
        type: Number,
        required: true
    }
})

const emit = defineEmits(['close'])

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)

const candidate = ref(null)
const installments = ref([])
const practicalLessons = ref([])
const lessonDateMenu = ref(false)
const lessonDateModel = ref(null)
const lessonDateDisplay = ref('')
const newLessonTime = ref('')
const newLessonVehicle = ref('')
const addingLesson = ref(false)

const timeSlots = ['07:00', '08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00']
const vehicleOptions = ['Vehicle 1', 'Vehicle 2', 'Vehicle 3']

const canAddLesson = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}')
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || ''
        return role === 'Admin' || role === 'Instructor'
    } catch {
        return false
    }
})

function formatDate(value) {
    if (!value) return ''
    const str = String(value).split('T')[0]
    const parts = str.split('-')
    if (parts.length === 3) return `${parts[2]}.${parts[1]}.${parts[0]}`
    return str
}

const handleLessonDateChange = (value) => {
    if (value) {
        lessonDateDisplay.value = formatDate(value)
        lessonDateModel.value = typeof value === 'string' ? value : value
    } else {
        lessonDateDisplay.value = ''
        lessonDateModel.value = null
    }
    nextTick(() => { lessonDateMenu.value = false })
}

const clearLessonDate = () => {
    lessonDateDisplay.value = ''
    lessonDateModel.value = null
}

const addLesson = async () => {
    const dateStr = lessonDateDisplay.value
    if (!dateStr || !newLessonTime.value) {
        settingStore.toggleSnackbar({ status: true, msg: 'Date and Time are required' })
        return
    }
    addingLesson.value = true
    try {
        await candidateStore.addPracticalLesson({
            candidateId: props.candidateId,
            lessonDate: dateStr,
            time: newLessonTime.value,
            vehicle: newLessonVehicle.value || null
        })
        settingStore.toggleSnackbar({ status: true, msg: 'Lesson added' })
        lessonDateDisplay.value = ''
        lessonDateModel.value = null
        newLessonTime.value = ''
        newLessonVehicle.value = ''
        loadCandidateDetails()
    } catch (err) {
        settingStore.toggleSnackbar({ status: true, msg: err?.response?.data?.responseMsg || 'Error adding lesson' })
    } finally {
        addingLesson.value = false
    }
}

const loadCandidateDetails = () => {
    candidateStore.getCandidateDetails(props.candidateId)
        .then((response) => {
            candidate.value = response.data.candidate
            installments.value = response.data.installments || []
            practicalLessons.value = response.data.practicalLessons || []
        })
        .catch(() => {
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading candidate details' })
        })
}

const closeDialog = () => emit('close')

watch(() => props.candidateId, () => {
    if (props.candidateId) loadCandidateDetails()
}, { immediate: true })

onMounted(() => {
    if (props.candidateId) loadCandidateDetails()
})
</script>
