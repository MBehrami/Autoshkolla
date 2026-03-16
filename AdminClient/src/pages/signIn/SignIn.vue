<template>
    <div class="login-page">
        <div class="login-wrapper">
            <!-- Logo & Branding -->
            <div class="login-brand">
                <v-img @click="switchToLanding" :src="logoSrc" max-width="120" class="cursor-pointer mx-auto"></v-img>
            </div>

            <!-- Login Card -->
            <div class="login-card">
                <div class="login-card-header">
                    <h1 class="login-title">Mirë se vini</h1>
                    <p class="login-subtitle">Sistemi i menaxhimit</p>
                </div>

                <v-form v-model="signInFormValid" @submit.prevent="submitSignInForm">
                    <div class="login-fields">
                        <div class="field-group">
                            <label class="field-label">Email</label>
                            <v-text-field
                                v-model="signInForm.email"
                                :rules="emailRules"
                                placeholder="emri@email.com"
                                variant="outlined"
                                density="comfortable"
                                prepend-inner-icon="mdi-email-outline"
                                hide-details="auto"
                                required
                                class="login-input"
                            ></v-text-field>
                        </div>

                        <div class="field-group">
                            <label class="field-label">Fjalëkalimi</label>
                            <v-text-field
                                v-model="signInForm.password"
                                :rules="passwordRules"
                                placeholder="Shkruani fjalëkalimin"
                                variant="outlined"
                                density="comfortable"
                                prepend-inner-icon="mdi-lock-outline"
                                :append-inner-icon="showPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'"
                                :type="showPassword ? 'text' : 'password'"
                                @click:append-inner="showPassword = !showPassword"
                                hide-details="auto"
                                required
                                class="login-input"
                            ></v-text-field>
                        </div>
                    </div>

                    <div class="login-actions">
                        <v-btn
                            :disabled="!signInFormValid"
                            :loading="loading"
                            block
                            size="large"
                            type="submit"
                            color="#1e293b"
                            class="login-btn"
                        >
                            Hyr
                        </v-btn>
                    </div>
                </v-form>

                <div class="login-footer">
                    <button type="button" class="forgot-link" @click="forget = true">
                        Keni harruar fjalëkalimin?
                    </button>
                </div>

                <!-- Forgot Password Overlay -->
                <v-expand-transition>
                    <div v-if="forget" class="forgot-overlay">
                        <div class="login-card-header">
                            <v-img @click="switchToLanding" :src="logoSrc" max-width="60" class="cursor-pointer mx-auto mb-4"></v-img>
                            <h1 class="login-title">Rivendos Fjalëkalimin</h1>
                            <p class="login-subtitle">Shkruani email-in tuaj për të marrë linkun e rivendosjes</p>
                        </div>

                        <v-form v-model="forgetPasswordFormValid" @submit.prevent="submitForgetPasswordForm">
                            <div class="login-fields">
                                <div class="field-group">
                                    <label class="field-label">Email</label>
                                    <v-text-field
                                        v-model="forgetEmail"
                                        :rules="emailRules"
                                        placeholder="emri@email.com"
                                        variant="outlined"
                                        density="comfortable"
                                        prepend-inner-icon="mdi-email-outline"
                                        hide-details="auto"
                                        required
                                        class="login-input"
                                    ></v-text-field>
                                </div>
                            </div>

                            <div class="forgot-actions">
                                <v-btn
                                    variant="outlined"
                                    size="large"
                                    color="#64748b"
                                    class="login-btn flex-grow-1"
                                    @click="forget = false"
                                >
                                    Mbyll
                                </v-btn>
                                <v-btn
                                    :disabled="!forgetPasswordFormValid"
                                    size="large"
                                    color="#1e293b"
                                    class="login-btn flex-grow-1"
                                    type="submit"
                                >
                                    Dërgo
                                </v-btn>
                            </div>
                        </v-form>
                    </div>
                </v-expand-transition>
            </div>

            <p class="login-copyright">&copy; {{ new Date().getFullYear() }} Autoshkolla Linda</p>
        </div>
    </div>
