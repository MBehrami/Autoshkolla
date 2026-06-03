<template>
    <div class="page-container">
        <!-- Page Header -->
        <div class="page-header">
            <div class="page-title">Dashboard</div>
            <div class="page-subtitle">Mirë se vini! Këtu keni një pasqyrë të shpejtë të aktivitetit.</div>
        </div>

        <!-- Summary Cards — admin only -->
        <v-row v-if="isAdmin">
            <v-col cols="12" sm="6" lg="4">
                <v-card class="stat-card">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <div class="stat-icon stat-icon--blue">
                            <v-icon icon="mdi-account-school" size="26"></v-icon>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ summary.totalCandidates }}</div>
                            <div class="stat-label">Gjithsej Kandidatë</div>
                        </div>
                        <v-spacer></v-spacer>
                        <v-chip v-if="summary.totalCandidates > 0" color="primary" variant="tonal" size="small" class="stat-badge">
                            <v-icon start size="14">mdi-trending-up</v-icon>
                            Aktiv
                        </v-chip>
                    </v-card-text>
                </v-card>
            </v-col>

            <v-col cols="12" sm="6" lg="4">
                <v-card class="stat-card">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <div class="stat-icon stat-icon--teal">
                            <v-icon icon="mdi-account-tie" size="26"></v-icon>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ summary.totalInstructors }}</div>
                            <div class="stat-label">Gjithsej Instruktorë</div>
                        </div>
                        <v-spacer></v-spacer>
                        <v-chip v-if="summary.totalInstructors > 0" color="success" variant="tonal" size="small" class="stat-badge">
                            <v-icon start size="14">mdi-check-circle</v-icon>
                            Aktiv
                        </v-chip>
                    </v-card-text>
                </v-card>
            </v-col>

            <v-col cols="12" sm="6" lg="4">
                <v-card class="stat-card">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <div class="stat-icon stat-icon--slate">
                            <v-icon icon="mdi-car" size="26"></v-icon>
                        </div>
                        <div class="stat-info">
                            <div class="stat-value">{{ summary.activeVehicles }}</div>
                            <div class="stat-label">Automjete Aktive</div>
                        </div>
                        <v-spacer></v-spacer>
                        <v-chip v-if="summary.activeVehicles > 0" color="secondary" variant="tonal" size="small" class="stat-badge">
                            <v-icon start size="14">mdi-check-circle</v-icon>
                            Aktiv
                        </v-chip>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- Quick Links -->
        <v-row class="mt-2">
            <v-col cols="12" md="6">
                <v-card>
                    <v-card-text class="pa-5">
                        <div class="d-flex align-center mb-4">
                            <v-icon icon="mdi-lightning-bolt" color="primary" class="mr-2"></v-icon>
                            <span class="text-subtitle-1 font-weight-bold" style="color: var(--slate-800);">Veprime të shpejta</span>
                        </div>
                        <div class="quick-links">
                            <v-btn variant="tonal" color="primary" class="text-none quick-link-btn" prepend-icon="mdi-account-plus" to="/candidates">
                                {{ isInstructor ? 'Kandidatët e Mi' : 'Kandidatët' }}
                            </v-btn>
                            <v-btn variant="tonal" color="success" class="text-none quick-link-btn" prepend-icon="mdi-calendar-clock" to="/schedules">
                                Oraret
                            </v-btn>
                            <v-btn v-if="!isInstructor" variant="tonal" color="warning" class="text-none quick-link-btn" prepend-icon="mdi-car-clock" to="/driving-sessions">
                                Vozitjet
                            </v-btn>
                            <v-btn v-if="!isInstructor" variant="tonal" color="info" class="text-none quick-link-btn" prepend-icon="mdi-file-document-outline" to="/daily-report">
                                Raporti
                            </v-btn>
                            <v-btn v-if="isInstructor" variant="tonal" color="warning" class="text-none quick-link-btn" prepend-icon="mdi-gas-station" to="/vehicle-fuel">
                                Derivatet
                            </v-btn>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="6">
                <v-card class="h-100">
                    <v-card-text class="pa-5">
                        <div class="d-flex align-center mb-4">
                            <v-icon icon="mdi-information-outline" color="secondary" class="mr-2"></v-icon>
                            <span class="text-subtitle-1 font-weight-bold" style="color: var(--slate-800);">Informacion</span>
                        </div>
                        <div class="info-items">
                            <div class="info-item">
                                <v-icon icon="mdi-calendar" size="18" color="primary" class="mr-3"></v-icon>
                                <span class="text-body-2" style="color: var(--slate-600);">Data e sotme: <strong style="color: var(--slate-800);">{{ todayDate }}</strong></span>
                            </div>
                            <div class="info-item">
                                <v-icon icon="mdi-account-circle" size="18" color="primary" class="mr-3"></v-icon>
                                <span class="text-body-2" style="color: var(--slate-600);">Përdoruesi: <strong style="color: var(--slate-800);">{{ profileName }}</strong></span>
                            </div>
                            <div class="info-item">
                                <v-icon icon="mdi-shield-account" size="18" color="primary" class="mr-3"></v-icon>
                                <span class="text-body-2" style="color: var(--slate-600);">Roli: <strong style="color: var(--slate-800);">{{ profileRole }}</strong></span>
                            </div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Today's Calendars ─── -->
        <v-row class="mt-2">
            <v-col cols="12" lg="6">
                <TodayCalendar
                    title="Vozitjet e planifikuara për sot"
                    icon="mdi-car-clock"
                    icon-color="#ea580c"
                    icon-bg="#fff7ed"
                    :events="drivingSessions"
                    :color-fn="getEventColor"
                    :title-fn="drivingTitle"
                    :sub-fn="drivingSub"
                    empty-message="Asnjë vozitje e planifikuar për sot"
                    empty-icon="mdi-car-off"
                />
            </v-col>
            <v-col cols="12" lg="6">
                <TodayCalendar
                    title="Orari i planifikuar për sot nga instruktorët"
                    icon="mdi-account-tie"
                    icon-color="#2563eb"
                    icon-bg="#eff6ff"
                    :events="instructorSessions"
                    :color-fn="getEventColor"
                    :title-fn="instructorTitle"
                    :sub-fn="instructorSub"
                    :legend="instructorLegend"
                    empty-message="Asnjë orar planifikuar për sot nga instruktorët"
                    empty-icon="mdi-calendar-account"
                />
            </v-col>
        </v-row>
    </div>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { useScheduleStore } from '@/store/ScheduleStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import TodayCalendar from './TodayCalendar.vue';

