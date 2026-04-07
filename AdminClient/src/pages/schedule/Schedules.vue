<template>
    <div class="page-container">
        <div class="page-header mb-6">
            <div class="page-title">Oraret</div>
        </div>
        <!-- ─── Toolbar ─── -->
        <v-card elevation="2" rounded="lg" class="mb-4 schedules-header-card">
            <v-toolbar density="comfortable" flat class="schedules-toolbar">
                <div class="schedules-actions-wrap">
                    <div class="schedules-nav-group">
                        <v-btn variant="outlined" size="small" @click="goToday">Sot</v-btn>
                        <v-btn icon variant="text" size="small" @click="goPrev">
                            <v-icon icon="mdi-chevron-left"></v-icon>
                        </v-btn>
                        <v-btn icon variant="text" size="small" @click="goNext">
                            <v-icon icon="mdi-chevron-right"></v-icon>
                        </v-btn>
                        <span class="text-h6 font-weight-medium ml-2 sch-header-title">{{ headerTitle }}</span>
                    </div>
                    <div class="schedules-controls-wrap">
                        <v-select v-if="isAdmin" v-model="filterInstructor" :items="instructorOptions"
                            item-title="fullName" item-value="userId" label="Instruktorët" variant="outlined"
                            density="compact" hide-details clearable class="sch-filter"
                            @update:model-value="loadEvents"></v-select>
                        <v-select v-model="filterVehicle" :items="vehicleOptions" item-title="label"
                            item-value="vehicleId" label="Automjeti" variant="outlined" density="compact" hide-details
                            clearable class="sch-filter" @update:model-value="loadEvents"></v-select>
                        <v-btn-toggle v-model="viewMode" mandatory density="compact" variant="outlined" color="primary" class="sch-view-toggle">
                            <v-btn value="day" size="small">Dita</v-btn>
                            <v-btn value="week" size="small">Javë</v-btn>
                        </v-btn-toggle>
                        <v-btn color="primary" variant="elevated" class="text-capitalize sch-add-btn" prepend-icon="mdi-plus"
                            @click="openCreateDialog(null)">
                            Shto orë
                        </v-btn>
                    </div>
                </div>
            </v-toolbar>
        </v-card>

        <!-- ─── Legend — instructor colors ─── -->
        <div class="d-flex ga-3 mb-3 flex-wrap align-center">
            <v-chip v-for="inst in instructorLegend" :key="inst.id" size="small" variant="flat"
                :style="{ background: inst.bg, color: inst.text, border: '1px solid ' + inst.border }">
                <v-icon start icon="mdi-circle" size="10" :style="{ color: inst.border }"></v-icon>
                {{ inst.name }}
            </v-chip>
            <v-chip size="small" color="orange" variant="tonal">
                <v-icon start icon="mdi-circle" size="10"></v-icon>{{ isAdmin ? 'Vozitjet' : 'Booked / Exam Slot' }}
            </v-chip>
        </div>

        <!-- ─── Day View ─── -->
        <v-card v-if="viewMode === 'day'" elevation="2" rounded="lg" class="calendar-card">
            <div class="day-grid">
                <div class="day-header">
                    <div class="time-gutter-header"></div>
                    <div class="day-col-header text-center">
                        <div class="text-body-2 text-medium-emphasis">{{ dayLabel(currentDate) }}</div>
                        <div class="text-h5 font-weight-bold" :class="{ 'text-primary': isToday(currentDate) }">
                            {{ currentDate.getDate() }}
                        </div>
                    </div>
                </div>
                <div class="day-body" ref="dayBodyRef">
                    <div v-for="hour in hours" :key="hour" class="time-row">
                        <div class="time-gutter">{{ formatHour(hour) }}</div>
                        <div class="time-cell" @click="handleCellClick(currentDate, hour)">
                            <div class="time-cell-events">
                                <div v-for="ev in getEventsForHour(currentDate, hour)" :key="ev.id + ev.eventType"
                                    class="cal-event" :style="getEventColor(ev)"
                                    @click.stop="openEventDetail(ev)">
                                    <div class="cal-event-time">{{ ev.startTime }}–{{ ev.endTime }}</div>
                                    <div class="cal-event-title text-truncate">{{ eventTitle(ev) }}</div>
                                    <div class="cal-event-sub text-truncate">{{ ev.vehiclePlate }}{{ ev.vehicleBrand ? ' – ' + ev.vehicleBrand : '' }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </v-card>

        <!-- ─── Week View ─── -->
        <v-card v-if="viewMode === 'week'" elevation="2" rounded="lg" class="calendar-card">
            <div class="week-grid">
                <div class="week-header">
                    <div class="time-gutter-header"></div>
                    <div v-for="d in weekDays" :key="d.toISOString()" class="week-col-header text-center">
                        <div class="text-body-2 text-medium-emphasis">{{ dayLabel(d) }}</div>
                        <div class="text-subtitle-1 font-weight-bold" :class="{ 'text-primary': isToday(d) }">
                            {{ d.getDate() }}
                        </div>
                    </div>
                </div>
                <div class="week-body" ref="weekBodyRef">
                    <div v-for="hour in hours" :key="hour" class="time-row">
                        <div class="time-gutter">{{ formatHour(hour) }}</div>
                        <div v-for="d in weekDays" :key="d.toISOString() + hour" class="time-cell week-cell"
                            @click="handleCellClick(d, hour)">
                            <div class="time-cell-events">
                                <div v-for="ev in getEventsForHour(d, hour)" :key="ev.id + ev.eventType"
                                    class="cal-event" :style="getEventColor(ev)"
                                    @click.stop="openEventDetail(ev)">
                                    <div class="cal-event-time">{{ ev.startTime }}</div>
                                    <div class="cal-event-title text-truncate">{{ eventTitle(ev) }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </v-card>

        <!-- ─── Event Detail / Edit Dialog ─── -->
        <v-dialog v-model="detailDialog" max-width="550" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon :icon="detailEvent?.eventType === 'driving-session' ? 'mdi-car-clock' : 'mdi-calendar-clock'"
                        :color="detailEvent?.eventType === 'driving-session' ? 'orange' : 'primary'"></v-icon>
                    <span>{{ isDsBlockedForInstructor(detailEvent) ? 'Booked / Exam Slot' : (detailEvent?.eventType === 'driving-session' ? 'Driving Session' : 'Schedule Event') }}</span>
                    <v-spacer></v-spacer>
                    <v-btn v-if="canEditEvent(detailEvent)" icon variant="text" size="small" color="primary"
                        @click="openEditFromDetail">
                        <v-icon icon="mdi-pencil" size="18"></v-icon>
                    </v-btn>
                    <v-btn v-if="canDeleteEvent(detailEvent)" icon variant="text" size="small" color="error"
                        @click="confirmDeleteFromDetail">
                        <v-icon icon="mdi-delete" size="18"></v-icon>
                    </v-btn>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-5" v-if="detailEvent">
                    <v-row dense>
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis">Data</div><div class="font-weight-medium">{{ detailEvent.eventDate }}</div></v-col>
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis">Ora</div><div class="font-weight-medium">{{ detailEvent.startTime }} – {{ detailEvent.endTime }}</div></v-col>
                        <v-col cols="6" v-if="isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Statusi</div>
                            <div class="font-weight-medium">Booked / Exam Slot</div>
                        </v-col>
                        <v-col cols="6" v-if="!isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Kandidati</div>
                            <div class="font-weight-medium">{{ detailEvent.candidateName }}</div>
                        </v-col>
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis mt-2">Automjeti</div><div class="font-weight-medium">{{ detailEvent.vehiclePlate }}{{ detailEvent.vehicleBrand ? ' – ' + detailEvent.vehicleBrand : '' }}</div></v-col>
                        <v-col cols="6" v-if="!isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Instruktori</div>
                            <div class="font-weight-medium">{{ detailEvent.instructorName || '—' }}</div>
                        </v-col>
                        <v-col cols="12" v-if="detailEvent.notes && !isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Shënime</div>
                            <div class="font-weight-medium">{{ detailEvent.notes }}</div>
                        </v-col>
                    </v-row>
                </v-card-text>
                <v-card-actions class="pa-4"><v-spacer></v-spacer><v-btn variant="text" @click="detailDialog = false">Mbyll</v-btn></v-card-actions>
            </v-card>
        </v-dialog>

        <!-- ─── Create / Edit Dialog ─── -->
        <v-dialog v-model="formDialog" max-width="700" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-calendar-plus" color="primary"></v-icon>
                    <span>{{ editingId ? 'Ndrysho orarin' : 'Regjistro orarin' }}</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <v-form ref="formRef" @submit.prevent="saveEvent">
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-menu v-model="fDateMenu" :close-on-content-click="false" location="bottom" min-width="auto">
                                    <template v-slot:activator="{ props: mp }">
                                        <v-text-field :model-value="form.eventDate" label="Data" variant="outlined"
                                            density="compact" prepend-inner-icon="mdi-calendar" readonly
                                            :rules="[v => !!v || 'Required']" v-bind="mp"></v-text-field>
                                    </template>
                                    <v-date-picker v-model="fDateModel" color="primary" @update:model-value="handleFormDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                            <v-col cols="6" md="3">
                                <v-select v-model="form.startTime" :items="timeSlots" label="Fillimi" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-clock-start"></v-select>
                            </v-col>
                            <v-col cols="6" md="3">
                                <v-select v-model="form.endTime" :items="timeSlots" label="Përfundimi" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-clock-end"></v-select>
                            </v-col>
                            <v-col cols="12" md="6" v-if="isAdmin">
                                <v-select v-model="form.instructorUserId" :items="instructorOptions"
                                    item-title="fullName" item-value="userId" label="Instruktori" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-account-tie"></v-select>
                            </v-col>
                            <v-col cols="12" :md="isAdmin ? 6 : 12">
                                <v-switch
                                    v-model="manualCandidate"
                                    label="Kandidati nuk gjendet? Shkruaj manualisht"
                                    color="primary"
                                    density="compact"
                                    hide-details
                                    class="mb-2"
                                ></v-switch>
                            </v-col>
                            <v-col v-if="!manualCandidate" cols="12" md="6">
                                <v-autocomplete v-model="form.candidateId" :items="candidateOptions"
                                    item-title="fullName" item-value="candidateId" label="Kandidati *" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Zgjidhni kandidatin']"
                                    prepend-inner-icon="mdi-account-search"
                                    no-data-text="Nuk u gjet – aktivizo shkruaj manualisht"></v-autocomplete>
                            </v-col>
                            <template v-if="manualCandidate">
                                <v-col cols="12" md="3">
                                    <v-text-field
                                        v-model="form.manualFirstName"
                                        label="Emri *"
                                        variant="outlined"
                                        density="compact"
                                        :rules="[v => !!v || 'Shkruani emrin']"
                                        prepend-inner-icon="mdi-account"
                                    ></v-text-field>
                                </v-col>
                                <v-col cols="12" md="3">
                                    <v-text-field
                                        v-model="form.manualLastName"
                                        label="Mbiemri *"
                                        variant="outlined"
                                        density="compact"
                                        :rules="[v => !!v || 'Shkruani mbiemrin']"
                                        prepend-inner-icon="mdi-account"
                                    ></v-text-field>
                                </v-col>
                            </template>
                            <v-col cols="12" md="6">
                                <v-select v-model="form.vehicleId" :items="vehicleOptions" item-title="label"
                                    item-value="vehicleId" label="Automjeti" variant="outlined" density="compact"
                                    :rules="[v => !!v || 'Required']" prepend-inner-icon="mdi-car"></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="form.notes" label="Shënime" variant="outlined" density="compact"
                                    prepend-inner-icon="mdi-note-text-outline"></v-text-field>
                            </v-col>
                        </v-row>
                        <v-alert v-if="manualCandidate && !editingId" type="info" density="compact" variant="tonal" class="mt-2 mb-0">
                            Kandidati do të regjistrohet automatikisht në sistem.
                        </v-alert>
                        <v-alert v-if="formError" type="error" density="compact" class="mt-2 mb-0" closable
                            @click:close="formError = ''">{{ formError }}</v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="formDialog = false">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="saving" @click="saveEvent">
                        {{ editingId ? 'Përditëso' : 'Ruaj' }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Delete confirm -->
        <v-dialog v-model="deleteDialog" max-width="400">
            <v-card rounded="lg">
                <v-card-title>Fshi ngjarjen</v-card-title>
                <v-card-text>Jeni të sigurt që dëshironi të fshini këtë orar?</v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="deleteDialog = false">Anulo</v-btn>
                    <v-btn color="error" variant="elevated" :loading="deleting" @click="doDelete">Fshi</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useScheduleStore } from '@/store/ScheduleStore';
import { useCandidateStore } from '@/store/CandidateStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, nextTick, watch } from 'vue';

const store = useScheduleStore();
const candidateStore = useCandidateStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(store);

// ─── Instructor color palette ───
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
];
const DS_COLOR = { bg: 'rgba(255,152,0,0.15)', border: '#FF9800', text: '#E65100' };

const instructorColorMap = ref({});

function assignInstructorColors() {
    const ids = [...new Set(events.value.filter(e => e.eventType !== 'driving-session' && e.instructorUserId).map(e => e.instructorUserId))];
    const map = {};
    ids.forEach((id, i) => { map[id] = INSTRUCTOR_COLORS[i % INSTRUCTOR_COLORS.length]; });
    instructorColorMap.value = map;
}

function getEventColor(ev) {
    if (ev.eventType === 'driving-session') {
        return { background: DS_COLOR.bg, borderLeft: `3px solid ${DS_COLOR.border}`, color: DS_COLOR.text };
    }
    const c = instructorColorMap.value[ev.instructorUserId] || INSTRUCTOR_COLORS[0];
    return { background: c.bg, borderLeft: `3px solid ${c.border}`, color: c.text };
}

const instructorLegend = computed(() => {
    const map = instructorColorMap.value;
    return Object.keys(map).map(id => {
        const numId = Number(id);
        const ev = events.value.find(e => e.instructorUserId === numId && e.eventType !== 'driving-session');
        return { id: numId, name: ev?.instructorName || `Instruktor #${id}`, ...map[id] };
    });
});

// ─── Role ───
const isAdmin = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || '';
        return role === 'Admin' || role === 'SuperAdmin';
    } catch { return false; }
});
const currentUserId = computed(() => {
    try {
        const uid = localStorage.getItem('userId');
        if (uid) return parseInt(uid);
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        return profile?.obj?.userId || profile?.obj?.UserId || 0;
    } catch { return 0; }
});

