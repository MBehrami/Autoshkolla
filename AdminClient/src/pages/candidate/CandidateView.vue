<template>
    <div class="page-container">
        <!-- Header -->
        <div class="page-header d-flex flex-wrap align-center justify-space-between ga-3 mb-6">
            <div class="d-flex align-center ga-3">
                <v-btn icon variant="text" @click="$router.push('/candidates')" class="mr-1">
                    <v-icon>mdi-arrow-left</v-icon>
                </v-btn>
                <div>
                    <div class="page-title">{{ candidate ? (candidate.firstName + ' ' + candidate.lastName) : 'Detajet e kandidatit' }}</div>
                    <div class="page-subtitle">Nr. Rendor: {{ candidate?.serialNumber || '–' }}</div>
                </div>
            </div>
            <div class="d-flex ga-2 flex-wrap">
                <v-btn v-if="!isInstructor" variant="tonal" color="error" class="text-none" prepend-icon="mdi-file-pdf-box"
                    size="small" :loading="downloadingPdf" @click="downloadApplication">
                    Shkarko Aplikacionin
                </v-btn>
                <v-btn v-if="!isInstructor" variant="tonal" color="deep-purple" class="text-none" prepend-icon="mdi-file-document-outline"
                    size="small" :loading="downloadingContract" @click="downloadContract">
                    Shkarko Kontratën
                </v-btn>
                <v-btn v-if="!isInstructor" variant="tonal" color="teal" class="text-none" prepend-icon="mdi-certificate-outline"
                    size="small" :loading="downloadingCertificate" @click="downloadCertificate">
                    Shkarko Vërtetimin
                </v-btn>
                <v-btn v-if="!isInstructor" variant="tonal" color="blue-grey" class="text-none" prepend-icon="mdi-card-account-details-outline"
                    size="small" disabled>
                    Shkarko Kartelën
                    <v-tooltip activator="parent" location="bottom">Së shpejti</v-tooltip>
                </v-btn>
                <v-btn v-if="!isInstructor" variant="flat" color="primary" class="text-none" prepend-icon="mdi-pencil"
                    size="small" @click="$router.push(`/candidates/${candidateId}/edit`)">
                    Ndrysho
                </v-btn>
            </div>
        </div>

        <v-progress-linear v-if="loading" indeterminate color="primary" class="mb-4"></v-progress-linear>

        <template v-if="candidate">
            <v-row>
                <!-- Left Column -->
                <v-col cols="12" lg="8">
                    <!-- Personal Information -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Të dhënat personale</div>
                            <v-row>
                                <v-col cols="12" sm="6" md="4">
                                    <div class="detail-label">Emri</div>
                                    <div class="detail-value">{{ candidate.firstName || '–' }}</div>
                                </v-col>
                                <v-col cols="12" sm="6" md="4">
                                    <div class="detail-label">Mbiemri</div>
                                    <div class="detail-value">{{ candidate.lastName || '–' }}</div>
                                </v-col>
                                <v-col v-if="!isInstructor" cols="12" sm="6" md="4">
                                    <div class="detail-label">Emri i prindit</div>
                                    <div class="detail-value">{{ candidate.parentName || '–' }}</div>
                                </v-col>
                                <v-col v-if="!isInstructor" cols="12" sm="6" md="4">
                                    <div class="detail-label">Data e lindjes</div>
                                    <div class="detail-value">{{ candidate.dateOfBirth || '–' }}</div>
                                </v-col>
                                <v-col v-if="!isInstructor" cols="12" sm="6" md="4">
                                    <div class="detail-label">Numri personal</div>
                                    <div class="detail-value">{{ candidate.personalNumber || '–' }}</div>
                                </v-col>
                                <v-col v-if="!isInstructor" cols="12" sm="6" md="4">
                                    <div class="detail-label">Vendi i lindjes</div>
                                    <div class="detail-value">{{ candidate.placeOfBirth || '–' }}</div>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Contact & Municipality -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Kontakti & Komuna</div>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <div class="detail-label">Numri i telefonit</div>
                                    <div class="detail-value">{{ candidate.phoneNumber || '–' }}</div>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <div class="detail-label">Komuna</div>
                                    <div class="detail-value">{{ candidate.address || '–' }}</div>
                                </v-col>
                            </v-row>
                        </v-card-text>
                    </v-card>

                    <!-- Practical Lessons -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Orët praktike <v-chip size="x-small" variant="tonal" color="primary" class="ml-2">{{ sortedLessons.length }}</v-chip></div>
                            <v-table v-if="sortedLessons.length > 0" density="comfortable" class="detail-table">
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
                        </v-card-text>
                    </v-card>

                    <!-- Driving Sessions (Admin) -->
                    <v-card v-if="!isInstructor" class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title d-flex flex-wrap align-center ga-2">
                                <span>Vozitjet</span>
                                <v-chip size="x-small" variant="tonal" color="primary">{{ drivingSessions.length }}</v-chip>
                                <v-chip v-if="drivingPaymentTotal > 0" size="x-small" variant="tonal" color="success">
                                    Total pagesat: {{ drivingPaymentTotal.toFixed(2) }} &euro;
                                </v-chip>
                                <v-chip v-if="waitingCount > 0" size="x-small" variant="tonal" color="warning">
                                    {{ waitingCount }} në pritje
                                </v-chip>
                            </div>
                            <v-table v-if="drivingSessions.length > 0" density="comfortable" class="detail-table">
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
                                        <td>
                                            <template v-if="ds.drivingDate">{{ ds.drivingDate }}</template>
                                            <v-chip v-else size="x-small" color="warning" variant="tonal">Listë pritjeje</v-chip>
                                        </td>
                                        <td>{{ ds.drivingTime || '–' }}</td>
                                        <td>{{ ds.vehiclePlate }} {{ ds.vehicleBrand ? '– ' + ds.vehicleBrand : '' }}</td>
                                        <td>
                                            <span v-if="ds.paymentAmount > 0" class="font-weight-medium" style="color:#10b981">{{ ds.paymentAmount }} &euro;</span>
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
                        </v-card-text>
                    </v-card>

                    <!-- Payments History (Admin) -->
                    <v-card v-if="!isInstructor" class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">
                                Historiku i pagesave
                                <v-chip size="x-small" variant="tonal" color="success" class="ml-2">Total: {{ totalPayments }} &euro;</v-chip>
                            </div>
                            <v-table v-if="payments.length > 0" density="comfortable" class="detail-table">
                                <thead>
                                    <tr>
                                        <th>Lloji</th>
                                        <th>Përshkrimi</th>
                                        <th>Shuma</th>
                                        <th>Data</th>
                                        <th>Statusi</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="(p, idx) in payments" :key="idx">
                                        <td><v-chip :color="paymentTypeColor(p.type)" size="x-small" variant="tonal">{{ p.type }}</v-chip></td>
                                        <td>{{ p.description }}</td>
                                        <td class="font-weight-medium" style="color:#10b981">{{ p.amount }} &euro;</td>
                                        <td>{{ p.date || '–' }}</td>
                                        <td>
                                            <v-chip v-if="p.status" :color="statusColor(p.status)" size="x-small" variant="tonal">{{ p.status }}</v-chip>
                                            <span v-else>–</span>
                                        </td>
                                    </tr>
                                </tbody>
                            </v-table>
                            <v-alert v-else type="info" density="compact" variant="tonal">Nuk ka pagesa</v-alert>
                        </v-card-text>
                    </v-card>
                </v-col>

                <!-- Right Column — Summary Sidebar -->
                <v-col cols="12" lg="4">
                    <!-- Category & Vehicle -->
                    <v-card class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Kategoria & Vetura</div>
                            <div class="summary-row">
                                <v-icon size="18" color="primary" class="mr-2">mdi-tag</v-icon>
                                <span class="detail-label-inline">Kategoria:</span>
                                <span class="detail-value-inline">{{ candidate.categoryName || '–' }}</span>
                            </div>
                            <div class="summary-row">
                                <v-icon size="18" color="primary" class="mr-2">mdi-car</v-icon>
                                <span class="detail-label-inline">Lloji i vetures:</span>
                                <span class="detail-value-inline">{{ candidate.vehicleType || '–' }}</span>
                            </div>
                            <div v-if="!isInstructor" class="summary-row">
                                <v-icon size="18" color="primary" class="mr-2">mdi-account-tie</v-icon>
                                <span class="detail-label-inline">Instruktori:</span>
                                <span class="detail-value-inline">{{ candidate.instructorName || '–' }}</span>
                            </div>
                            <div class="summary-row">
                                <v-icon size="18" color="primary" class="mr-2">mdi-clock-outline</v-icon>
                                <span class="detail-label-inline">Orët praktike:</span>
                                <span class="detail-value-inline">{{ candidate.practicalHours ?? '–' }}</span>
                            </div>
                        </v-card-text>
                    </v-card>

                    <!-- Payment Info (Admin) -->
                    <v-card v-if="!isInstructor" class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Pagesat</div>
                            <div class="summary-row">
                                <v-icon size="18" color="success" class="mr-2">mdi-cash-multiple</v-icon>
                                <span class="detail-label-inline">Pagesa e shërbimit:</span>
                                <span class="detail-value-inline font-weight-bold">{{ candidate.totalServiceAmount || 0 }} &euro;</span>
                            </div>
                            <div v-if="!isInstructor" class="summary-row">
                                <v-icon size="18" color="secondary" class="mr-2">mdi-credit-card-outline</v-icon>
                                <span class="detail-label-inline">Metoda:</span>
                                <span class="detail-value-inline">{{ candidate.paymentMethod || '–' }}</span>
                            </div>
                            <v-divider class="my-3"></v-divider>
                            <div class="summary-row">
                                <span class="detail-label-inline">Terheqja e dokumentëve:</span>
                                <span class="detail-value-inline">{{ candidate.docWithdrawalAmount || '–' }} &euro; <span v-if="candidate.docWithdrawalDate" class="text-medium-emphasis text-caption">({{ candidate.docWithdrawalDate }})</span></span>
                            </div>
                            <div class="summary-row">
                                <span class="detail-label-inline">Pagesa e vozitjes:</span>
                                <span class="detail-value-inline">{{ candidate.drivingPaymentAmount || '–' }} &euro; <span v-if="candidate.drivingPaymentDate" class="text-medium-emphasis text-caption">({{ candidate.drivingPaymentDate }})</span></span>
                            </div>
                        </v-card-text>
                    </v-card>

                    <!-- Installments (Admin) -->
                    <v-card v-if="!isInstructor && installments.length > 0" class="mb-4">
                        <v-card-text class="pa-5">
                            <div class="section-title">Këstet</div>
                            <div v-for="inst in installments" :key="inst.installmentId" class="installment-row">
                                <div class="d-flex justify-space-between align-center">
                                    <span class="text-body-2 font-weight-medium">Kësti {{ inst.installmentNumber }}</span>
                                    <span class="text-body-2 font-weight-bold" style="color:#10b981">{{ inst.amount }} &euro;</span>
                                </div>
                                <div class="text-caption text-medium-emphasis">{{ inst.installmentDate || 'Pa datë' }}</div>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>
        </template>
    </div>