const userStore = useUserStore()
const scheduleStore = useScheduleStore()
const settingStore = useSettingStore()

const summary = ref({
    totalCandidates: 0,
    totalInstructors: 0,
    activeVehicles: 0
})

const profileInfo = JSON.parse(localStorage.getItem('profile') || '{}')
const profileName = profileInfo?.obj?.fullName || ''
const profileRole = profileInfo?.obj?.roleName || ''

const isAdmin = computed(() => {
    const role = profileRole
    return role === 'Admin' || role === 'SuperAdmin'
})

const isInstructor = computed(() => {
    return profileRole === 'Instructor'
})

const todayDate = new Date().toLocaleDateString('sq-AL', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
})

// ─────────────────────────────────────────────────────────────
//  Today's calendars (driving sessions + instructor schedule)
// ─────────────────────────────────────────────────────────────

// Instructor color palette — kept in sync with the Schedule module
const INSTRUCTOR_COLORS = [
    { bg: 'rgba(37,99,235,0.12)',  border: '#2563eb', text: '#1e40af' },
    { bg: 'rgba(16,185,129,0.12)', border: '#10b981', text: '#065f46' },
    { bg: 'rgba(168,85,247,0.12)', border: '#a855f7', text: '#6b21a8' },
    { bg: 'rgba(239,68,68,0.12)',  border: '#ef4444', text: '#991b1b' },
    { bg: 'rgba(245,158,11,0.12)', border: '#f59e0b', text: '#92400e' },
    { bg: 'rgba(6,182,212,0.12)',  border: '#06b6d4', text: '#155e75' },
    { bg: 'rgba(236,72,153,0.12)', border: '#ec4899', text: '#9d174d' },
    { bg: 'rgba(99,102,241,0.12)', border: '#6366f1', text: '#3730a3' },
    { bg: 'rgba(20,184,166,0.12)', border: '#14b8a6', text: '#115e59' },
    { bg: 'rgba(234,88,12,0.12)',  border: '#ea580c', text: '#9a3412' },
]
const DS_COLOR = { bg: 'rgba(255,152,0,0.15)', border: '#FF9800', text: '#E65100' }

const todayEvents = ref([])
const instructorColorMap = ref({})

const drivingSessions = computed(() => todayEvents.value.filter(e => e.eventType === 'driving-session'))
const instructorSessions = computed(() => todayEvents.value.filter(e => e.eventType !== 'driving-session'))

function assignInstructorColors() {
    const ids = [...new Set(instructorSessions.value.filter(e => e.instructorUserId).map(e => e.instructorUserId))]
    const map = {}
    ids.forEach((id, i) => { map[id] = INSTRUCTOR_COLORS[i % INSTRUCTOR_COLORS.length] })
    instructorColorMap.value = map
}

function getEventColor(ev) {
    if (ev.status === 'Cancelled') {
        return { background: '#FFEBEE', borderLeft: '3px solid #D32F2F', color: '#C62828' }
    }
    if (ev.eventType === 'driving-session') {
        return { background: DS_COLOR.bg, borderLeft: `3px solid ${DS_COLOR.border}`, color: DS_COLOR.text }
    }
    const c = instructorColorMap.value[ev.instructorUserId] || INSTRUCTOR_COLORS[0]
    return { background: c.bg, borderLeft: `3px solid ${c.border}`, color: c.text }
}

