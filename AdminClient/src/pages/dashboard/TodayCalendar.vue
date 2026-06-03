<template>
    <v-card class="today-cal-card" rounded="lg">
        <div class="today-cal-head">
            <div class="d-flex align-center ga-3">
                <div class="today-cal-icon" :style="{ background: iconBg }">
                    <v-icon :icon="icon" size="22" :color="iconColor"></v-icon>
                </div>
                <div>
                    <div class="today-cal-title">{{ title }}</div>
                    <div class="today-cal-date">{{ todayLabel }}</div>
                </div>
            </div>
            <v-chip size="small" variant="tonal" :color="iconColor" class="today-cal-count">
                {{ activeCount }} {{ activeCount === 1 ? 'aktivitet' : 'aktivitete' }}
            </v-chip>
        </div>

        <v-divider></v-divider>

        <!-- Instructor legend (color-coded) -->
        <div v-if="legend && legend.length" class="today-cal-legend">
            <v-chip v-for="item in legend" :key="item.id" size="x-small" variant="flat"
                :style="{ background: item.bg, color: item.text, border: '1px solid ' + item.border }">
                <v-icon start icon="mdi-circle" size="9" :style="{ color: item.border }"></v-icon>
                {{ item.name }}
            </v-chip>
        </div>

        <!-- Empty state -->
        <div v-if="!events || events.length === 0" class="today-cal-empty">
            <v-icon :icon="emptyIcon" size="44" color="grey-lighten-1"></v-icon>
            <div class="today-cal-empty-title">{{ emptyMessage }}</div>
            <div class="today-cal-empty-sub">Nuk ka asnjë aktivitet të planifikuar për sot.</div>
        </div>

        <!-- Day timeline -->
        <div v-else class="today-cal-body" ref="bodyRef">
            <div v-for="hour in hours" :key="hour" class="tc-row">
                <div class="tc-gutter">{{ formatHour(hour) }}</div>
                <div class="tc-cell">
                    <div class="tc-cell-events">
                        <div v-for="ev in eventsForHour(hour)" :key="ev.id + ev.eventType"
                            :class="['tc-event', { cancelled: ev.status === 'Cancelled' }]"
                            :style="colorFn(ev)">
                            <div class="tc-event-time">{{ ev.startTime }}–{{ ev.endTime }}</div>
                            <div class="tc-event-title text-truncate">{{ titleFn(ev) }}</div>
                            <div v-if="subFn(ev)" class="tc-event-sub text-truncate">{{ subFn(ev) }}</div>
                            <v-chip v-if="ev.status === 'Cancelled'" size="x-small" color="error" variant="flat"
                                class="tc-event-status">Anuluar</v-chip>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </v-card>
</template>

<script setup>
import { computed, ref, onMounted, nextTick } from 'vue';

const props = defineProps({
    title: { type: String, default: '' },
    icon: { type: String, default: 'mdi-calendar' },
    iconColor: { type: String, default: 'primary' },
    iconBg: { type: String, default: '#eff6ff' },
    events: { type: Array, default: () => [] },
    colorFn: { type: Function, required: true },
    titleFn: { type: Function, required: true },
    subFn: { type: Function, default: () => '' },
    legend: { type: Array, default: () => [] },
    emptyMessage: { type: String, default: 'Asnjë seancë sot' },
    emptyIcon: { type: String, default: 'mdi-calendar-blank-outline' },
});

const bodyRef = ref(null);

const todayLabel = new Date().toLocaleDateString('sq-AL', {
    weekday: 'long', day: 'numeric', month: 'long',
});

const activeCount = computed(() => props.events.filter(e => e.status !== 'Cancelled').length);

// Show only the hours that contain events, so the dashboard stays compact.
const hours = computed(() => {
    const set = new Set();
    props.events.forEach((e) => {
        const h = parseInt(String(e.startTime).split(':')[0], 10);
        if (!isNaN(h)) set.add(h);
    });
    return [...set].sort((a, b) => a - b);
});

function eventsForHour(hour) {
    return props.events
        .filter(e => parseInt(String(e.startTime).split(':')[0], 10) === hour)
        .sort((a, b) => String(a.startTime).localeCompare(String(b.startTime)));
}

function formatHour(h) { return `${String(h).padStart(2, '0')}:00`; }

onMounted(() => {
    nextTick(() => {
        if (bodyRef.value) bodyRef.value.scrollTop = 0;
    });
});
</script>

<style scoped>
.today-cal-card {
    border: 1px solid var(--slate-200);
    height: 100%;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.today-cal-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 16px 20px;
}

.today-cal-icon {
    width: 44px;
    height: 44px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.today-cal-title {
    font-size: 1.0625rem;
    font-weight: 700;
    color: var(--slate-800);
    line-height: 1.2;
}

.today-cal-date {
    font-size: 0.78rem;
    color: var(--slate-500);
    text-transform: capitalize;
    margin-top: 2px;
}

.today-cal-count {
    font-weight: 600;
    flex-shrink: 0;
}

.today-cal-legend {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    padding: 12px 20px 4px;
}

.today-cal-empty {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 48px 20px;
    gap: 6px;
}

.today-cal-empty-title {
    font-weight: 600;
    color: var(--slate-600);
    margin-top: 6px;
}

.today-cal-empty-sub {
    font-size: 0.8125rem;
    color: var(--slate-500);
}

.today-cal-body {
    flex: 1;
    overflow-y: auto;
    max-height: 420px;
    padding: 8px 0;
}

.tc-row {
    display: flex;
    align-items: stretch;
    min-height: 44px;
    border-bottom: 1px solid #f1f5f9;
}

.tc-row:last-child { border-bottom: none; }

.tc-gutter {
    width: 56px;
    min-width: 56px;
    flex-shrink: 0;
    text-align: right;
    padding: 8px 10px 0 0;
    font-size: 11px;
    font-weight: 600;
    color: var(--slate-500);
}

.tc-cell {
    flex: 1;
    border-left: 1px solid #eef2f7;
    padding: 6px 12px 6px 12px;
    min-width: 0;
}

.tc-cell-events {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.tc-event {
    border-radius: 8px;
    padding: 7px 10px;
    font-size: 12px;
    line-height: 1.35;
    overflow: hidden;
    transition: box-shadow 0.15s, transform 0.1s;
    position: relative;
}

.tc-event:hover {
    box-shadow: 0 3px 10px rgba(0, 0, 0, 0.12);
    transform: translateY(-1px);
}

.tc-event.cancelled {
    opacity: 0.8;
}

.tc-event-time {
    font-weight: 700;
    font-size: 11px;
    opacity: 0.95;
}

.tc-event-title {
    font-weight: 600;
    font-size: 12.5px;
}

.tc-event-sub {
    font-size: 11px;
    opacity: 0.85;
}

.tc-event-status {
    margin-top: 4px;
}

@media (max-width: 600px) {
    .today-cal-head { padding: 14px 16px; }
    .today-cal-title { font-size: 1rem; }
    .tc-gutter { width: 46px; min-width: 46px; font-size: 10px; }
    .tc-cell { padding: 6px 8px; }
    .today-cal-body { max-height: 360px; }
}
</style>