</template>

<script setup>
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { downloadApplicationPdf } from '@/utils/applicationPdf';
import { downloadContractPdf } from '@/utils/contractPdf';
import { downloadCertificatePdf } from '@/utils/certificatePdf';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute()
const candidateId = computed(() => Number(route.params.id))

const candidateStore = useCandidateStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(candidateStore)

const candidate = ref(null)
const installments = ref([])
const practicalLessons = ref([])
const drivingSessions = ref([])
const payments = ref([])
const downloadingPdf = ref(false)
const downloadingContract = ref(false)
const downloadingCertificate = ref(false)

const isInstructor = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}')
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || ''
        return role === 'Instructor'
    } catch { return false }
})

const sortedLessons = computed(() => {
    const list = practicalLessons.value || []
    return [...list].sort((a, b) => {
        const dA = (a.lessonDate || '').split('.').reverse().join('')
        const dB = (b.lessonDate || '').split('.').reverse().join('')
        if (dA !== dB) return dA.localeCompare(dB)
        return (a.time || '').localeCompare(b.time || '')
    })
})

const totalPayments = computed(() => {
    return (payments.value || []).reduce((sum, p) => sum + (p.amount || 0), 0)
})

const drivingPaymentTotal = computed(() => {
    return (drivingSessions.value || []).reduce((sum, ds) => sum + (ds.paymentAmount || 0), 0)
})

