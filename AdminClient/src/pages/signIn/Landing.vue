<template>
    <v-app>
        <v-navigation-drawer theme="dark" v-model="drawer" temporary absolute>
            <v-list-item v-text="allSettings.siteTitle" class="text-h6 py-4"></v-list-item>
            <v-divider></v-divider>
            <v-list density="default" nav>
                <v-list-item title="Home" prepend-icon="mdi-home" :href="homeUrl"></v-list-item>
                <v-list-item title="Features" prepend-icon="mdi-feature-search" :href="featuresUrl"></v-list-item>
                <v-list-item title="Contact" prepend-icon="mdi-account-box" :href="contactUrl"></v-list-item>
            </v-list>
            <v-divider></v-divider>
            <v-list density="default" nav>
                <v-list-item title="Sign In" prepend-icon="mdi-login" @click="switchToSignIn"></v-list-item>
            </v-list>
        </v-navigation-drawer>
        <v-app-bar density="comfortable" elevation="10" style="position: sticky; top: 0;">
            <template v-slot:prepend>
                <v-avatar rounded="0" size="40">
                    <v-img :src="logoSrc" max-height="50" max-width="70"></v-img>
                </v-avatar>
            </template>
            <template v-if="smAndDown" v-slot:append>
                <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
            </template>
            <template v-else v-slot:default>
                <v-spacer></v-spacer>
                <div>
                    <v-btn text class="text-capitalize" :href="homeUrl">Home</v-btn>
                    <v-btn text class="text-capitalize" :href="featuresUrl">Features</v-btn>
                    <v-btn text class="text-capitalize" :href="contactUrl">Contact</v-btn>
                </div>
                <v-spacer></v-spacer>
                <div class="pr-1">
                    <v-btn class="text-capitalize" variant="outlined" @click="switchToSignIn">Sign In</v-btn>
                </div>
            </template>
        </v-app-bar>
        <v-main>
            <section id="home">
                <v-container class="mt-12">
                    <v-row justify="center">
                        <v-col cols="10">
                            <v-row>
                                <v-col cols="12" md="6">
                                    <h1 style="font-size: 45px;margin-bottom: 20px;">{{ allSettings.homeHeader1 }}</h1>
                                    <div style="font-size: 14px;font-weight: 300; margin-bottom: 35px;">
                                        {{ allSettings.homeDetail1 }}</div>
                                    <v-btn class="text-capitalize" variant="outlined"
                                        @click.stop="toggleRegistration">Get
                                        Registered</v-btn>
                                </v-col>
                                <v-col cols="12" md="6">
                                    <v-img max-height="450" max-width="900" :src="homeImg"></v-img>
                                </v-col>
                            </v-row>
                        </v-col>
                    </v-row>
                    <v-row justify="center" style="margin-top: 150px;">
                        <v-col cols="10" md="8">
                            <h1 style="font-size: 45px; text-align: center;">{{ allSettings.homeHeader2 }}</h1>
                        </v-col>
                    </v-row>
                    <v-row justify="center" style="margin-top: 20px; margin-bottom: 150px;">
                        <v-col cols="12">
                            <div style="font-size: 14px;font-weight: 300; text-align: center;">
                                {{ allSettings.homeDetail2 }}</div>
                        </v-col>
                    </v-row>
                    <v-row justify="center" class="mt-16 mb-16">
                        <v-col cols="12">
                            <v-row>
                                <v-col cols="12" sm="6" md="3">
                                    <v-card elevation="16" min-height="185px" rounded="shaped">
                                        <v-card-title class="bg-surface-variant pa-6">
                                            <v-row justify="center"
                                                class="text-white">{{ allSettings.homeBox1Header }}</v-row>
                                        </v-card-title>
                                        <v-card-text
                                            class="font-weight-light text-center">{{ allSettings.homeBox1Detail }}</v-card-text>
                                    </v-card>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-card elevation="16" min-height="185px" rounded="shaped">
                                        <v-card-title class="bg-surface-variant pa-6">
                                            <v-row justify="center"
                                                class="text-white">{{ allSettings.homeBox2Header }}</v-row>
                                        </v-card-title>
                                        <v-card-text
                                            class="font-weight-light text-center">{{ allSettings.homeBox2Detail }}</v-card-text>
                                    </v-card>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-card elevation="16" min-height="185px" rounded="shaped">
                                        <v-card-title class="bg-surface-variant pa-6">
                                            <v-row justify="center"
                                                class="text-white">{{ allSettings.homeBox3Header }}</v-row>
                                        </v-card-title>
                                        <v-card-text
                                            class="font-weight-light text-center">{{ allSettings.homeBox3Detail }}</v-card-text>
                                    </v-card>
                                </v-col>
                                <v-col cols="12" sm="6" md="3">
                                    <v-card elevation="16" min-height="185px" rounded="shaped">
                                        <v-card-title class="bg-surface-variant pa-6">
                                            <v-row justify="center"
                                                class="text-white">{{ allSettings.homeBox4Header }}</v-row>
                                        </v-card-title>
                                        <v-card-text
                                            class="font-weight-light text-center">{{ allSettings.homeBox4Detail }}</v-card-text>
                                    </v-card>
                                </v-col>

                            </v-row>
                        </v-col>
                    </v-row>
                </v-container>
            </section>

            <section id="features" class="mt-16 mb-16">
                <v-row justify="center">
                    <v-col cols="10">
                        <v-row>
                            <v-col cols="12" md="6" align-self="center">
                                <v-card flat>
                                    <v-card-title>
                                        <h3>{{ allSettings.feature1Header }}</h3>
                                    </v-card-title>
                                    <v-card-text class="font-weight-light">
                                        {{ allSettings.feature1Detail }}
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="6" align-self="center">
                                <v-img max-width="500" max-height="500" :src="getFeature1Img"></v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6" align-self="center" order-md="2">
                                <v-card flat>
                                    <v-card-title>
                                        <h3>{{ allSettings.feature2Header }}</h3>
                                    </v-card-title>
                                    <v-card-text class="font-weight-light">
                                        {{ allSettings.feature2Detail }}
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="6" order-md="1" align-self="center">
                                <v-img max-width="500" max-height="500" :src="getFeature2Img"></v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6" align-self="center">
                                <v-card flat>
                                    <v-card-title>
                                        <h3>{{ allSettings.feature3Header }}</h3>
                                    </v-card-title>
                                    <v-card-text class="font-weight-light">
                                        {{ allSettings.feature3Detail }}
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="6" align-self="center">
                                <v-img max-width="500" max-height="500" :src="getFeature3Img"></v-img>
                            </v-col>
                        </v-row>
                        <v-row>
                            <v-col cols="12" md="6" align-self="center" order-md="2">
                                <v-card flat>
                                    <v-card-title>
                                        <h3>{{ allSettings.feature4Header }}</h3>
                                    </v-card-title>
                                    <v-card-text class="font-weight-light">
                                        {{ allSettings.feature4Detail }}
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="6" align-self="center" order-md="1">
                                <v-img max-width="500" max-height="500" :src="getFeature4Img"></v-img>
                            </v-col>
                        </v-row>
                    </v-col>
                </v-row>
            </section>

            <section id="register" class="mt-16 mb-16">
                <v-card class="bg-surface-variant text-center" elevation="16">
                    <v-card-text class="pt-10">{{ allSettings.registrationText }}</v-card-text>
                    <v-row class="pb-10" justify="center">
                        <v-card-actions>
                            <v-btn class="text-capitalize" variant="outlined" @click.stop="toggleRegistration">Register
                                Now</v-btn>
                        </v-card-actions>
                    </v-row>
                </v-card>
            </section>

            <section id="contact" class="mt-16 mb-16">
                <v-row justify="center">
                    <v-col cols="10">
                        <v-row>
                            <v-col cols="12" sm="5">
                                <h1>Contact Us</h1>
                                <h4 class="font-weight-light mt-3">
                                    {{ allSettings.contactUsText }}
                                </h4>
                                <h4 class="font-weight-light mt-3">
                                    Telephone: {{ allSettings.contactUsTelephone }}
                                </h4>
                                <h4 class="font-weight-light">
                                    Email: {{ allSettings.contactUsEmail }}
                                </h4>
                            </v-col>
                            <v-col cols="12" sm="7">
                                <v-form v-model="valid" @submit.prevent="submitForm">
                                    <v-text-field v-model="formData.name" :rules="nameRules" label="Name"
                                        variant="underlined" required>
                                    </v-text-field>
                                    <v-text-field v-model="formData.email" :rules="emailRules" label="E-mail"
                                        variant="underlined" required>
                                    </v-text-field>
                                    <v-textarea v-model="formData.message" :rules="messageRules" variant="underlined"
                                        label="Message" required>
                                    </v-textarea>
                                    <v-btn :disabled="!valid" block rounded class="text-capitalize mt-10"
                                        color="grey-darken-3" type="submit" text="Submit">
                                    </v-btn>
                                </v-form>
                            </v-col>

                        </v-row>
                    </v-col>
                </v-row>
            </section>
        </v-main>
        <v-footer class="d-flex flex-column text-center justify-center bg-surface-variant mt-16">
            <div>
                <v-btn variant="text" icon="mdi-facebook" :href="allSettings.footerFacebook" target="_blank"></v-btn>
                <v-btn variant="text" icon="mdi-linkedin" :href="allSettings.footerLinkedin" target="_blank"></v-btn>
                <v-btn variant="text" icon="mdi-twitter" :href="allSettings.footerTwitter" target="_blank"></v-btn>
                <v-btn variant="text" icon="mdi-instagram" :href="allSettings.footerInstagram" target="_blank"></v-btn>
            </div>
            <div class="pt-2">
                {{ allSettings.footerText }}
            </div>
            <v-divider class="mt-4" color="white" length="950" thickness="1"></v-divider>
            <div class="pt-4">
                {{ new Date().getFullYear() }} — <strong>{{ allSettings.siteTitle }}</strong>
            </div>
        </v-footer>
    </v-app>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { useUserStore } from '@/store/UserStore';