</template>

<script setup>
import { detect } from 'detect-browser';
import { useRouter } from 'vue-router';
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { ref } from 'vue';
import { storeToRefs } from 'pinia';

const browser=detect()
const userStore=useUserStore()
const {loading}=storeToRefs(userStore)
const settingStore=useSettingStore()
const signInFormValid=ref(false)
const forgetPasswordFormValid=ref(false)
const forget=ref(false)
const showPassword=ref(false)
const passwordRules = [
    (v) => !!v || 'Fjalëkalimi është i detyrueshëm',
    (v) => (v && v.length >= 6) || 'Fjalëkalimi duhet të ketë më shumë se 6 karaktere',
]
const emailRules = [
    (v) => !!v || 'Email-i është i detyrueshëm',
    (v) => /.+@.+\..+/.test(v) || 'Email-i duhet të jetë i vlefshëm',
]
const router=useRouter()
const forgetEmail=ref('')
const signInForm=ref({
    email:'',
    password:''
})
const appInitialData=JSON.parse(localStorage.getItem('allSettings')) || {}

const logoSrc='/linda_logo.png'

const switchToLanding=()=>{
    // landing page removed; stay on sign-in
}

const submitForgetPasswordForm=()=>{
    if(appInitialData.defaultEmail=='' || appInitialData.password=='' || appInitialData.host=='' || appInitialData.port==null){
        settingStore.toggleSnackbar({status:true,msg:'Cilësimet e email-it nuk janë konfiguruar ende! Konfiguroni ato dhe pastaj dërgoni email nga këtu.'})
    }else{
        userStore.userInfoForForgetPassword(forgetEmail.value)
        .then((response)=>{
            if(response.status==200){
                const objEmail={
                    toEmail:forgetEmail.value,
                    logoPath:logoSrc.value,
                    siteUrl:window.location.origin,
                    siteTitle:appInitialData.siteTitle,
                    resetLink:window.location.origin+'/password-reset/'+response.data.forgetPasswordRef
                }
                settingStore.passwordEmailSent(objEmail)
                .then(response=>{
                    if(response.status==200){
                        settingStore.toggleSnackbar({status:true,msg:response.data.responseMsg})
                    }else{
                        settingStore.toggleSnackbar({status:true,msg:"Nuk mund të dërgohet email-i tani! Ju lutem provoni më vonë."})
                    }                  
                })
            }else if(response.status==202){
                settingStore.toggleSnackbar({status:true,msg:response.data.responseMsg})
            }
        })
    }
}

const submitSignInForm=()=>{
    login()
}

const login=()=>{
    userStore.userSignIn(signInForm.value)
    .then(response=>{
        if(response.status==200){
            userStore.getUserIp()
            .then(res=>{
                const objLogHistory={
                    userId:localStorage.getItem('userId'),
                    ip:res.data.ip,
                    browser:browser.name,
                    browserVersion:browser.version,
                    platform:browser.os                            
                }
                userStore.createHistory(objLogHistory)
                .then((res)=>{
                    localStorage.setItem('logCode',res.data.responseMsg)
                })
                userStore.setVisibility(true)
                settingStore.overlayToggle(true)
                router.push({name:'Dashboard'})
            })
        }else if(response.status==202){
            settingStore.toggleSnackbar({status:true,msg:response.data.responseMsg})
        }
    })
    .catch(error=>{
        const msg = error?.response?.data?.responseMsg
          || (error?.message === 'Network Error'
            ? 'Serveri nuk është i arritshëm. Ju lutem kontrolloni lidhjen tuaj.'
            : 'Hyrja dështoi. Ju lutem provoni përsëri.')
        settingStore.toggleSnackbar({ status: true, msg })
    })
}
</script>

<style scoped>
</style>
