<template>
    <v-container fluid class="dashboard-container pa-4 pa-md-6">
        <!-- ─── Summary Cards ─── -->
        <v-row class="mb-2">
            <v-col cols="12" sm="6" md="3">
                <v-card class="summary-card" elevation="2" rounded="lg" @click="switchToUser">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="blue-lighten-4" size="52">
                            <v-icon icon="mdi-account-group" color="blue-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ userStatus.totalUser }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Users</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="3">
                <v-card class="summary-card" elevation="2" rounded="lg" @click="switchToUser">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="green-lighten-4" size="52">
                            <v-icon icon="mdi-thumb-up" color="green-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ userStatus.activeUser }}</div>
                            <div class="text-body-2 text-medium-emphasis">Active Users</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="3">
                <v-card class="summary-card" elevation="2" rounded="lg" @click="switchToUser">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="orange-lighten-4" size="52">
                            <v-icon icon="mdi-thumb-down" color="orange-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ userStatus.inActiveUser }}</div>
                            <div class="text-body-2 text-medium-emphasis">Inactive Users</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="3">
                <v-card class="summary-card" elevation="2" rounded="lg" @click="switchToUser">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="purple-lighten-4" size="52">
                            <v-icon icon="mdi-shield-account" color="purple-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ userStatus.adminUser }}</div>
                            <div class="text-body-2 text-medium-emphasis">Admin Users</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Business Summary Cards ─── -->
        <v-row class="mb-2">
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="indigo-lighten-4" size="52">
                            <v-icon icon="mdi-account-school" color="indigo-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ summary.totalCandidates }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Candidates</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="teal-lighten-4" size="52">
                            <v-icon icon="mdi-account-tie" color="teal-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ summary.totalInstructors }}</div>
                            <div class="text-body-2 text-medium-emphasis">Total Instructors</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="6" md="4">
                <v-card class="summary-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-4 pa-4">
                        <v-avatar color="cyan-lighten-4" size="52">
                            <v-icon icon="mdi-car" color="cyan-darken-2" size="28"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h5 font-weight-bold">{{ summary.activeVehicles }}</div>
                            <div class="text-body-2 text-medium-emphasis">Active Vehicles</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Charts ─── -->
        <v-row>
            <v-col cols="12">
                <date-chart />
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" md="6">
                <role-chart />
            </v-col>
            <v-col cols="12" md="6">
                <browser-chart />
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" md="6">
                <month-chart />
            </v-col>
            <v-col cols="12" md="6">
                <year-chart />
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup>
import DateChart from '@/components/chart/DateChart.vue';
import MonthChart from '@/components/chart/MonthChart.vue';
import YearChart from '@/components/chart/YearChart.vue';
import BrowserChart from '@/components/chart/BrowserChart.vue';
import RoleChart from '@/components/chart/RoleChart.vue';
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { useRouter } from 'vue-router';
import { ref } from 'vue';

const userStore = useUserStore()
const settingStore = useSettingStore()
const userStatus = ref({
    totalUser: 0,
    activeUser: 0,
    inActiveUser: 0,
    adminUser: 0
})
const summary = ref({
    totalCandidates: 0,
    totalInstructors: 0,
    activeVehicles: 0
})
const router = useRouter()

// Default status so template never reads undefined
const defaultStatus = () => ({
    totalUser: 0,
    activeUser: 0,
    inActiveUser: 0,
    adminUser: 0
})

// Get users status; handle 403/401 so Dashboard does not crash
userStore.getUserStatus()
    .then((res) => {
        const data = res?.data
        userStatus.value = (data && typeof data === 'object')
            ? { ...defaultStatus(), ...data }
            : defaultStatus()
    })
    .catch(() => {
        userStatus.value = defaultStatus()
    })
    .finally(() => {
        settingStore.overlayToggle(false)
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

//switch to user
const switchToUser = () => {
    router.push({ name: 'Users' })
}
</script>

<style scoped>
.dashboard-container {
    max-width: 1400px;
    margin: 0 auto;
}

.summary-card {
    transition: transform 0.15s ease, box-shadow 0.15s ease;
    cursor: pointer;
}

.summary-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1) !important;
}
</style>