// ─── View state ───
const viewMode = ref('day');
const currentDate = ref(new Date());
const dayBodyRef = ref(null);
const weekBodyRef = ref(null);

const hours = Array.from({ length: 15 }, (_, i) => i + 6);

// ─── Navigation ───
function goToday() { currentDate.value = new Date(); loadEvents(); }
function goPrev() {
    const d = new Date(currentDate.value);
    if (viewMode.value === 'day') d.setDate(d.getDate() - 1);
    else d.setDate(d.getDate() - 7);
    currentDate.value = d;
    loadEvents();
}
function goNext() {
    const d = new Date(currentDate.value);
    if (viewMode.value === 'day') d.setDate(d.getDate() + 1);
    else d.setDate(d.getDate() + 7);
    currentDate.value = d;
    loadEvents();
}

watch(viewMode, () => loadEvents());

const headerTitle = computed(() => {
    const d = currentDate.value;
    const months = ['Janar', 'Shkurt', 'Mars', 'Prill', 'Maj', 'Qershor', 'Korrik', 'Gusht', 'Shtator', 'Tetor', 'Nëntor', 'Dhjetor'];
    if (viewMode.value === 'day') {
        return `${d.getDate()} ${months[d.getMonth()]} ${d.getFullYear()}`;
    }
    const ws = weekStart(d);
    const we = new Date(ws); we.setDate(we.getDate() + 6);
    if (ws.getMonth() === we.getMonth()) return `${ws.getDate()} – ${we.getDate()} ${months[ws.getMonth()]} ${ws.getFullYear()}`;
    return `${ws.getDate()} ${months[ws.getMonth()].slice(0, 3)} – ${we.getDate()} ${months[we.getMonth()].slice(0, 3)} ${ws.getFullYear()}`;
});