const waitingCount = computed(() => {
    return (drivingSessions.value || []).filter(ds => !ds.drivingDate).length
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
    if (s === 'kaloi') return 'success'
    if (s === 'deshtoi') return 'error'
    if (s === 'anuloi') return 'warning'
    return 'secondary'
}

function paymentTypeColor(type) {
    if (!type) return 'secondary'
    if (type.includes('Installment') || type.includes('Registration')) return 'primary'
    if (type.includes('Document')) return 'accent'
    if (type.includes('Driving Session')) return 'info'
    if (type.includes('Driving Payment')) return 'secondary'
    return 'secondary'
}

const loadCandidateDetails = () => {
    candidateStore.getCandidateDetails(candidateId.value)
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
        console.error('Download application error:')
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë shkarkimit të aplikacionit' })
    } finally {
        downloadingPdf.value = false
    }
}

const downloadContract = async () => {
    if (!candidate.value) return
    downloadingContract.value = true
    try {
        await downloadContractPdf(candidate.value)
    } catch (err) {
        console.error('Download contract error:')
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë shkarkimit të kontratës' })
    } finally {
        downloadingContract.value = false
    }
}

const downloadCertificate = async () => {
    if (!candidate.value) return
    downloadingCertificate.value = true
    try {
        await downloadCertificatePdf(candidate.value)
    } catch (err) {
        console.error('Download certificate error:')
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë shkarkimit të vërtetimit' })
    } finally {
        downloadingCertificate.value = false
    }
}

