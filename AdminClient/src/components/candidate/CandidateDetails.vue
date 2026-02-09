<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center">
            <span>Detajet e kandidatit</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4" v-if="loading">
            <v-progress-linear indeterminate></v-progress-linear>
        </v-card-text>
        <v-card-text class="pa-4" v-else-if="candidate">
            <!-- ─── Personal Info (visible to all roles) ─── -->
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.serialNumber" label="Serial Number (Nr. Rendor)" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.firstName" label="Emri" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.lastName" label="Mbiemri" variant="outlined" readonly></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.phoneNumber" label="Numri i telefonit" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.address" label="Adresa" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.vehicleType" label="Lloji i vetures" variant="outlined" readonly></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="candidate.categoryName" label="Kategoria" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col v-if="!isInstructor" cols="12" md="4">
                    <v-text-field :model-value="candidate.practicalHours" label="Orët praktike" variant="outlined" readonly></v-text-field>
                </v-col>
            </v-row>

            <!-- ─── Admin-only fields ─── -->
            <template v-if="!isInstructor">
                <v-row>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.parentName" label="Emri i prindit" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.dateOfBirth" label="Data e lindjes" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.personalNumber" label="Numri personal" variant="outlined" readonly></v-text-field>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.placeOfBirth" label="Vendi i lindjes" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.instructorName" label="Instruktori" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.paymentMethod" label="Metoda e pagesës" variant="outlined" readonly></v-text-field>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.totalServiceAmount" label="Pagesa e shërbimit" variant="outlined" readonly></v-text-field>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col cols="12" md="3">
                        <v-text-field :model-value="candidate.docWithdrawalAmount" label="Pagesa e terheqjes së dokumentëve" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-text-field :model-value="candidate.docWithdrawalDate" label="Data e pagesës së terheqjes" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-text-field :model-value="candidate.drivingPaymentAmount" label="Pagesa e vozitjes" variant="outlined" readonly></v-text-field>
                    </v-col>
                    <v-col cols="12" md="3">
                        <v-text-field :model-value="candidate.drivingPaymentDate" label="Data e pagesës së vozitjes" variant="outlined" readonly></v-text-field>
                    </v-col>
                </v-row>

                <!-- ─── Installments (Admin only) ─── -->
                <v-divider class="my-4"></v-divider>
                <h3 class="mb-3">Këstet (Installments)</h3>
                <v-table v-if="installments && installments.length > 0" density="comfortable" class="elevation-1 rounded mb-2">
                    <thead>
                        <tr>
                            <th>Nr.</th>
                            <th>Shuma</th>
                            <th>Data</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="inst in installments" :key="inst.installmentId">
                            <td>{{ inst.installmentNumber }}</td>
                            <td>{{ inst.amount }} &euro;</td>
                            <td>{{ inst.installmentDate }}</td>
                        </tr>
                    </tbody>
                </v-table>
                <v-alert v-else type="info" density="compact" variant="tonal">Nuk ka këste</v-alert>
            </template>

            <!-- ─── Practical Lessons (visible to all roles) ─── -->
            <v-divider class="my-4"></v-divider>
            <h3 class="mb-3">Orët praktike <span class="text-caption text-medium-emphasis">({{ sortedLessons.length }} orë)</span></h3>
            <v-table v-if="sortedLessons.length > 0" density="comfortable" class="elevation-1 rounded mb-2">
                <thead>
                    <tr>
                        <th>Nr.</th>
                        <th>Data</th>
                        <th>Fillimi</th>
                        <th>Mbarimi</th>
                        <th>Instruktori</th>
                        <th>Vetura</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(lesson, index) in sortedLessons" :key="lesson.practicalLessonId || index">
                        <td>{{ index + 1 }}</td>
                        <td>{{ lesson.lessonDate || lesson.date }}</td>
                        <td>{{ lesson.time }}</td>
                        <td>{{ lesson.endTime || calcEndTime(lesson.time) }}</td>
                        <td>{{ lesson.instructorName }}</td>
                        <td>{{ lesson.vehicle || '–' }}</td>
                    </tr>
                </tbody>
            </v-table>
            <v-alert v-else type="info" density="compact" variant="tonal">Nuk ka orë praktike</v-alert>

            <!-- ─── Driving Sessions History (Admin only) ─── -->
            <template v-if="!isInstructor">
                <v-divider class="my-4"></v-divider>
                <h3 class="mb-3">Vozitjet (Driving Sessions) <span class="text-caption text-medium-emphasis">({{ drivingSessions.length }})</span></h3>
                <v-table v-if="drivingSessions.length > 0" density="comfortable" class="elevation-1 rounded mb-2">
                    <thead>
                        <tr>
                            <th>Data</th>
                            <th>Ora</th>
                            <th>Vetura</th>
                            <th>Pagesa</th>
                            <th>Data e pagesës</th>
                            <th>Statusi</th>
                            <th>Egzamineri</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="ds in drivingSessions" :key="ds.drivingSessionId">
                            <td>{{ ds.drivingDate }}</td>
                            <td>{{ ds.drivingTime }}</td>
                            <td>{{ ds.vehiclePlate }} {{ ds.vehicleBrand ? '– ' + ds.vehicleBrand : '' }}</td>
                            <td>
                                <span v-if="ds.paymentAmount > 0" class="text-green-darken-2 font-weight-medium">{{ ds.paymentAmount }} &euro;</span>
                                <span v-else class="text-medium-emphasis">–</span>
                            </td>
                            <td>{{ ds.paymentDate || '–' }}</td>
                            <td>
                                <v-chip v-if="ds.status" :color="statusColor(ds.status)" size="small" variant="tonal">{{ ds.status }}</v-chip>
                                <span v-else class="text-medium-emphasis">–</span>
                            </td>
                            <td>{{ ds.examiner || '–' }}</td>
                        </tr>
                    </tbody>
                </v-table>
                <v-alert v-else type="info" density="compact" variant="tonal">Nuk ka vozitje</v-alert>

                <!-- ─── All Payments History (Admin only) ─── -->
                <v-divider class="my-4"></v-divider>
                <h3 class="mb-3">Historiku i pagesave (Payments)
                    <span class="text-caption text-medium-emphasis">({{ payments.length }} pagesa, Total: {{ totalPayments }} &euro;)</span>
                </h3>
                <v-table v-if="payments.length > 0" density="comfortable" class="elevation-1 rounded mb-2">
                    <thead>
                        <tr>
                            <th>Lloji</th>
                            <th>Përshkrimi</th>
                            <th>Shuma</th>
                            <th>Data</th>
                            <th>Statusi</th>
                            <th>Egzamineri</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(p, idx) in payments" :key="idx">
                            <td>
                                <v-chip :color="paymentTypeColor(p.type)" size="small" variant="tonal">{{ p.type }}</v-chip>
                            </td>
                            <td>{{ p.description }}</td>
                            <td class="text-green-darken-2 font-weight-medium">{{ p.amount }} &euro;</td>
                            <td>{{ p.date || '–' }}</td>
                            <td>
                                <v-chip v-if="p.status" :color="statusColor(p.status)" size="x-small" variant="tonal">{{ p.status }}</v-chip>
                                <span v-else>–</span>
                            </td>
                            <td>{{ p.examiner || '–' }}</td>
                        </tr>
                    </tbody>
                </v-table>
                <v-alert v-else type="info" density="compact" variant="tonal">Nuk ka pagesa</v-alert>
            </template>
        </v-card-text>

        <v-divider></v-divider>
        <v-card-actions class="pa-4">
            <v-btn
                v-if="!isInstructor"
                color="error"
                variant="tonal"
                class="text-capitalize"
                prepend-icon="mdi-file-pdf-box"
                :loading="downloadingPdf"
                @click="downloadApplication"
            >
                Shkarko Aplikacionin
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn text="Close" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { downloadApplicationPdf } from '@/utils/applicationPdf';
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
const drivingSessions = ref([])
const payments = ref([])
const downloadingPdf = ref(false)

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}')
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || ''
        return role === 'Instructor'
    } catch { return false }
})

