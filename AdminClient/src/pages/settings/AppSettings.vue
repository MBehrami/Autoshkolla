<template>
    <v-container>
        <v-expansion-panels>
            <v-expansion-panel title="General Settings" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="generalFormValidity" @submit.prevent="updateGeneralSettings">
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.siteTitle" :rules="[rules.required]"
                                    label="Site Title" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.welComeMessage"
                                    :rules="[rules.required, rules.lengthChk]" label="Welcome Message"
                                    variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.copyRightText"
                                    :rules="[rules.required, rules.lengthChk]" label="Copyright Text"
                                    variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="generalSettingForm.version" :rules="[rules.required]"
                                    label="App Version" variant="underlined" type="number" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-checkbox v-model="generalSettingForm.allowWelcomeEmail" label="Allow Welcome Email">
                                </v-checkbox>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-checkbox v-model="generalSettingForm.allowFaq" label="Allow Faq">
                                </v-checkbox>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="3" sm="6">
                                <v-file-input accept="image/*" label="Site Logo" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onLogoChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-img :src="logoPreviewSrc" max-height="80" max-width="120">
                                </v-img>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-file-input accept="image/*" label="Site Favicon" prepend-icon="mdi-camera"
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
                                <v-btn :disabled="!generalFormValidity" :loading="loading" text="Update" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel title="Email Settings" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="emailFormValidity" @submit.prevent="updateEmailSettings">
                        <v-row>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.defaultEmail" label="Email"
                                    :rules="[rules.required, rules.emailChk]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3" sm="6">
                                <v-text-field v-model="emailSettingForm.password" label="Password"
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
                                <v-btn :disabled="!emailFormValidity" :loading="loading" text="Update" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel title="Landing Page Settings" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="landingFormValidity" @submit.prevent="updateLandingSettings">
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeHeader1" label="Home Header Top"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeDetail1" label="Home Detail Top"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-file-input accept="image/*" label="Home Image" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onHomeImageChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-img :src="homeImagePreviewSrc" max-height="100" max-width="150" contain>
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeHeader2" label="Home Header Bottom"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeDetail2" label="Home Detail Bottom"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeBox1Header" label="Home Box1 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeBox1Detail" label="Home Box1 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeBox2Header" label="Home Box2 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeBox2Detail" label="Home Box2 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeBox3Header" label="Home Box3 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeBox3Detail" label="Home Box3 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.homeBox4Header" label="Home Box4 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.homeBox4Detail" label="Home Box4 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.feature1Header" label="Feature1 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.feature1Detail" label="Feature1 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-file-input accept="image/*" label="Feature1 Picture" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onFeature1ImageChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-img :src="feature1PreviewSrc" max-height="100" max-width="150" contain>
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.feature2Header" label="Feature2 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.feature2Detail" label="Feature2 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-file-input accept="image/*" label="Feature2 Picture" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onFeature2ImageChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-img :src="feature2PreviewSrc" max-height="100" max-width="150" contain>
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.feature3Header" label="Feature3 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.feature3Detail" label="Feature3 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-file-input accept="image/*" label="Feature3 Picture" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onFeature3ImageChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-img :src="feature3PreviewSrc" max-height="100" max-width="150" contain>
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-text-field v-model="landingSettingForm.feature4Header" label="Feature4 Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-textarea v-model="landingSettingForm.feature1Detail" label="Feature1 Detail"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-file-input accept="image/*" label="Feature4 Picture" prepend-icon="mdi-camera"
                                    variant="underlined" @update:model-value="onFeature4ImageChange" show-size>
                                </v-file-input>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-img :src="feature4PreviewSrc" max-height="100" max-width="150" contain>
                                </v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12">
                                <v-textarea v-model="landingSettingForm.registrationText" label="Registration Text"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="4">
                                <v-textarea v-model="landingSettingForm.contactUsText" label="Contact US Text"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                            <v-col cols="12" md="4">
                                <v-text-field v-model="landingSettingForm.contactUsEmail" label="Contact US Email"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4">
                                <v-text-field v-model="landingSettingForm.contactUsTelephone"
                                    label="Contact US Telephone" :rules="[rules.required]" variant="underlined"
                                    clearable required>
                                </v-text-field>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12">
                                <v-textarea v-model="landingSettingForm.footerText" label="Landing Footer Text"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="3">
                                <v-text-field v-model="landingSettingForm.footerFacebook" label="Footer Facebook"
                                    variant="underlined" clearable>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3">
                                <v-text-field v-model="landingSettingForm.footerInstagram" label="Footer Instagram"
                                    variant="underlined" clearable>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3">
                                <v-text-field v-model="landingSettingForm.footerLinkedin" label="Footer LinkedIn"
                                    variant="underlined" clearable>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="3">
                                <v-text-field v-model="landingSettingForm.footerTwitter" label="Footer Twitter"
                                    variant="underlined" clearable>
                                </v-text-field>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-btn :disabled="!landingFormValidity" :loading="loading" text="Update" type="submit"
                                    color="grey-darken-3" class="text-capitalize">
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel title="Email Text Settings" class="font-weight-bold">
                <v-expansion-panel-text>
                    <v-form v-model="emailTextFormValidity" @submit.prevent="updateEmailTextSettings">
                        <div class="py-4 font-weight-bold">Forget Password Email</div>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.forgetPasswordEmailHeader" label="Subject"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.forgetPasswordEmailSubject" label="Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-textarea v-model="emailTextSettingForm.forgetPasswordEmailBody" label="Body"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <div class="py-4 font-weight-bold">Welcome Email</div>
                        <v-row>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.welcomeEmailHeader" label="Subject"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-text-field v-model="emailTextSettingForm.welcomeEmailSubject" label="Header"
                                    :rules="[rules.required]" variant="underlined" clearable required>
                                </v-text-field>
                            </v-col>
                            <v-col cols="12" md="4" sm="6">
                                <v-textarea v-model="emailTextSettingForm.welcomeEmailBody" label="Body"
                                    :rules="[rules.required]" variant="underlined" auto-grow clearable required>
                                </v-textarea>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-btn :disabled="!emailTextFormValidity" :loading="loading" text="Update" type="submit"
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
const homeImagePreviewSrc = ref('')
const feature1PreviewSrc = ref('')
const feature2PreviewSrc = ref('')
const feature3PreviewSrc = ref('')
const feature4PreviewSrc = ref('')
const generalFormValidity = ref(false)
const emailFormValidity = ref(false)
const emailTextFormValidity = ref(false)
const landingFormValidity = ref(false)
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
const landingSettingForm = ref({
    siteSettingsId: '',
    homeHeader1: '',
    homeDetail1: '',
    homePicture: '',
    homeHeader2: '',
    homeDetail2: '',
    homeBox1Header: '',
    homeBox1Detail: '',
    homeBox2Header: '',
    homeBox2Detail: '',
    homeBox3Header: '',
    homeBox3Detail: '',
    homeBox4Header: '',
    homeBox4Detail: '',
    feature1Header: '',
    feature1Detail: '',
    feature1Picture: '',
    feature2Header: '',
    feature2Detail: '',
    feature2Picture: '',
    feature3Header: '',
    feature3Detail: '',
    feature3Picture: '',
    feature4Header: '',
    feature4Detail: '',
    feature4Picture: '',
    registrationText: '',
    contactUsText: '',
    contactUsTelephone: '',
    contactUsEmail: '',
    footerText: '',
    footerFacebook: '',
    footerTwitter: '',
    footerLinkedin: '',
    footerInstagram: '',
    lastUpdatedBy: localStorage.getItem('userId')
})
const rules = {
    required: (v) => !!v || 'Required',
    lengthChk: (v) => (v && v.length >= 10) || 'Name must be greater than or equal to 10 characters',
    emailChk: (v) => /.+@.+\..+/.test(v) || 'E-mail must be valid',
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

        landingSettingForm.value.siteSettingsId = response.data.siteSettingsId
        landingSettingForm.value.homeHeader1 = response.data.homeHeader1
        landingSettingForm.value.homeDetail1 = response.data.homeDetail1
        landingSettingForm.value.homePicture = response.data.homePicture
        homeImagePreviewSrc.value = response.data.homePicture != '' ? import.meta.env.VITE_API_URL + response.data.homePicture : ''
        landingSettingForm.value.homeHeader2 = response.data.homeHeader2
        landingSettingForm.value.homeDetail2 = response.data.homeDetail2
        landingSettingForm.value.homeBox1Header = response.data.homeBox1Header
        landingSettingForm.value.homeBox1Detail = response.data.homeBox1Detail
        landingSettingForm.value.homeBox2Header = response.data.homeBox2Header
        landingSettingForm.value.homeBox2Detail = response.data.homeBox2Detail
        landingSettingForm.value.homeBox3Header = response.data.homeBox3Header
        landingSettingForm.value.homeBox3Detail = response.data.homeBox3Detail
        landingSettingForm.value.homeBox4Header = response.data.homeBox4Header
        landingSettingForm.value.homeBox4Detail = response.data.homeBox4Detail
        landingSettingForm.value.feature1Header = response.data.feature1Header
        landingSettingForm.value.feature1Detail = response.data.feature1Detail
        feature1PreviewSrc.value = response.data.feature1Picture != '' ? import.meta.env.VITE_API_URL + response.data.feature1Picture : ''
        landingSettingForm.value.feature1Picture = response.data.feature1Picture
        landingSettingForm.value.feature2Header = response.data.feature2Header
        landingSettingForm.value.feature2Detail = response.data.feature2Detail
        feature2PreviewSrc.value = response.data.feature2Picture != '' ? import.meta.env.VITE_API_URL + response.data.feature2Picture : ''
        landingSettingForm.value.feature2Picture = response.data.feature2Picture
        landingSettingForm.value.feature3Header = response.data.feature3Header
        landingSettingForm.value.feature3Detail = response.data.feature3Detail
        feature3PreviewSrc.value = response.data.feature3Picture != '' ? import.meta.env.VITE_API_URL + response.data.feature3Picture : ''
        landingSettingForm.value.feature3Picture = response.data.feature3Picture
        landingSettingForm.value.feature4Header = response.data.feature4Header
        landingSettingForm.value.feature4Detail = response.data.feature4Detail
        feature4PreviewSrc.value = response.data.feature4Picture != '' ? import.meta.env.VITE_API_URL + response.data.feature4Picture : ''
        landingSettingForm.value.feature4Picture = response.data.feature4Picture
        landingSettingForm.value.registrationText = response.data.registrationText
        landingSettingForm.value.contactUsText = response.data.contactUsText
        landingSettingForm.value.contactUsEmail = response.data.contactUsEmail
        landingSettingForm.value.contactUsTelephone = response.data.contactUsTelephone
        landingSettingForm.value.footerText = response.data.footerText
        landingSettingForm.value.footerFacebook = response.data.footerFacebook
        landingSettingForm.value.footerInstagram = response.data.footerInstagram
        landingSettingForm.value.footerLinkedin = response.data.footerLinkedin
        landingSettingForm.value.footerTwitter = response.data.footerTwitter
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

//home image change event
const onHomeImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    landingSettingForm.value.homePicture = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        homeImagePreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//Feature1 image change event
const onFeature1ImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    landingSettingForm.value.feature1Picture = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        feature1PreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//Feature2 image change event
const onFeature2ImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    landingSettingForm.value.feature2Picture = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        feature2PreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//Feature3 image change event
const onFeature3ImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    landingSettingForm.value.feature3Picture = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        feature3PreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//Feature4 image change event
const onFeature4ImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    landingSettingForm.value.feature4Picture = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        feature4PreviewSrc.value = event.target.result
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
                settingStore.toggleSnackbar({ status: true, msg: 'Successfully Updated' })
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
                settingStore.toggleSnackbar({ status: true, msg: 'Successfully Updated' })
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
                settingStore.toggleSnackbar({ status: true, msg: 'Successfully Updated' })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

//landing page settings update
const updateLandingSettings = () => {
    settingStore.landingSettingsUpdate(landingSettingForm.value)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: 'Successfully Updated' })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

</script>