function weekStart(d) {
    const r = new Date(d); const day = r.getDay(); const diff = day === 0 ? -6 : 1 - day;
    r.setDate(r.getDate() + diff); r.setHours(0, 0, 0, 0); return r;
}
const weekDays = computed(() => {
    const ws = weekStart(currentDate.value);
    return Array.from({ length: 7 }, (_, i) => { const d = new Date(ws); d.setDate(d.getDate() + i); return d; });
});
function dayLabel(d) { return ['Diel', 'Hën', 'Mar', 'Mër', 'Enj', 'Pre', 'Sht'][d.getDay()]; }
function isToday(d) { const t = new Date(); return d.getDate() === t.getDate() && d.getMonth() === t.getMonth() && d.getFullYear() === t.getFullYear(); }
function formatHour(h) { return `${String(h).padStart(2, '0')}:00`; }
function fmtDate(d) { return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`; }

// ─── Events data ───
const events = ref([]);
const filterInstructor = ref(null);
const filterVehicle = ref(null);

function dateRange() {
    if (viewMode.value === 'day') {
        const s = fmtDate(currentDate.value);
        return { from: s, to: s };
    }
    const ws = weekStart(currentDate.value);
    const we = new Date(ws); we.setDate(we.getDate() + 6);
    return { from: fmtDate(ws), to: fmtDate(we) };
}

function loadEvents() {
    const { from, to } = dateRange();
    // Instructors always filter by own ID; admins use the dropdown filter
    const instructorFilter = isAdmin.value ? (filterInstructor.value || 0) : currentUserId.value;
    store.getEvents(from, to, instructorFilter, filterVehicle.value || 0)
        .then((res) => {
            const raw = res?.data?.data ?? res?.data?.Data ?? [];
            events.value = raw.map(normalizeEvent).filter(Boolean);
            assignInstructorColors();
        })
        .catch(() => {
            events.value = [];
            settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë ngarkimit të orarit' });
        });
}

function normalizeEvent(r) {
    if (!r || typeof r !== 'object') return null;
    return {
        id: r.id ?? r.Id ?? r.scheduleEventId ?? r.ScheduleEventId,
        eventDate: r.eventDate ?? r.EventDate ?? '',
        startTime: r.startTime ?? r.StartTime ?? '',
        endTime: r.endTime ?? r.EndTime ?? '',
        instructorUserId: r.instructorUserId ?? r.InstructorUserId ?? 0,
        instructorName: r.instructorName ?? r.InstructorName ?? '',
        candidateId: r.candidateId ?? r.CandidateId ?? 0,
        candidateName: r.candidateName ?? r.CandidateName ?? '',
        vehicleId: r.vehicleId ?? r.VehicleId ?? 0,
        vehiclePlate: r.vehiclePlate ?? r.VehiclePlate ?? '',
        vehicleBrand: r.vehicleBrand ?? r.VehicleBrand ?? '',
        notes: r.notes ?? r.Notes ?? '',
        eventType: r.eventType ?? r.EventType ?? 'schedule',
        addedBy: r.addedBy ?? r.AddedBy ?? 0,
    };
}

function getEventsForHour(date, hour) {
    const ds = fmtDate(date);
    return events.value.filter(e => {
        if (e.eventDate !== ds) return false;
        const parts = e.startTime.split(':');
        return parseInt(parts[0]) === hour;
    });
}

function isDsBlockedForInstructor(ev) {
    return !isAdmin.value && ev?.eventType === 'driving-session';
}

function eventTitle(ev) {
    if (isDsBlockedForInstructor(ev)) {
        return ev.vehiclePlate + (ev.vehicleBrand ? ' – ' + ev.vehicleBrand : '');
    }
    return ev.candidateName;
}

// ─── Dropdowns ───
const instructorOptions = ref([]);
const candidateOptions = ref([]);
const vehicleOptions = ref([]);

function loadDropdowns() {
    store.getInstructorsDropdown().then(r => {
        instructorOptions.value = (r?.data?.data ?? []).map(u => ({ userId: u.userId ?? u.UserId, fullName: u.fullName ?? u.FullName ?? '' }));
    }).catch(() => {});
    store.getCandidatesDropdown().then(r => {
        candidateOptions.value = (r?.data?.data ?? []).map(c => ({ candidateId: c.candidateId ?? c.CandidateId, fullName: c.fullName ?? c.FullName ?? '' }));
    }).catch(() => {});
    store.getVehiclesDropdown().then(r => {
        vehicleOptions.value = (r?.data?.data ?? []).map(v => ({ vehicleId: v.vehicleId ?? v.VehicleId, label: `${v.plateNumber ?? v.PlateNumber}${v.brand ? ' – ' + v.brand : ''}` }));
    }).catch(() => {});
}

// ─── Detail dialog ───
const detailDialog = ref(false);
const detailEvent = ref(null);

function openEventDetail(ev) { detailEvent.value = ev; detailDialog.value = true; }
function canEditEvent(ev) { if (!ev || ev.eventType === 'driving-session') return false; if (isAdmin.value) return true; return ev.addedBy === currentUserId.value; }
function canDeleteEvent(ev) { return canEditEvent(ev); }
function openEditFromDetail() { detailDialog.value = false; openEditDialog(detailEvent.value); }
function confirmDeleteFromDetail() { detailDialog.value = false; deleteTarget.value = detailEvent.value; deleteDialog.value = true; }

// ─── Create / Edit dialog ───
const formDialog = ref(false);
const saving = ref(false);
const formRef = ref(null);
const formError = ref('');
const editingId = ref(null);
const fDateMenu = ref(false);
const fDateModel = ref(null);

const manualCandidate = ref(false);
const form = ref({ eventDate: '', startTime: null, endTime: null, instructorUserId: null, candidateId: null, manualFirstName: '', manualLastName: '', vehicleId: null, notes: '' });

function handleFormDate(v) {
    if (v instanceof Date) form.value.eventDate = fmtDate(v);
    else { const p = String(v).split('T')[0].split('-'); if (p.length === 3) form.value.eventDate = `${p[2]}.${p[1]}.${p[0]}`; }
    nextTick(() => { fDateMenu.value = false; });
}

const timeSlots = (() => {
    const s = [];
    for (let h = 6; h <= 21; h++) for (let m = 0; m < 60; m += 15) { if (h === 21 && m > 0) break; s.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`); }
    return s;
})();

