<template>
    <v-container class="d-flex ga-12 flex-column">
        <v-row class="py-4">
            <v-col cols="12" sm="3">
                <v-card color="grey-darken-1" @click="switchToUser">
                    <v-card-title>
                        <v-icon size="medium">mdi-account-circle</v-icon>
                    </v-card-title>
                    <v-card-text>Total <span>{{ userStatus.totalUser }}</span></v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="3">
                <v-card color="grey-darken-2" @click="switchToUser">
                    <v-card-title>
                        <v-icon size="medium">mdi-thumb-up</v-icon>
                    </v-card-title>
                    <v-card-text>Active <span>{{ userStatus.activeUser }}</span></v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="3">
                <v-card color="grey-darken-3" @click="switchToUser">
                    <v-card-title>
                        <v-icon size="medium">mdi-thumb-down</v-icon>
                    </v-card-title>
                    <v-card-text>In-Active <span>{{ userStatus.inActiveUser }}</span></v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" sm="3">
                <v-card color="grey-darken-4" @click="switchToUser">
                    <v-card-title>
                        <v-icon size="medium">mdi-shield</v-icon>
                    </v-card-title>
                    <v-card-text>Admin <span>{{ userStatus.adminUser }}</span></v-card-text>
                </v-card>
            </v-col>
        </v-row>
        <v-row>
            <date-chart />
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
const userStatus = ref({})
const router = useRouter()

//get users status
userStore.getUserStatus()
    .then((res) => {
        settingStore.overlayToggle(false)
        userStatus.value = res.data
    })

//switch to user
const switchToUser = () => {
    router.push({ name: 'Users' })
}

</script>