// Sort lessons by date asc then time asc
const sortedLessons = computed(() => {
    const list = practicalLessons.value || []
    return [...list].sort((a, b) => {
        const dA = (a.lessonDate || '').split('.').reverse().join('')
        const dB = (b.lessonDate || '').split('.').reverse().join('')
        if (dA !== dB) return dA.localeCompare(dB)
        return (a.time || '').localeCompare(b.time || '')
    })
})

// Total payments amount
const totalPayments = computed(() => {
    return (payments.value || []).reduce((sum, p) => sum + (p.amount || 0), 0)
})

function calcEndTime(startTime) {
    if (!startTime) return ''
    const parts = String(startTime).split(':')
    if (parts.length !== 2) return ''
    const totalMin = parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10) + 45
    return `${String(Math.floor(totalMin / 60)).padStart(2, '0')}:${String(totalMin % 60).padStart(2, '0')}`
}

function statusColor(status) {
    if (!status) return 'grey'
    const s = status.toLowerCase()
    if (s === 'kaloi') return 'green'
    if (s === 'deshtoi') return 'red'
    if (s === 'anuloi') return 'orange'
    return 'grey'
}

function paymentTypeColor(type) {
    if (!type) return 'grey'
    if (type.includes('Installment') || type.includes('Registration')) return 'blue'
    if (type.includes('Document')) return 'purple'
    if (type.includes('Driving Session')) return 'teal'
    if (type.includes('Driving Payment')) return 'indigo'
    return 'grey'
}

const loadCandidateDetails = () => {
    candidateStore.getCandidateDetails(props.candidateId)
        .then((response) => {
            candidate.value = response.data.candidate
            installments.value = response.data.installments || []
            practicalLessons.value = response.data.practicalLessons || []
            drivingSessions.value = response.data.drivingSessions || []
            payments.value = response.data.payments || []
        })
        .catch(() => {
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading candidate details' })
        })
}

const downloadApplication = async () => {
    if (!candidate.value) return
    downloadingPdf.value = true
    try {
        await downloadApplicationPdf(candidate.value)
    } catch (err) {
        console.error('Download application error:', err)
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë shkarkimit të aplikacionit' })
    } finally {
        downloadingPdf.value = false
    }
}

const closeDialog = () => emit('close')

watch(() => props.candidateId, () => {
    if (props.candidateId) loadCandidateDetails()
}, { immediate: true })

onMounted(() => {
    if (props.candidateId) loadCandidateDetails()
})
</script>