function openCreateDialog(prefill) {
    editingId.value = null;
    formError.value = '';
    manualCandidate.value = false;
    form.value = {
        eventDate: prefill?.date ? fmtDate(prefill.date) : fmtDate(currentDate.value),
        startTime: prefill?.hour ? `${String(prefill.hour).padStart(2, '0')}:00` : null,
        endTime: prefill?.hour ? `${String(prefill.hour).padStart(2, '0')}:45` : null,
        instructorUserId: isAdmin.value ? null : currentUserId.value,
        candidateId: null,
        manualFirstName: '',
        manualLastName: '',
        vehicleId: null,
        notes: '',
    };
    fDateModel.value = null;
    formDialog.value = true;
}

function openEditDialog(ev) {
    if (!ev) return;
    editingId.value = ev.id;
    formError.value = '';
    manualCandidate.value = false;
    form.value = {
        eventDate: ev.eventDate,
        startTime: ev.startTime,
        endTime: ev.endTime,
        instructorUserId: ev.instructorUserId,
        candidateId: ev.candidateId,
        manualFirstName: '',
        manualLastName: '',
        vehicleId: ev.vehicleId,
        notes: ev.notes || '',
    };
    fDateModel.value = null;
    formDialog.value = true;
}

function handleCellClick(date, hour) {
    openCreateDialog({ date, hour });
}

