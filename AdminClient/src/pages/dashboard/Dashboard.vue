<template>
    <div class="page-container">
        <!-- Page Header -->
        <div class="page-header">
            <div class="page-title">Dashboard</div>
            <div class="page-subtitle">Mirë se vini! Këtu keni një pasqyrë të shpejtë të aktivitetit.</div>
        </div>

        <!-- Summary Cards -->
        <v-row>
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
                                Kandidatët
                            </v-btn>
                            <v-btn variant="tonal" color="success" class="text-none quick-link-btn" prepend-icon="mdi-calendar-clock" to="/schedules">
                                Oraret
                            </v-btn>
                            <v-btn variant="tonal" color="warning" class="text-none quick-link-btn" prepend-icon="mdi-car-clock" to="/driving-sessions">
                                Vozitjet
                            </v-btn>
                            <v-btn variant="tonal" color="info" class="text-none quick-link-btn" prepend-icon="mdi-file-document-outline" to="/daily-report">
                                Raporti
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
    </div>
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

const profileInfo = JSON.parse(localStorage.getItem('profile') || '{}')
const profileName = profileInfo?.obj?.fullName || ''
const profileRole = profileInfo?.obj?.roleName || ''

const todayDate = new Date().toLocaleDateString('sq-AL', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
})

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
