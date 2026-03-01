<template>
    <v-container>
        <v-expansion-panels>
            <v-expansion-panel title="Konfigurimet e Përgjithshme" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="generalFormValidity" @submit.prevent="updateGeneralSettings">
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.siteTitle" :rules="[rules.required]"
                                    label="Titulli i Faqes" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.welComeMessage"
                                    :rules="[rules.required, rules.lengthChk]" label="Mesazhi i Mirëseardhjes"
                                    variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.copyRightText"
                                    :rules="[rules.required, rules.lengthChk]" label="Teksti i Të Drejtave të Autorit"
                                    variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.version" :rules="[rules.required]"
                                    label="Versioni i Aplikacionit" variant="underlined" type="number" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-checkbox v-model="generalSettingForm.allowWelcomeEmail" label="Lejoni Email-in e Mirëseardhjes">
                                </v-checkbox>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-checkbox v-model="generalSettingForm.allowFaq" label="Lejoni Pyetjet e Shpeshta">
                                </v-checkbox>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="3" sm="6">
                                <v-file-input accept="image/*" label="Logoja e Faqes" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onLogoChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-img :src="logoPreviewSrc" max-height="80" max-width="120">
                                </v-img>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-file-input accept="image/*" label="Favicon-i i Faqes" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onFaviconChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-img :src="faviconPreviewSrc" max-height="80" max-width="120">
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-btn :disabled="!generalFormValidity" :loading="loading" text="Përditëso" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel title="Konfigurimet e Email-it" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="emailFormValidity" @submit.prevent="updateEmailSettings">
                        <v-row>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.defaultEmail" label="Email"
                                    :rules="[rules.required, rules.emailChk]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.password" label="Fjalëkalim"
                                    :rules="[rules.required]" variant="underlined" type="password" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.host" label="Host" :rules="[rules.required]"
                                    variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.port" label="Port" :rules="[rules.required]"
                                    variant="underlined" type="number" clearable required>
                                </v-text-field>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-btn :disabled="!emailFormValidity" :loading="loading" text="Përditëso" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel title="Konfigurimet e Tekstit të Email-it" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="emailTextFormValidity" @submit.prevent="updateEmailTextSettings">
                        <div class="py-4 font-weight-bold">Email i Nulimit të Fjalëkalimit</div>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.forgetPasswordEmailHeader" label="Tema"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.forgetPasswordEmailSubject" label="Kreu"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-textarea v-model="emailTextSettingForm.forgetPasswordEmailBody" label="Trupi"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <div class="py-4 font-weight-bold">Email Mirëseardhjeje</div>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.welcomeEmailHeader" label="Tema"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.welcomeEmailSubject" label="Kreu"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-textarea v-model="emailTextSettingForm.welcomeEmailBody" label="Trupi"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-btn :disabled="!emailTextFormValidity" :loading="loading" text="Përditëso" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
        </v-expansion-panels>
    </v-container>

</template>

<script setup>
import { ref } from 'vue';
import { useSettingStore } from '@/store/SettingStore';
import { useUserStore } from '@/store/UserStore';
import { storeToRefs } from 'pinia';

const settingStore = useSettingStore()
const userStore = useUserStore()
const { loading } = storeToRefs(settingStore)
const logoPreviewSrc = ref('')
const faviconPreviewSrc = ref('')
const generalFormValidity = ref(false)
const emailFormValidity = ref(false)
const emailTextFormValidity = ref(false)
const generalSettingForm = ref({
    siteSettingsId: '',
    siteTitle: '',
    welComeMessage: '',
    copyRightText: '',
    logoPath: '',
    faviconPath: '',
    allowWelcomeEmail: false,
    allowFaq: false,
    version: '',
    lastUpdatedBy: localStorage.getItem('userId')
})
const emailSettingForm = ref({
    siteSettingsId: '',
    defaultEmail: '',
    password: '',
    host: '',
    port: '',
    lastUpdatedBy: localStorage.getItem('userId')
})
const emailTextSettingForm = ref({
    siteSettingsId: '',
    forgetPasswordEmailSubject: '',
    forgetPasswordEmailHeader: '',
    forgetPasswordEmailBody: '',
    welcomeEmailSubject: '',
    welcomeEmailHeader: '',
    welcomeEmailBody: '',
    lastUpdatedBy: localStorage.getItem('userId')
})
const rules = {
    required: (v) => !!v || 'I Nevojshëm',
    lengthChk: (v) => (v && v.length >= 10) || 'Emri duhet të ketë më shumë se ose të barabartë me 10 karaktere',
    emailChk: (v) => /.+@.+\.+/.test(v) || 'Email duhet të jetë i vlefshëm',
}

//initialize settings data
settingStore.fetchSiteSettings()
    .then((response) => {
        generalSettingForm.value.siteSettingsId = response.data.siteSettingsId
        generalSettingForm.value.siteTitle = response.data.siteTitle
        generalSettingForm.value.welComeMessage = response.data.welComeMessage
        generalSettingForm.value.copyRightText = response.data.copyRightText
        generalSettingForm.value.version = response.data.version
        generalSettingForm.value.allowFaq = response.data.allowFaq
        generalSettingForm.value.allowWelcomeEmail = response.data.allowWelcomeEmail
        generalSettingForm.value.welComeMessage = response.data.welComeMessage
        generalSettingForm.value.logoPath = response.data.logoPath
        logoPreviewSrc.value = response.data.logoPath != '' ? import.meta.env.VITE_API_URL + response.data.logoPath : ''
        generalSettingForm.value.faviconPath = response.data.faviconPath
        faviconPreviewSrc.value = response.data.faviconPath != '' ? import.meta.env.VITE_API_URL + response.data.faviconPath : ''

        emailSettingForm.value.siteSettingsId = response.data.siteSettingsId
        emailSettingForm.value.defaultEmail = response.data.defaultEmail
        emailSettingForm.value.password = response.data.password
        emailSettingForm.value.host = response.data.host
        emailSettingForm.value.port = response.data.port

        emailTextSettingForm.value.siteSettingsId = response.data.siteSettingsId
        emailTextSettingForm.value.forgetPasswordEmailHeader = response.data.forgetPasswordEmailHeader
        emailTextSettingForm.value.forgetPasswordEmailSubject = response.data.forgetPasswordEmailSubject
        emailTextSettingForm.value.forgetPasswordEmailBody = response.data.forgetPasswordEmailBody
        emailTextSettingForm.value.welcomeEmailHeader = response.data.welcomeEmailHeader
        emailTextSettingForm.value.welcomeEmailSubject = response.data.welcomeEmailSubject
        emailTextSettingForm.value.welcomeEmailBody = response.data.welcomeEmailBody
    })

//logo change event
const onLogoChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        settingStore.logoUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    generalSettingForm.value.logoPath = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        logoPreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//favicon change event
const onFaviconChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        settingStore.faviconUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    generalSettingForm.value.faviconPath = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        faviconPreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//general settings update
const updateGeneralSettings = () => {
    settingStore.generalSettingsUpdate(generalSettingForm.value)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: 'U Përditësua me Sukses' })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

//email settings update
const updateEmailSettings = () => {
    settingStore.emailSettingsUpdate(emailSettingForm.value)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: 'U Përditësua me Sukses' })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

//email text settings update
const updateEmailTextSettings = () => {
    settingStore.emailTextSettingsUpdate(emailTextSettingForm.value)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: 'U Përditësua me Sukses' })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

</script>