async function saveEvent() {
    formError.value = '';
    const valid = await formRef.value?.validate();
    if (!valid?.valid) return;

    saving.value = true;
    try {
        let candidateId = form.value.candidateId;

        // If manual entry, create a quick candidate record first, then find its ID
        if (manualCandidate.value && !editingId.value) {
            const firstName = form.value.manualFirstName.trim();
            const lastName = form.value.manualLastName.trim();
            try {
                const quickRes = await candidateStore.createCandidate({
                    firstName,
                    lastName,
                    totalServiceAmount: 0,
                    installments: [],
                });
                const body = quickRes?.data;
                if (body?.status === 'error') {
                    formError.value = body.responseMsg || 'Nuk u arrit të regjistrohet kandidati';
                    saving.value = false;
                    return;
                }
                // Reload dropdown to get the newly created candidate's ID
                const dropdownRes = await store.getCandidatesDropdown();
                const freshList = (dropdownRes?.data?.data ?? []).map(c => ({
                    candidateId: c.candidateId ?? c.CandidateId,
                    fullName: c.fullName ?? c.FullName ?? '',
                }));
                candidateOptions.value = freshList;
                const targetName = `${firstName} ${lastName}`;
                const match = freshList.find(c => c.fullName.startsWith(targetName));
                if (!match) {
                    formError.value = 'Kandidati u regjistrua por nuk u gjet në listë. Provoni përsëri.';
                    saving.value = false;
                    return;
                }
                candidateId = match.candidateId;
            } catch (err) {
                formError.value = err?.response?.data?.responseMsg || 'Gabim gjatë regjistrimit të kandidatit';
                saving.value = false;
                return;
            }
        }

        const payload = {
            eventDate: form.value.eventDate,
            startTime: form.value.startTime,
            endTime: form.value.endTime,
            instructorUserId: isAdmin.value ? form.value.instructorUserId : 0,
            candidateId: candidateId,
            vehicleId: form.value.vehicleId,
            notes: form.value.notes || null,
        };

        let res;
        if (editingId.value) {
            res = await store.updateEvent(editingId.value, payload);
        } else {
            res = await store.createEvent(payload);
        }

        const body = res?.data;
        if (body?.status === 'error') {
            formError.value = body.responseMsg || 'Gabim';
            return;
        }

        // Sync practical lesson when creating a new event
        if (!editingId.value && candidateId > 0) {
            const vehicleLabel = vehicleOptions.value.find(v => v.vehicleId === form.value.vehicleId)?.label || '';
            try {
                await candidateStore.addPracticalLesson({
                    candidateId: candidateId,
                    lessonDate: form.value.eventDate,
                    time: form.value.startTime,
                    vehicle: vehicleLabel,
                });
            } catch (_) { /* non-blocking */ }
        }

        settingStore.toggleSnackbar({ status: true, msg: editingId.value ? 'Orari u përditësua!' : 'Orari u regjistrua me sukses!' });
        formDialog.value = false;
        loadEvents();
    } catch (err) {
        formError.value = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Gabim gjatë ruajtjes';
    } finally {
        saving.value = false;
    }
}

