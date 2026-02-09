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
            <v-row>
                <v-col cols="12" md="3">
                    <v-text-field
                        :model-value="candidate.docWithdrawalAmount"
                        label="Doc Withdrawal Amount"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="3">
                    <v-text-field
                        :model-value="candidate.docWithdrawalDate"
                        label="Doc Withdrawal Date"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="3">
                    <v-text-field
                        :model-value="candidate.drivingPaymentAmount"
                        label="Driving Payment Amount"
                        variant="outlined"
                        readonly
                    ></v-text-field>
                </v-col>
                <v-col cols="12" md="3">
                    <v-text-field
                        :model-value="candidate.drivingPaymentDate"
                        label="Driving Payment Date"
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

            <!-- Practical Lessons Section (Admin: view-only; Instructor uses the edit modal instead) -->
            <v-divider class="my-4"></v-divider>
            <v-row>
                <v-col cols="12">
                    <h3 class="mb-4">Practical Lessons <span class="text-caption text-medium-emphasis">({{ sortedLessons.length }} recorded)</span></h3>
                </v-col>
            </v-row>
            <v-row v-if="sortedLessons && sortedLessons.length > 0">
                <v-col cols="12">
                    <v-table density="comfortable" class="elevation-1 rounded">
                        <thead>
                            <tr>
                                <th>Nr. Rendor</th>
                                <th>Date</th>
                                <th>Start</th>
                                <th>End</th>
                                <th>Instructor</th>
                                <th>Vehicle</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(lesson, index) in sortedLessons" :key="lesson.practicalLessonId || lesson.lessonId || index">
                                <td>{{ index + 1 }}</td>
                                <td>{{ lesson.lessonDate || lesson.date }}</td>
                                <td>{{ lesson.time }}</td>
                                <td>{{ lesson.endTime || calcEndTime(lesson.time) }}</td>
                                <td>{{ lesson.instructorName }}</td>
                                <td>{{ lesson.vehicle || '–' }}</td>
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
import { ref, computed, onMounted, watch } from 'vue';

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

// Sort lessons by date asc then time asc for Nr. Rendor (1..N)
const sortedLessons = computed(() => {
    const list = practicalLessons.value || []
    return [...list].sort((a, b) => {
        const dA = (a.lessonDate || '').split('.').reverse().join('')
        const dB = (b.lessonDate || '').split('.').reverse().join('')
        if (dA !== dB) return dA.localeCompare(dB)
        return (a.time || '').localeCompare(b.time || '')
    })
})

function calcEndTime(startTime) {
    if (!startTime) return ''
    const parts = String(startTime).split(':')
    if (parts.length !== 2) return ''
    const totalMin = parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10) + 45
    return `${String(Math.floor(totalMin / 60)).padStart(2, '0')}:${String(totalMin % 60).padStart(2, '0')}`
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
