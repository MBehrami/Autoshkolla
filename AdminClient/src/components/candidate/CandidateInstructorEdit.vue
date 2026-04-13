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
                <h3 class="text-subtitle-1 font-weight-medium mb-3">Informatat e kandidatit (read-only)</h3>
                <v-row dense>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.serialNumber" label="Serial Number (Nr. Rendor)" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.firstName" label="First Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col v-if="!isAdditionalLessonMode" cols="12" md="4">
                        <v-text-field :model-value="candidate.parentName" label="Parent Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.lastName" label="Last Name" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col v-if="!isAdditionalLessonMode" cols="12" md="4">
                        <v-text-field :model-value="candidate.dateOfBirth" label="Date of Birth" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.phoneNumber" label="Phone Number" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col v-if="!isAdditionalLessonMode" cols="12" md="4">
                        <v-text-field :model-value="candidate.address" label="Komuna" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.categoryName" label="Category" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                    <v-col cols="12" md="4">
                        <v-text-field :model-value="candidate.vehicleType" label="Vehicle Type" variant="outlined" density="compact" readonly hide-details></v-text-field>
                    </v-col>
                </v-row>
                <v-chip v-if="isAdditionalLessonMode" color="info" variant="tonal" size="small" class="mt-2 mb-2">Orë Shtesë</v-chip>

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
                            <th class="text-left">Statusi</th>
                            <th class="text-left">Veprimet</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(lesson, index) in sortedLessons" :key="lesson.practicalLessonId || index"
                            :class="{ 'cancelled-row': lesson.status === 'Cancelled' }">
                            <td>{{ index + 1 }}</td>
                            <td>{{ lesson.lessonDate }}</td>
                            <td>{{ lesson.time }}</td>
                            <td>{{ lesson.endTime || calcEndTime(lesson.time) }}</td>
                            <td>{{ lesson.vehicle || '–' }}</td>
                            <td>{{ lesson.instructorName || '–' }}</td>
                            <td>
                                <v-chip v-if="lesson.status === 'Cancelled'" color="error" size="x-small" variant="tonal">
                                    Anuluar
                                    <v-tooltip activator="parent" location="top" v-if="lesson.cancellationReason">{{ lesson.cancellationReason }}</v-tooltip>
                                </v-chip>
                                <v-chip v-else color="success" size="x-small" variant="tonal">Aktive</v-chip>
                            </td>
                            <td>
                                <v-btn v-if="lesson.status !== 'Cancelled' && !isLessonInPast(lesson)" icon variant="text" color="error" size="x-small"
                                    @click="openCancelDialog(lesson)">
                                    <v-icon size="16">mdi-cancel</v-icon>
                                    <v-tooltip activator="parent" location="top">Anulo orën</v-tooltip>
                                </v-btn>
                                <v-icon v-else-if="lesson.status !== 'Cancelled'" size="16" color="grey" class="ml-1">mdi-lock-outline</v-icon>
                                <span v-else class="text-medium-emphasis text-caption">–</span>
                            </td>
                        </tr>
                        <tr v-if="!sortedLessons.length">
                            <td colspan="8" class="text-center text-medium-emphasis py-4">No practical lessons yet.</td>
                        </tr>
                    </tbody>
                </v-table>

                <!-- Cancel Lesson Dialog -->
                <v-dialog v-model="cancelDialog" max-width="450">
                    <v-card rounded="lg">
                        <v-card-title class="pa-4">Anulo orën praktike</v-card-title>
                        <v-divider></v-divider>
                        <v-card-text class="pa-4">
                            <v-alert type="warning" density="compact" variant="tonal" class="mb-3">
                                Ora do të anulohet dhe do t'i kthehet kandidatit.
                            </v-alert>
                            <v-textarea v-model="cancelReason" label="Arsyeja e anulimit *" variant="outlined"
                                density="compact" rows="3" :rules="[v => !!v || 'Arsyeja eshte e detyrueshme']"
                                auto-grow></v-textarea>
                            <v-alert v-if="cancelError" type="error" density="compact" class="mt-2" closable
                                @click:close="cancelError = ''">{{ cancelError }}</v-alert>
                        </v-card-text>
                        <v-card-actions class="pa-4">
                            <v-spacer></v-spacer>
                            <v-btn variant="text" @click="cancelDialog = false">Mbyll</v-btn>
                            <v-btn color="error" variant="elevated" :loading="cancelling" :disabled="!cancelReason"
                                @click="confirmCancel">Anulo orën</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>

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
                                item-title="label"
                                item-value="plateNumber"
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
    candidateId: { type: Number, default: null },
    additionalLessonId: { type: Number, default: null }
});

