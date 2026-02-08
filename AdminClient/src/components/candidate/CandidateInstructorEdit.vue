<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center sticky-header">
            <span>Edit candidate – Practical lessons (Instructor)</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4">
            <template v-if="detailsLoading">
                <v-progress-linear indeterminate></v-progress-linear>
            </template>
            <template v-else-if="candidate">
                <!-- Read-only candidate info -->
                <h3 class="text-subtitle-1 font-weight-medium mb-3">Candidate information (read-only)</h3>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.serialNumber" label="Serial Number (Nr. Rendor)" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.firstName" label="First Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.parentName" label="Parent Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.lastName" label="Last Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.dateOfBirth" label="Date of Birth" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.phoneNumber" label="Phone Number" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.address" label="Address" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.categoryName" label="Category" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.vehicleType" label="Vehicle Type" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                </v-row>

                <!-- Practical Lessons table with Nr. Rendor -->
                <v-divider class="my-4"></v-divider>
                <h3 class="text-subtitle-1 font-weight-medium mb-3">
                    Practical lessons
                    <span class="text-caption text-medium-emphasis ml-2">({{ sortedLessons.length }} recorded)</span>
                </h3>
                <v-table density="comfortable" class="elevation-1 rounded">
                    <thead>
                        <tr>
                            <th class="text-left">Nr. Rendor</th>
                            <th class="text-left">Date</th>
                            <th class="text-left">Start</th>
                            <th class="text-left">End</th>
                            <th class="text-left">Vehicle</th>
                            <th class="text-left">Instructor</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(lesson, index) in sortedLessons" :key="lesson.practicalLessonId || index">
                            <td>{{ index + 1 }}</td>
                            <td>{{ lesson.lessonDate }}</td>
                            <td>{{ lesson.time }}</td>
                            <td>{{ lesson.endTime || calcEndTime(lesson.time) }}</td>
                            <td>{{ lesson.vehicle || '–' }}</td>
                            <td>{{ lesson.instructorName || '–' }}</td>
                        </tr>
                        <tr v-if="!sortedLessons.length">
                            <td colspan="6" class="text-center text-medium-emphasis py-4">No practical lessons yet.</td>
                        </tr>
                    </tbody>
                </v-table>

                <!-- Add Practical Lesson form -->
                <div class="mt-4">
                    <h4 class="text-body-2 font-weight-medium mb-2">Add practical lesson (Shto ore praktike)</h4>
                    <v-row dense align="center">
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
                        <v-col cols="12" md="2">
                            <v-select
                                v-model="newLessonTime"
                                :items="timeSlots"
                                label="Start time"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                            ></v-select>
                        </v-col>
                        <v-col cols="12" md="2">
                            <v-text-field
                                :model-value="computedEndTime"
                                label="End time"
                                variant="outlined"
                                density="compact"
                                hide-details
                                readonly
                                disabled
                            ></v-text-field>
                        </v-col>
                        <v-col cols="12" md="3">
                            <v-select
                                v-model="newLessonVehicle"
                                :items="lessonVehicles"
                                label="Vehicle"
                                variant="outlined"
                                density="compact"
                                hide-details
                                clearable
                            ></v-select>
                        </v-col>
                        <v-col cols="12" md="2">
                            <v-btn color="primary" variant="elevated" size="small" :loading="addingLesson" :disabled="!canSubmit" @click="addLesson">
                                Shto ore praktike
                            </v-btn>
                        </v-col>
                    </v-row>
                    <v-alert v-if="addError" type="error" density="compact" class="mt-2" closable @click:close="addError = ''">{{ addError }}</v-alert>
                </div>
            </template>
            <v-alert v-else-if="loadError" type="error" density="compact">{{ loadError }}</v-alert>
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
import { ref, computed, watch, onMounted, nextTick } from 'vue';

const props = defineProps({
    candidateId: { type: Number, required: true }
});

const emit = defineEmits(['close', 'saved']);

const candidateStore = useCandidateStore();
const settingStore = useSettingStore();

const candidate = ref(null);
const practicalLessons = ref([]);
const detailsLoading = ref(false);
const loadError = ref('');
const addError = ref('');
const lessonDateMenu = ref(false);
const lessonDateModel = ref(null);
const lessonDateDisplay = ref('');
const newLessonTime = ref(null);
const newLessonVehicle = ref(null);
const addingLesson = ref(false);
const lessonVehicles = ref([]);

