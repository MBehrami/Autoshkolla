<template>
    <div class="reset-page">
        <v-card min-width="350" class="py-4" elevation="10">
            <v-card-title class="text-center">
                <div class="d-flex justify-center"><v-img @click="$router.push({ name: 'Landing' })" :src="logo"
                        max-height="50" max-width="70" class="cursor-pointer"></v-img></div>
                <div class="text-h6 font-weight-bold">{{ appInfo.siteTitle }}</div>
            </v-card-title>
            <v-form v-model="valid" @submit.prevent="passwordReset">
                <v-card-text>
                    <div class="text-body-1 font-weight-regular">
                        <span>Name:</span> <span>{{ userInfo.fullName }}</span>
                    </div>
                    <div class="text-body-1 font-weight-regular">
                        <span>Email:</span> <span>{{ userInfo.email }}</span>
                    </div>
                    <v-text-field v-model="resetForm.password" :rules="passwordRules" label="New Password"
                        variant="underlined" append-inner-icon="mdi-lock" type="password" required>
                    </v-text-field>
                    <v-text-field v-model="resetForm.confirmPassword" :rules="passwordRules"
                        label="Confirm New Password" variant="underlined" append-inner-icon="mdi-lock" type="password"
                        required>
                    </v-text-field>
                </v-card-text>
                <v-card-actions class="pt-0">
                    <v-btn :disabled="!valid" :loading="loading" block rounded variant="elevated" text="Reset"
                        type="submit" color="grey-darken-3" class="text-capitalize mt-4">
                    </v-btn>
                </v-card-actions>
            </v-form>
        </v-card>
    </div>
</template>

<script setup>
import { useRoute } from 'vue-router';
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { computed, ref } from 'vue';
import { storeToRefs } from 'pinia';

const route = useRoute()
const userStore = useUserStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(userStore)
const userInfo = ref({})
const appInfo = ref({})
const valid = ref(false)
const passwordRules = [
    (v) => !!v || 'Password is required',
    (v) => (v && v.length >= 6) || 'Password must be more than 6 characters',
]
const resetForm = ref({
    password: '',
    confirmPassword: ''
})

//app info
settingStore.fetchSiteSettings()
    .then((response) => {
        appInfo.value = response.data
    })

//user info
userStore.getSingleUserByRef(route.params.ref)
    .then((response) => {
        userInfo.value = response.data
    })

//logo
const logo = computed(() => {
    return import.meta.env.VITE_API_URL + appInfo.value.logoPath
})

//reset password
const passwordReset = () => {
    if (resetForm.value.password !== resetForm.value.confirmPassword) {
        settingStore.toggleSnackbar({ status: true, msg: 'Password not matched!' })
    } else {
        const obj = {
            password: resetForm.value.password,
            userId: userInfo.value.userId
        }
        userStore.passwordChange(obj)
            .then(response => {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            })
    }
}
</script>

<style scoped>
.reset-page {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
}
</style>