// ─── Delete ───
const deleteDialog = ref(false);
const deleting = ref(false);
const deleteTarget = ref(null);

function confirmDelete(ev) { deleteTarget.value = ev; deleteDialog.value = true; }

async function doDelete() {
    if (!deleteTarget.value) return;
    deleting.value = true;
    try {
        await store.deleteEvent(deleteTarget.value.id);
        settingStore.toggleSnackbar({ status: true, msg: 'Orari u fshi' });
        deleteDialog.value = false;
        loadEvents();
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë fshirjes' });
    } finally {
        deleting.value = false;
    }
}

// ─── Scroll to current hour on mount ───
function scrollToNow() {
    nextTick(() => {
        const now = new Date().getHours();
        const idx = Math.max(0, now - 6);
        const el = dayBodyRef.value || weekBodyRef.value;
        if (el) el.scrollTop = idx * 60;
    });
}

// ─── Init ───
onMounted(() => {
    loadEvents();
    loadDropdowns();
    scrollToNow();
});
</script>

<style scoped>
.schedules-toolbar {
    padding: 12px 20px !important;
    min-height: 56px !important;
    gap: 8px;
    background: rgba(255, 255, 255, 0.95);
}

.schedules-header-card {
    overflow: visible !important;
}

.schedules-toolbar :deep(.v-toolbar__content) {
    height: auto !important;
    min-height: 0 !important;
    overflow: visible !important;
    flex-wrap: wrap !important;
    row-gap: 8px;
}