// 15-minute interval time slots from 07:00 to 20:00
const timeSlots = (() => {
    const slots = [];
    for (let h = 7; h <= 20; h++) {
        for (let m = 0; m < 60; m += 15) {
            if (h === 20 && m > 0) break; // stop at 20:00
            slots.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`);
        }
    }
    return slots;
})();

// Computed end time = start + 45 min
const computedEndTime = computed(() => calcEndTime(newLessonTime.value));

function calcEndTime(startTime) {
    if (!startTime) return '';
    const parts = startTime.split(':');
    if (parts.length !== 2) return '';
    const h = parseInt(parts[0], 10);
    const m = parseInt(parts[1], 10);
    const totalMin = h * 60 + m + 45;
    return `${String(Math.floor(totalMin / 60)).padStart(2, '0')}:${String(totalMin % 60).padStart(2, '0')}`;
}

const canSubmit = computed(() => !!lessonDateDisplay.value && !!newLessonTime.value);

const sortedLessons = computed(() => {
    const list = practicalLessons.value || [];
    return [...list].sort((a, b) => {
        const dA = (a.lessonDate || '').split('.').reverse().join('');
        const dB = (b.lessonDate || '').split('.').reverse().join('');
        if (dA !== dB) return dA.localeCompare(dB);
        return (a.time || '').localeCompare(b.time || '');
    });
});

function formatDate(value) {
    if (!value) return '';
    // Vuetify 3 date picker returns a Date object
    if (value instanceof Date) {
        const dd = String(value.getDate()).padStart(2, '0');
        const mm = String(value.getMonth() + 1).padStart(2, '0');
        const yyyy = value.getFullYear();
        return `${dd}.${mm}.${yyyy}`;
    }
    // Fallback for ISO string "2026-02-08" or "2026-02-08T00:00:00"
    const str = String(value).split('T')[0];
    const parts = str.split('-');
    if (parts.length === 3) return `${parts[2]}.${parts[1]}.${parts[0]}`;
    return str;
}

const handleLessonDateChange = (value) => {
    if (value) {
        lessonDateDisplay.value = formatDate(value);
        lessonDateModel.value = typeof value === 'string' ? value : value;
    } else {
        lessonDateDisplay.value = '';
        lessonDateModel.value = null;
    }
    nextTick(() => { lessonDateMenu.value = false; });
};

const clearLessonDate = () => {
    lessonDateDisplay.value = '';
    lessonDateModel.value = null;
};

const loadDetails = () => {
    loadError.value = '';
    detailsLoading.value = true;
    candidateStore.getCandidateDetails(props.candidateId)
        .then((response) => {
            candidate.value = response.data?.candidate ?? null;
            practicalLessons.value = response.data?.practicalLessons ?? [];
        })
        .catch(() => {
            loadError.value = 'Could not load candidate. You may only edit candidates assigned to you.';
        })
        .finally(() => {
            detailsLoading.value = false;
        });
};

const loadLessonVehicles = () => {
    candidateStore.getLessonVehicles()
        .then((response) => {
            const raw = response?.data?.data;
            lessonVehicles.value = Array.isArray(raw) ? raw : ['Vehicle 1', 'Vehicle 2', 'Vehicle 3'];
        })
        .catch(() => {
            lessonVehicles.value = ['Vehicle 1', 'Vehicle 2', 'Vehicle 3'];
        });
};

const addLesson = async () => {
    addError.value = '';
    const dateStr = lessonDateDisplay.value;
    if (!dateStr || !newLessonTime.value) {
        addError.value = 'Date and Start time are required';
        return;
    }
    addingLesson.value = true;
    try {
        const response = await candidateStore.addPracticalLesson({
            candidateId: props.candidateId,
            lessonDate: dateStr,
            time: newLessonTime.value,
            vehicle: newLessonVehicle.value || null
        });

        // Check response body for error
        const body = response?.data;
        if (body && body.status === 'error') {
            addError.value = body.responseMsg || 'Failed to save lesson';
            return;
        }

        settingStore.toggleSnackbar({ status: true, msg: 'Lesson added successfully' });
        // Reset form
        lessonDateDisplay.value = '';
        lessonDateModel.value = null;
        newLessonTime.value = null;
        newLessonVehicle.value = null;
        // Refresh lessons list
        loadDetails();
        emit('saved');
    } catch (err) {
        // err.response exists for 4xx/5xx
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || err?.message || 'Error adding lesson';
        addError.value = msg;
    } finally {
        addingLesson.value = false;
    }
};

const closeDialog = () => emit('close');

watch(() => props.candidateId, () => {
    if (props.candidateId) {
        loadDetails();
        loadLessonVehicles();
    }
}, { immediate: true });

onMounted(() => {
    if (props.candidateId) {
        loadDetails();
        loadLessonVehicles();
    }
});
</script>

<style scoped>
.sticky-header {
    position: sticky;
    top: 0;
    background: rgb(var(--v-theme-surface));
    z-index: 1;
}
</style>
