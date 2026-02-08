<template>
    <div class="schedules-container">
        <!-- ─── Toolbar ─── -->
        <v-card elevation="2" rounded="lg" class="mb-4">
            <v-toolbar density="comfortable" flat class="schedules-toolbar">
                <template v-slot:prepend>
                    <div class="d-flex align-center ga-2">
                        <v-btn variant="outlined" size="small" @click="goToday">Today</v-btn>
                        <v-btn icon variant="text" size="small" @click="goPrev">
                            <v-icon icon="mdi-chevron-left"></v-icon>
                        </v-btn>
                        <v-btn icon variant="text" size="small" @click="goNext">
                            <v-icon icon="mdi-chevron-right"></v-icon>
                        </v-btn>
                        <span class="text-h6 font-weight-medium ml-2">{{ headerTitle }}</span>
                    </div>
                </template>
                <template v-slot:append>
                    <div class="d-flex flex-wrap align-center ga-3">
                        <!-- Filters -->
                        <v-select v-if="isAdmin" v-model="filterInstructor" :items="instructorOptions"
                            item-title="fullName" item-value="userId" label="Instructor" variant="outlined"
                            density="compact" hide-details clearable class="sch-filter"
                            @update:model-value="loadEvents"></v-select>
                        <v-select v-model="filterVehicle" :items="vehicleOptions" item-title="label"
                            item-value="vehicleId" label="Vehicle" variant="outlined" density="compact" hide-details
                            clearable class="sch-filter" @update:model-value="loadEvents"></v-select>

                        <!-- View toggle -->
                        <v-btn-toggle v-model="viewMode" mandatory density="compact" variant="outlined" color="primary">
                            <v-btn value="day" size="small">Day</v-btn>
                            <v-btn value="week" size="small">Week</v-btn>
                        </v-btn-toggle>

                        <v-btn color="primary" variant="elevated" class="text-capitalize" prepend-icon="mdi-plus"
                            @click="openCreateDialog(null)">
                            Add Event
                        </v-btn>
                    </div>
                </template>
            </v-toolbar>
        </v-card>

        <!-- ─── Legend ─── -->
        <div class="d-flex ga-4 mb-3 flex-wrap">
            <v-chip size="small" color="primary" variant="tonal">
                <v-icon start icon="mdi-circle" size="10"></v-icon>Schedule
            </v-chip>
            <v-chip size="small" color="orange" variant="tonal">
                <v-icon start icon="mdi-circle" size="10"></v-icon>{{ isAdmin ? 'Driving Session' : 'Booked / Exam Slot' }}
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
                            <!-- Events for this hour -->
                            <template v-for="ev in getEventsForHour(currentDate, hour)" :key="ev.id + ev.eventType">
                                <div class="cal-event" :class="eventClass(ev)" :style="eventStyle(ev, hour)"
                                    @click.stop="openEventDetail(ev)">
                                    <div class="cal-event-time">{{ ev.startTime }}–{{ ev.endTime }}</div>
                                    <div class="cal-event-title text-truncate">{{ eventTitle(ev) }}</div>
                                    <div class="cal-event-sub text-truncate">{{ ev.vehiclePlate }}{{ ev.vehicleBrand ? ' – ' + ev.vehicleBrand : '' }}</div>
                                </div>
                            </template>
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
                            <template v-for="ev in getEventsForHour(d, hour)" :key="ev.id + ev.eventType">
                                <div class="cal-event" :class="eventClass(ev)" :style="eventStyle(ev, hour)"
                                    @click.stop="openEventDetail(ev)">
                                    <div class="cal-event-time">{{ ev.startTime }}</div>
                                    <div class="cal-event-title text-truncate">{{ eventTitle(ev) }}</div>
                                </div>
                            </template>
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
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis">Date</div><div class="font-weight-medium">{{ detailEvent.eventDate }}</div></v-col>
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis">Time</div><div class="font-weight-medium">{{ detailEvent.startTime }} – {{ detailEvent.endTime }}</div></v-col>
                        <!-- For driving sessions when Instructor: show only vehicle -->
                        <v-col cols="6" v-if="isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Status</div>
                            <div class="font-weight-medium">Booked / Exam Slot</div>
                        </v-col>
                        <v-col cols="6" v-if="!isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Candidate</div>
                            <div class="font-weight-medium">{{ detailEvent.candidateName }}</div>
                        </v-col>
                        <v-col cols="6"><div class="text-body-2 text-medium-emphasis mt-2">Vehicle</div><div class="font-weight-medium">{{ detailEvent.vehiclePlate }}{{ detailEvent.vehicleBrand ? ' – ' + detailEvent.vehicleBrand : '' }}</div></v-col>
                        <v-col cols="6" v-if="!isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Instructor</div>
                            <div class="font-weight-medium">{{ detailEvent.instructorName || '—' }}</div>
                        </v-col>
                        <v-col cols="6" v-if="detailEvent.notes && !isDsBlockedForInstructor(detailEvent)">
                            <div class="text-body-2 text-medium-emphasis mt-2">Notes</div>
                            <div class="font-weight-medium">{{ detailEvent.notes }}</div>
                        </v-col>
                    </v-row>
                </v-card-text>
                <v-card-actions class="pa-4"><v-spacer></v-spacer><v-btn variant="text" @click="detailDialog = false">Close</v-btn></v-card-actions>
            </v-card>
        </v-dialog>

        <!-- ─── Create / Edit Dialog ─── -->
        <v-dialog v-model="formDialog" max-width="700" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-calendar-plus" color="primary"></v-icon>
                    <span>{{ editingId ? 'Edit Event' : 'New Schedule Event' }}</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <v-form ref="formRef" @submit.prevent="saveEvent">
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-menu v-model="fDateMenu" :close-on-content-click="false" location="bottom" min-width="auto">
                                    <template v-slot:activator="{ props: mp }">
                                        <v-text-field :model-value="form.eventDate" label="Date *" variant="outlined"
                                            density="compact" prepend-inner-icon="mdi-calendar" readonly
                                            :rules="[v => !!v || 'Required']" v-bind="mp"></v-text-field>
                                    </template>
                                    <v-date-picker v-model="fDateModel" color="primary" @update:model-value="handleFormDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                            <v-col cols="6" md="3">
                                <v-select v-model="form.startTime" :items="timeSlots" label="Start *" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-clock-start"></v-select>
                            </v-col>
                            <v-col cols="6" md="3">
                                <v-select v-model="form.endTime" :items="timeSlots" label="End *" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-clock-end"></v-select>
                            </v-col>
                            <v-col cols="12" md="6" v-if="isAdmin">
                                <v-select v-model="form.instructorUserId" :items="instructorOptions"
                                    item-title="fullName" item-value="userId" label="Instructor *" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-account-tie"></v-select>
                            </v-col>
                            <v-col cols="12" :md="isAdmin ? 6 : 12">
                                <v-autocomplete v-model="form.candidateId" :items="candidateOptions"
                                    item-title="fullName" item-value="candidateId" label="Candidate *" variant="outlined"
                                    density="compact" :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-account"></v-autocomplete>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-select v-model="form.vehicleId" :items="vehicleOptions" item-title="label"
                                    item-value="vehicleId" label="Vehicle *" variant="outlined" density="compact"
                                    :rules="[v => !!v || 'Required']" prepend-inner-icon="mdi-car"></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="form.notes" label="Notes" variant="outlined" density="compact"
                                    prepend-inner-icon="mdi-note-text-outline"></v-text-field>
                            </v-col>
                        </v-row>
                        <v-alert v-if="formError" type="error" density="compact" class="mt-2 mb-0" closable
                            @click:close="formError = ''">{{ formError }}</v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="formDialog = false">Cancel</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="saving" @click="saveEvent">
                        {{ editingId ? 'Update' : 'Save' }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Delete confirm -->
        <v-dialog v-model="deleteDialog" max-width="400">
            <v-card rounded="lg">
                <v-card-title>Delete Event</v-card-title>
                <v-card-text>Are you sure you want to delete this schedule event?</v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
                    <v-btn color="error" variant="elevated" :loading="deleting" @click="doDelete">Delete</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useScheduleStore } from '@/store/ScheduleStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, nextTick, watch } from 'vue';

