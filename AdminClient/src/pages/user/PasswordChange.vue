<template>
    <v-container>
        <v-form v-model="valid" @submit.prevent="changePassword">
            <v-row>
                <v-col cols="12" md="6">
                    <v-text-field v-model="passwordForm.password" :rules="passwordRules" label="Fjalëkalimi"
                        variant="underlined" prepend-inner-icon="mdi-lock" type="password" required></v-text-field>
                </v-col>
                <v-col cols="12" md="6">
                    <v-text-field v-model="passwordForm.passwordConfirm" :rules="passwordRules" label="Konfirmo Fjalëkalimin"
                        variant="underlined" prepend-inner-icon="mdi-lock" type="password" required></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="6">
                    <v-btn :disabled="!valid" :loading="loading" text="Ndrysho" type="submit" color="grey-darken-3"
                        class="text-capitalize">
                    </v-btn>
                </v-col>
            </v-row>
        </v-form>
    </v-container>
</template>

<script setup>
import { ref } from 'vue';
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';

const userStore = useUserStore()
const settingStore = useSettingStore()
const valid = ref(false)
const passwordRules = [
    (v) => !!v || 'Fjalëkalimi është i detyrueshëm',
    (v) => (v && v.length >= 6) || 'Fjalëkalimi duhet të jetë më i gjatë se 6 karaktere',
]
const passwordForm = ref({
    password: '',
    passwordConfirm: ''
})
const { loading } = storeToRefs(userStore)

const changePassword = () => {
    if (passwordForm.value.password !== passwordForm.value.passwordConfirm) {
        settingStore.toggleSnackbar({ status: true, msg: 'Fjalëkalimi nuk përputhet!' })
    } else {
        const obj = {
            password: passwordForm.value.password,
            userId: localStorage.getItem('userId')
        }
        userStore.passwordChange(obj)
            .then(response => {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            })
    }

}
</script>