onMounted(() => {
    if (candidateId.value) loadCandidateDetails()
})
</script>

<style scoped>
.detail-label {
    font-size: 0.75rem;
    font-weight: 500;
    color: var(--slate-400);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    margin-bottom: 4px;
}

.detail-value {
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--slate-800);
}

.summary-row {
    display: flex;
    align-items: center;
    padding: 8px 0;
    gap: 4px;
    flex-wrap: wrap;
}

.summary-row + .summary-row {
    border-top: 1px solid var(--slate-100);
}

.detail-label-inline {
    font-size: 0.8125rem;
    color: var(--slate-500);
    white-space: nowrap;
}

.detail-value-inline {
    font-size: 0.8125rem;
    font-weight: 600;
    color: var(--slate-800);
    margin-left: auto;
}

.installment-row {
    padding: 10px 0;
}

.installment-row + .installment-row {
    border-top: 1px solid var(--slate-100);
}

.detail-table {
    border: 1px solid var(--slate-200);
    border-radius: 8px;
    overflow: hidden;
}

.detail-table :deep(th) {
    background: var(--slate-50) !important;
    font-size: 0.75rem !important;
    font-weight: 600 !important;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--slate-500) !important;
}

.detail-table :deep(td) {
    font-size: 0.8125rem !important;
    color: var(--slate-700) !important;
}

@media (max-width: 600px) {
    .detail-label {
        font-size: 0.6875rem;
        margin-bottom: 2px;
    }
    .detail-value {
        font-size: 0.8125rem;
    }
    .summary-row {
        flex-direction: column;
        align-items: flex-start;
        gap: 2px;
    }
    .detail-value-inline {
        margin-left: 0;
    }
    .detail-table :deep(th),
    .detail-table :deep(td) {
        padding: 6px 8px !important;
        font-size: 0.7rem !important;
    }
    .detail-table {
        overflow-x: auto;
    }
}
</style>