const store = useScheduleStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(store);

// ─── Role ───
const isAdmin = computed(() => {
    try { return localStorage.getItem('userRoleId') === '1'; } catch { return false; }
});
const currentUserId = computed(() => {
    try { return parseInt(localStorage.getItem('userId') || '0'); } catch { return 0; }
});

// ─── View state ───
const viewMode = ref('day');
const currentDate = ref(new Date());
const dayBodyRef = ref(null);
const weekBodyRef = ref(null);

const hours = Array.from({ length: 15 }, (_, i) => i + 6); // 06:00 – 20:00

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
    const months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
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
function dayLabel(d) { return ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'][d.getDay()]; }
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
    store.getEvents(from, to, filterInstructor.value || 0, filterVehicle.value || 0)
        .then((res) => {
            const raw = res?.data?.data ?? res?.data?.Data ?? [];
            events.value = raw.map(normalizeEvent).filter(Boolean);
        })
        .catch(() => {
            events.value = [];
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading schedule' });
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

// Find events whose start hour matches a given hour row for a given date
function getEventsForHour(date, hour) {
    const ds = fmtDate(date);
    return events.value.filter(e => {
        if (e.eventDate !== ds) return false;
        const parts = e.startTime.split(':');
        return parseInt(parts[0]) === hour;
    });
}

function eventClass(ev) {
    return ev.eventType === 'driving-session' ? 'cal-event--ds' : 'cal-event--sch';
}

// For Instructor: driving sessions show vehicle-only info ("Booked / Exam Slot")
// The backend already strips candidate data for Instructor, so candidateName = "Booked / Exam Slot"
function isDsBlockedForInstructor(ev) {
    return !isAdmin.value && ev?.eventType === 'driving-session';
}

function eventTitle(ev) {
    if (isDsBlockedForInstructor(ev)) {
        // Show vehicle plate as the title for blocked DS slots
        return ev.vehiclePlate + (ev.vehicleBrand ? ' – ' + ev.vehicleBrand : '');
    }
    return ev.candidateName;
}

function eventStyle(ev, hourRow) {
    // Calculate vertical position within the hour cell and height
    const [sh, sm] = ev.startTime.split(':').map(Number);
    const [eh, em] = ev.endTime.split(':').map(Number);
    const topMin = sm;
    const duration = (eh * 60 + em) - (sh * 60 + sm);
    const topPx = (topMin / 60) * 60; // 60px per hour cell
    const heightPx = Math.max((duration / 60) * 60, 20);
    return { top: `${topPx}px`, height: `${heightPx}px` };
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

const form = ref({ eventDate: '', startTime: null, endTime: null, instructorUserId: null, candidateId: null, vehicleId: null, notes: '' });

function handleFormDate(v) {
    if (v instanceof Date) form.value.eventDate = fmtDate(v);
    else { const p = String(v).split('T')[0].split('-'); if (p.length === 3) form.value.eventDate = `${p[2]}.${p[1]}.${p[0]}`; }
    nextTick(() => { fDateMenu.value = false; });
}

// Time slots 06:00 – 21:00 every 15 min
const timeSlots = (() => {
    const s = [];
    for (let h = 6; h <= 21; h++) for (let m = 0; m < 60; m += 15) { if (h === 21 && m > 0) break; s.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`); }
    return s;
})();

function openCreateDialog(prefill) {
    editingId.value = null;
    formError.value = '';
    form.value = {
        eventDate: prefill?.date ? fmtDate(prefill.date) : fmtDate(currentDate.value),
        startTime: prefill?.hour ? `${String(prefill.hour).padStart(2, '0')}:00` : null,
        endTime: prefill?.hour ? `${String(prefill.hour).padStart(2, '0')}:45` : null,
        instructorUserId: isAdmin.value ? null : currentUserId.value,
        candidateId: null,
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
    form.value = {
        eventDate: ev.eventDate,
        startTime: ev.startTime,
        endTime: ev.endTime,
        instructorUserId: ev.instructorUserId,
        candidateId: ev.candidateId,
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
        const payload = {
            eventDate: form.value.eventDate,
            startTime: form.value.startTime,
            endTime: form.value.endTime,
            instructorUserId: isAdmin.value ? form.value.instructorUserId : 0,
            candidateId: form.value.candidateId,
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
            formError.value = body.responseMsg || 'Error';
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: editingId.value ? 'Event updated!' : 'Event created!' });
        formDialog.value = false;
        loadEvents();
    } catch (err) {
        formError.value = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Failed to save event';
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
        settingStore.toggleSnackbar({ status: true, msg: 'Event deleted' });
        deleteDialog.value = false;
        loadEvents();
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Error deleting event' });
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
.schedules-container {
    width: 100%;
    padding: 16px 20px;
    background: linear-gradient(180deg, #f8f9fa 0%, #fff 100%);
    min-height: 100%;
}

.schedules-toolbar {
    padding: 12px 20px !important;
    min-height: 56px !important;
    gap: 8px;
    background: rgba(255, 255, 255, 0.95);
}

.sch-filter {
    min-width: 160px;
    max-width: 180px;
}

/* ─── Calendar grid (shared) ─── */
.calendar-card {
    overflow: hidden;
}

.time-gutter-header {
    width: 60px;
    min-width: 60px;
    flex-shrink: 0;
}

.time-gutter {
    width: 60px;
    min-width: 60px;
    flex-shrink: 0;
    text-align: right;
    padding-right: 8px;
    font-size: 11px;
    color: #999;
    line-height: 60px;
}

.time-row {
    display: flex;
    min-height: 60px;
    border-bottom: 1px solid #eee;
}

.time-cell {
    flex: 1;
    position: relative;
    border-left: 1px solid #eee;
    min-height: 60px;
    cursor: pointer;
}
.time-cell:hover {
    background: rgba(25, 118, 210, 0.03);
}

/* ─── Day view ─── */
.day-grid {
    display: flex;
    flex-direction: column;
}

.day-header {
    display: flex;
    border-bottom: 2px solid #e0e0e0;
    padding: 8px 0;
    position: sticky;
    top: 0;
    z-index: 2;
    background: #fff;
}

.day-col-header {
    flex: 1;
    padding: 4px;
}

.day-body {
    max-height: 70vh;
    overflow-y: auto;
}

/* ─── Week view ─── */
.week-grid {
    display: flex;
    flex-direction: column;
}

.week-header {
    display: flex;
    border-bottom: 2px solid #e0e0e0;
    padding: 8px 0;
    position: sticky;
    top: 0;
    z-index: 2;
    background: #fff;
}

.week-col-header {
    flex: 1;
    padding: 4px;
    min-width: 0;
}

.week-body {
    max-height: 70vh;
    overflow-y: auto;
}

.week-cell {
    flex: 1;
    min-width: 0;
}

/* ─── Event blocks ─── */
.cal-event {
    position: absolute;
    left: 2px;
    right: 2px;
    border-radius: 4px;
    padding: 2px 6px;
    font-size: 11px;
    line-height: 1.3;
    overflow: hidden;
    cursor: pointer;
    z-index: 1;
    transition: box-shadow 0.15s;
}

.cal-event:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
    z-index: 2;
}

.cal-event--sch {
    background: rgba(25, 118, 210, 0.15);
    border-left: 3px solid #1976D2;
    color: #1565C0;
}

.cal-event--ds {
    background: rgba(255, 152, 0, 0.15);
    border-left: 3px solid #FF9800;
    color: #E65100;
}

.cal-event-time {
    font-weight: 600;
    font-size: 10px;
}

.cal-event-title {
    font-weight: 500;
}

.cal-event-sub {
    font-size: 10px;
    opacity: 0.8;
}
</style>