.schedules-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
}

.schedules-nav-group {
    display: flex;
    align-items: center;
    gap: 8px;
    min-width: 0;
    flex: 1 1 auto;
}

.schedules-controls-wrap {
    margin-left: 0;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 10px;
    flex: 1 1 420px;
    justify-content: flex-end;
}

.sch-header-title { white-space: normal; line-height: 1.2; }
.sch-add-btn { min-height: 40px; }

.schedules-toolbar :deep(.v-btn),
.schedules-toolbar :deep(.v-field),
.schedules-toolbar :deep(.v-btn-toggle) {
    border-radius: 4px !important;
}

.sch-filter { min-width: 160px; max-width: 180px; }

@media (max-width: 1100px) {
    .schedules-actions-wrap { display: grid; grid-template-columns: 1fr; gap: 10px; }
    .schedules-controls-wrap { width: 100%; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; justify-content: initial; }
    .sch-filter { width: 100%; min-width: 0; max-width: 100%; }
    .sch-view-toggle, .sch-add-btn { width: 100%; }
}

/* ─── Calendar grid (shared) ─── */
.calendar-card { overflow: hidden; }

.time-gutter-header { width: 60px; min-width: 60px; flex-shrink: 0; }

.time-gutter {
    width: 60px; min-width: 60px; flex-shrink: 0;
    text-align: right; padding-right: 8px;
    font-size: 11px; color: #999; padding-top: 4px;
}