const emit = defineEmits(['close', 'saved']);

const candidateStore = useCandidateStore();
const settingStore = useSettingStore();

const isAdditionalLessonMode = computed(() => !!props.additionalLessonId);

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
            if (h === 20 && m > 0) break;
            slots.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`);
        }
    }
    return slots;
})();

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

function isLessonInPast(lesson) {
    const dateStr = lesson.lessonDate || lesson.date;
    if (!dateStr) return false;
    const parts = dateStr.split('.');
    if (parts.length !== 3) return false;
    const lessonDate = new Date(parseInt(parts[2]), parseInt(parts[1]) - 1, parseInt(parts[0]));
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return lessonDate < today;
}

// Cancel lesson
const cancelDialog = ref(false);
const cancelReason = ref('');
const cancelError = ref('');
const cancelling = ref(false);
const cancellingLessonId = ref(null);

function openCancelDialog(lesson) {
    cancellingLessonId.value = lesson.practicalLessonId;
    cancelReason.value = '';
    cancelError.value = '';
    cancelDialog.value = true;
}

async function confirmCancel() {
    if (!cancelReason.value || !cancellingLessonId.value) return;
    cancelling.value = true;
    cancelError.value = '';
    try {
        const res = await candidateStore.cancelPracticalLesson(cancellingLessonId.value, { reason: cancelReason.value });
        const body = res?.data;
        if (body && body.status === 'error') {
            cancelError.value = body.responseMsg || 'Gabim gjatë anulimit';
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Ora u anulua me sukses' });
        cancelDialog.value = false;
        loadDetails();
        emit('saved');
    } catch (err) {
        cancelError.value = err?.response?.data?.responseMsg || 'Gabim gjatë anulimit';
    } finally {
        cancelling.value = false;
    }
}

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
    if (value instanceof Date) {
        const dd = String(value.getDate()).padStart(2, '0');
        const mm = String(value.getMonth() + 1).padStart(2, '0');
        const yyyy = value.getFullYear();
        return `${dd}.${mm}.${yyyy}`;
    }
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

    const promise = isAdditionalLessonMode.value
        ? candidateStore.getAdditionalLessonDetails(props.additionalLessonId)
        : candidateStore.getCandidateDetails(props.candidateId);

    promise
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
            if (Array.isArray(raw) && raw.length > 0 && typeof raw[0] === 'object') {
                lessonVehicles.value = raw;
            } else {
                lessonVehicles.value = [];
            }
        })
        .catch(() => {
            lessonVehicles.value = [];
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
        const payload = {
            lessonDate: dateStr,
            time: newLessonTime.value,
            vehicle: newLessonVehicle.value || null
        };

        if (isAdditionalLessonMode.value) {
            payload.additionalLessonId = props.additionalLessonId;
            payload.candidateId = null;
        } else {
            payload.candidateId = props.candidateId;
            payload.additionalLessonId = null;
        }

        const response = await candidateStore.addPracticalLesson(payload);

        const body = response?.data;
        if (body && body.status === 'error') {
            addError.value = body.responseMsg || 'Failed to save lesson';
            return;
        }

        settingStore.toggleSnackbar({ status: true, msg: 'Lesson added successfully' });
        lessonDateDisplay.value = '';
        lessonDateModel.value = null;
        newLessonTime.value = null;
        newLessonVehicle.value = null;
        loadDetails();
        emit('saved');
    } catch (err) {
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || err?.message || 'Error adding lesson';
        addError.value = msg;
    } finally {
        addingLesson.value = false;
    }
};

const closeDialog = () => emit('close');

const activeId = computed(() => props.additionalLessonId || props.candidateId);

watch(activeId, () => {
    if (activeId.value) {
        loadDetails();
        loadLessonVehicles();
    }
}, { immediate: true });

onMounted(() => {
    if (activeId.value) {
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
.cancelled-row {
    opacity: 0.6;
    text-decoration: line-through;
    background: rgba(239, 68, 68, 0.04);
}
</style>