const instructorLegend = computed(() => {
    const map = instructorColorMap.value
    return Object.keys(map).map(id => {
        const numId = Number(id)
        const ev = instructorSessions.value.find(e => e.instructorUserId === numId)
        return { id: numId, name: ev?.instructorName || `Instruktor #${id}`, ...map[id] }
    })
})

function vehicleLabel(ev) {
    return ev.vehiclePlate + (ev.vehicleBrand ? ' – ' + ev.vehicleBrand : '')
}

// Driving sessions: instructors only see a blocked slot (no candidate details)
function drivingTitle(ev) {
    if (isInstructor.value) return vehicleLabel(ev) || 'Booked / Exam Slot'
    return ev.candidateName || 'Vozitje'
}
function drivingSub(ev) {
    if (isInstructor.value) return ''
    return vehicleLabel(ev)
}

// Instructor schedule: show candidate + instructor name
function instructorTitle(ev) {
    return ev.candidateName || '—'
}
function instructorSub(ev) {
    const parts = []
    if (ev.instructorName) parts.push(ev.instructorName)
    const v = vehicleLabel(ev)
    if (v) parts.push(v)
    return parts.join(' · ')
}

function normalizeEvent(r) {
    if (!r || typeof r !== 'object') return null
    return {
        id: r.id ?? r.Id ?? r.scheduleEventId ?? r.ScheduleEventId,
        eventDate: r.eventDate ?? r.EventDate ?? '',
        startTime: r.startTime ?? r.StartTime ?? '',
        endTime: r.endTime ?? r.EndTime ?? '',
        instructorUserId: r.instructorUserId ?? r.InstructorUserId ?? 0,
        instructorName: r.instructorName ?? r.InstructorName ?? '',
        candidateName: r.candidateName ?? r.CandidateName ?? '',
        vehiclePlate: r.vehiclePlate ?? r.VehiclePlate ?? '',
        vehicleBrand: r.vehicleBrand ?? r.VehicleBrand ?? '',
        status: r.status ?? r.Status ?? null,
        eventType: r.eventType ?? r.EventType ?? 'schedule',
    }
}

function fmtToday() {
    const d = new Date()
    return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`
}

function loadTodayEvents() {
    // Load only today's data (from === to === today) to keep the dashboard fast
    const today = fmtToday()
    return scheduleStore.getEvents(today, today, 0, 0)
        .then((res) => {
            const raw = res?.data?.data ?? res?.data?.Data ?? []
            todayEvents.value = raw.map(normalizeEvent).filter(Boolean)
            assignInstructorColors()
        })
        .catch(() => { todayEvents.value = [] })
}

// Refresh automatically when the user returns to the tab so newly added,
// edited or cancelled sessions show up without a manual reload.
function handleVisibility() {
    if (document.visibilityState === 'visible') loadTodayEvents()
}

onMounted(() => {
    const tasks = [loadTodayEvents()]

    if (isAdmin.value) {
        tasks.push(
            userStore.getDashboardSummary()
                .then((res) => {
                    const d = res?.data
                    if (d && typeof d === 'object') {
                        summary.value = {
                            totalCandidates: d.totalCandidates ?? 0,
                            totalInstructors: d.totalInstructors ?? 0,
                            activeVehicles: d.activeVehicles ?? 0
                        }
                    }
                })
                .catch(() => { /* keep defaults */ })
        )
    }

    Promise.allSettled(tasks).finally(() => settingStore.overlayToggle(false))
    document.addEventListener('visibilitychange', handleVisibility)
})

onBeforeUnmount(() => {
    document.removeEventListener('visibilitychange', handleVisibility)
})
</script>

<style scoped>
.stat-card {
    border: 1px solid var(--slate-200);
    transition: all 0.2s ease;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-lg) !important;
}

.stat-icon {
    width: 52px;
    height: 52px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.stat-icon--blue {
    background: #eff6ff;
    color: #2563eb;
}

.stat-icon--blue .v-icon { color: #2563eb; }

.stat-icon--teal {
    background: #f0fdfa;
    color: #0d9488;
}

.stat-icon--teal .v-icon { color: #0d9488; }

.stat-icon--slate {
    background: #f1f5f9;
    color: #475569;
}

.stat-icon--slate .v-icon { color: #475569; }

.stat-info {
    min-width: 0;
}

.stat-value {
    font-size: 1.75rem;
    font-weight: 800;
    color: var(--slate-800);
    line-height: 1.1;
}

.stat-label {
    font-size: 0.8125rem;
    color: var(--slate-500);
    font-weight: 500;
    margin-top: 2px;
}

.stat-badge {
    font-weight: 600;
}

.quick-links {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
}

.quick-link-btn {
    justify-content: flex-start;
    padding: 10px 16px !important;
    height: auto !important;
    min-height: 44px !important;
}

.info-items {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.info-item {
    display: flex;
    align-items: center;
}

@media (max-width: 600px) {
    .quick-links {
        grid-template-columns: 1fr;
    }

    .stat-value {
        font-size: 1.5rem;
    }
}
</style>
