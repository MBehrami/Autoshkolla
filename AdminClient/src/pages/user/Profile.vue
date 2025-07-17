<template>
    <v-container>
        <v-form v-model="valid" @submit.prevent="updateProfile">
            <v-row>
                <v-col cols="12" md="6">
                    <v-text-field v-model="profileForm.fullName" :rules="nameRules" label="Name" variant="underlined"
                        prepend-icon="mdi-account" clearable required>
                    </v-text-field>
                </v-col>
                <v-col cols="12" md="6">
                    <v-text-field v-model="profileForm.email" :rules="emailRules" label="Email" variant="underlined"
                        prepend-icon="mdi-email" readonly required>
                    </v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="6">
                    <v-text-field v-model="profileForm.mobile" label="Mobile" variant="underlined"
                        prepend-icon="mdi-cellphone" clearable>
                    </v-text-field>
                </v-col>
                <v-col cols="12" md="6">
                    <v-menu v-model="birthMenu" :close-on-content-click="false" :nudge-right="40"
                        transition="scale-transition" offset-y min-width="auto">
                        <template v-slot:activator="{ props }">
                            <v-text-field v-model="profileForm.dateOfBirth" label="Birth Date"
                                prepend-icon="mdi-calendar" readonly v-bind="props" variant="underlined"
                                clearable></v-text-field>
                        </template>
                        <v-date-picker v-model="profileForm.dateOfBirth" color="primary"
                            @input="birthMenu = false"></v-date-picker>
                    </v-menu>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="6">
                    <v-file-input accept="image/*" label="Profile Picture" prepend-icon="mdi-camera"
                        variant="underlined" @update:model-value="onImageChange" show-size>
                    </v-file-input>
                </v-col>
                <v-col cols="12" md="6">
                    <v-img :src="imagePreviewSrc" max-height="100" max-width="150" contain>
                    </v-img>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="6">
                    <v-btn :disabled="!valid" :loading="loading" text="Update" type="submit" color="grey-darken-3"
                        class="text-capitalize">
                    </v-btn>
                </v-col>
            </v-row>
        </v-form>
    </v-container>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';
import { useDate } from 'vuetify';

const userStore = useUserStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(userStore)
const date = useDate()
const birthMenu = ref(false)
const imagePreviewSrc = ref('')
const valid = ref(false)
const nameRules = [
    (v) => !!v || 'Name is required',
    (v) => (v && v.length >= 3) || 'Name must be more than 3 characters',
]
const emailRules = [
    (v) => !!v || 'E-mail is required',
    (v) => /.+@.+\..+/.test(v) || 'E-mail must be valid',
]
const profileForm = ref({
    userId: localStorage.getItem('userId'),
    fullName: '',
    email: '',
    mobile: '',
    dateOfBirth: null,
    imagePath: null,
    lastUpdatedBy: localStorage.getItem('userId'),
})

//initialize profile
userStore.getSingleUser(localStorage.getItem('userId'))
    .then(res => {
        if (res.status == 200) {
            //console.log(res.data)
            profileForm.value.fullName = res.data.fullName
            profileForm.value.email = res.data.email
            profileForm.value.mobile = res.data.mobile
            profileForm.value.dateOfBirth = res.data.dateOfBirth != null ? new Date(res.data.dateOfBirth.substr(0, 10)) : null
            profileForm.value.imagePath = res.data.imagePath
            imagePreviewSrc.value = res.data.imagePath != null ? import.meta.env.VITE_API_URL + res.data.imagePath : ''
        }
    })

//image change event
const onImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    profileForm.value.imagePath = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        imagePreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//profile update
const updateProfile = () => {
    const obj = {
        ...profileForm.value,
        dateOfBirth: profileForm.value.dateOfBirth != null ? date.toISO(profileForm.value.dateOfBirth) : null
    }
    userStore.updateProfile(obj)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

</script>