.time-row {
    display: flex;
    min-height: 48px;
    border-bottom: 1px solid #eee;
}

.time-cell {
    flex: 1;
    border-left: 1px solid #eee;
    min-height: 48px;
    cursor: pointer;
    padding: 2px;
}
.time-cell:hover { background: rgba(25, 118, 210, 0.03); }

/* Events flow inside cell — no overlap */
.time-cell-events {
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
}

/* ─── Day view ─── */
.day-grid { display: flex; flex-direction: column; }
.day-header { display: flex; border-bottom: 2px solid #e0e0e0; padding: 8px 0; position: sticky; top: 0; z-index: 2; background: #fff; }
.day-col-header { flex: 1; padding: 4px; }
.day-body { max-height: 70vh; overflow-y: auto; }

/* In day view, events take full width and stack vertically */
.day-body .time-cell-events { flex-direction: column; }
.day-body .cal-event { width: 100%; }

/* ─── Week view ─── */
.week-grid { display: flex; flex-direction: column; }
.week-header { display: flex; border-bottom: 2px solid #e0e0e0; padding: 8px 0; position: sticky; top: 0; z-index: 2; background: #fff; }
.week-col-header { flex: 1; padding: 4px; min-width: 0; }
.week-body { max-height: 70vh; overflow-y: auto; }
.week-cell { flex: 1; min-width: 0; }

/* In week view, events stack vertically within narrow cells */
.week-body .time-cell-events { flex-direction: column; }
.week-body .cal-event { width: 100%; }

/* ─── Event blocks — flow layout (no absolute positioning) ─── */
.cal-event {
    border-radius: 4px;
    padding: 3px 6px;
    font-size: 11px;
    line-height: 1.3;
    overflow: hidden;
    cursor: pointer;
    transition: box-shadow 0.15s, transform 0.1s;
    min-height: 22px;
}

.cal-event:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
    transform: translateY(-1px);
    z-index: 2;
}

.cal-event-time { font-weight: 600; font-size: 10px; }
.cal-event-title { font-weight: 500; }
.cal-event-sub { font-size: 10px; opacity: 0.8; }

/* ─── Mobile ─── */
@media (max-width: 600px) {
    .schedules-toolbar { padding: 8px !important; flex-wrap: wrap; }
    .schedules-actions-wrap { display: grid; grid-template-columns: 1fr; gap: 10px; }
    .schedules-nav-group { display: grid; grid-template-columns: repeat(4, minmax(0, auto)); align-items: center; gap: 8px; }
    .sch-header-title { grid-column: 1 / -1; white-space: normal; margin-left: 0 !important; }
    .schedules-controls-wrap { margin-left: 0; width: 100%; display: grid; grid-template-columns: 1fr; gap: 10px; }
    .sch-filter { min-width: 100% !important; max-width: 100% !important; }
    .sch-view-toggle, .sch-add-btn { width: 100%; }
    .sch-view-toggle :deep(.v-btn) { flex: 1 1 auto; }
    .calendar-card { overflow-x: auto !important; }
    .day-body, .week-body { max-height: 60vh; }
    .time-gutter, .time-gutter-header { width: 42px; min-width: 42px; font-size: 10px; }
    .cal-event { font-size: 9px; padding: 2px 3px; }
}
</style>
