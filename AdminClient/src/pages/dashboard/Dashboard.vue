<template>
    <v-container fluid class="dashboard-container pa-4 pa-md-6">
        <!-- ─── Welcome Header ─── -->
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Dashboard</div>
            <div class="text-body-2 text-medium-emphasis">Overview of your driving school</div>
        </div>

        <!-- ─── Summary Cards ─── -->
        <v-row>
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="1" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <v-avatar color="#E8EAF6" size="56" rounded="lg">
                            <v-icon icon="mdi-account-school" color="#3949AB" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h4 font-weight-bold text-grey-darken-3">{{ summary.totalCandidates }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Candidates</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="1" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <v-avatar color="#E0F2F1" size="56" rounded="lg">
                            <v-icon icon="mdi-account-tie" color="#00796B" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h4 font-weight-bold text-grey-darken-3">{{ summary.totalInstructors }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Instructors</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="1" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-5">
                        <v-avatar color="#ECEFF1" size="56" rounded="lg">
                            <v-icon icon="mdi-car" color="#455A64" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h4 font-weight-bold text-grey-darken-3">{{ summary.activeVehicles }}</div>
                            <div class="text-body-2 text-medium-emphasis">Active Vehicles</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref } from 'vue';

const userStore = useUserStore()
const settingStore = useSettingStore()

const summary = ref({
    totalCandidates: 0,
    totalInstructors: 0,
    activeVehicles: 0
})

// Get business summary (candidates, instructors, vehicles)
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
    .finally(() => {
        settingStore.overlayToggle(false)
    })
</script>

<style scoped>
.dashboard-container {
    max-width: 1200px;
    margin: 0 auto;
}

.summary-card {
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    border: 1px solid rgba(0, 0, 0, 0.06);
}

.summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08) !important;
}
</style>