import { useRouter } from 'vue-router';
import logo from '@/assets/vue-admin-logo.png'
import { ref, computed } from 'vue';
import { useDisplay } from 'vuetify';

const settingStore = useSettingStore()
const userStore = useUserStore()
const router = useRouter()
const { smAndDown } = useDisplay()
const drawer = ref(null)
const allSettings = ref({})
const valid = ref(false)
const homeUrl = ref(window.location.origin + '/#home')
const featuresUrl = ref(window.location.origin + '/#features')
const contactUrl = ref(window.location.origin + '/#contact')
const formData = ref({
    name: '',
    email: '',
    message: ''
})
const nameRules = [
    (v) => !!v || 'Name is required',
    (v) => (v && v.length >= 3) || 'Name must be more than 3 characters',
]
const emailRules = [
    (v) => !!v || 'E-mail is required',
    (v) => /.+@.+\..+/.test(v) || 'E-mail must be valid',
]

const messageRules = [
    (v) => !!v || 'Message is required',
    (v) => (v && v.length >= 10) || 'Message must be more than 10 characters',
]

//fetch settings
settingStore.fetchSiteSettings()
    .then((res) => {
        if (res.status == 200) {
            document.getElementById('favicon').href = import.meta.env.VITE_API_URL + res.data.faviconPath
            allSettings.value = res.data
        }
    })
    .catch(error => {
        console.log('error', error)
    })

//registration window open
const toggleRegistration = () => {
    userStore.setRegistration()
    router.push({ name: 'SignIn' })
}

//get logo
const logoSrc = computed(() => {
    return allSettings.value.logoPath == '' ? logo : import.meta.env.VITE_API_URL + allSettings.value.logoPath
})
//home img
const homeImg = computed(() => {
    return import.meta.env.VITE_API_URL + allSettings.value.homePicture
})
//feature1 img
const getFeature1Img = computed(() => {
    return import.meta.env.VITE_API_URL + allSettings.value.feature1Picture
})
//feature2 img
const getFeature2Img = computed(() => {
    return import.meta.env.VITE_API_URL + allSettings.value.feature2Picture
})
//feature3 img
const getFeature3Img = computed(() => {
    return import.meta.env.VITE_API_URL + allSettings.value.feature3Picture
})
//feature4 img
const getFeature4Img = computed(() => {
    return import.meta.env.VITE_API_URL + allSettings.value.feature4Picture
})

//Switch to Sign In
const switchToSignIn = () => {
    router.push({ name: 'SignIn' })
}

//submit contact form
const submitForm = () => {
    settingStore.createContact(formData.value)
        .then((response) => {
            if (response.status == 200) {
                console.log(response)
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

//sign out
userStore.signOut()
</script>