<template>
    <div class="login-page">
        <v-card elevation="10" rounded="shaped" min-width="380" min-height="450">
            <v-card-title class="text-center">
                <div class="d-flex justify-center my-5"><v-img @click="switchToLanding" :src="logoSrc" max-width="150" class="cursor-pointer"></v-img></div>                
                <!-- <div class="text-h6 font-weight-bold">{{ appInitialData.siteTitle }}</div> -->
                <div class="text-subtitle-2 text-grey-darken-1">Sistemi i menaxhimit</div>
            </v-card-title>
            <v-card-text>
                <v-form v-model="signInFormValid" @submit.prevent="submitSignInForm">
                    <v-text-field
                    v-model="signInForm.email"
                    :rules="emailRules"
                    label="Email"
                    variant="underlined"
                    append-inner-icon="mdi-email"
                    required
                    >
                    </v-text-field>
                    <v-text-field
                    v-model="signInForm.password"
                    :rules="passwordRules"
                    label="Fjalëkalimi"
                    variant="underlined"
                    append-inner-icon="mdi-lock"
                    type="password"
                    required
                    >
                    </v-text-field>
                    <v-btn
                    :disabled="!signInFormValid"
                    :loading="loading"
                    block
                    rounded
                    text="Hyr"
                    type="submit"
                    color="grey-darken-3"
                    class="text-capitalize mt-4"
                    >
                    </v-btn>
                </v-form>
            </v-card-text>
            <v-card-actions>
                <v-row justify="center">
                    <v-btn @click="forget=true" variant="text" text="Keni harruar fjalëkalimin?" class="text-capitalize"></v-btn>
                </v-row>               
            </v-card-actions>

            <v-expand-transition>
                <v-card 
                v-if="forget"
                class="position-absolute w-100"
                height="100%"
                style="bottom: 0"
                >
                    <v-card-title class="text-center">
                        <div class="d-flex justify-center"><v-img @click="switchToLanding" :src="logoSrc" max-height="50" max-width="70" class="cursor-pointer"></v-img></div>                
                        <div class="text-h6 font-weight-bold">Rivendos Fjalëkalimin</div>
                    </v-card-title>
                    <v-form v-model="forgetPasswordFormValid" @submit.prevent="submitForgetPasswordForm">
                        <v-card-text>
                            <v-text-field
                            v-model="forgetEmail"
                            :rules="emailRules"
                            label="Email"
                            variant="underlined"
                            append-inner-icon="mdi-email"
                            required
                            >
                            </v-text-field>                                                
                        </v-card-text>
                        <v-card-actions class="pt-0">
                            <v-btn
                            variant="text"
                            text="Mbyll"
                            color="grey-darken-3"
                            class="text-capitalize"
                            @click="forget=false"
                            >
                            </v-btn>
                            <v-spacer></v-spacer>
                            <v-btn
                            :disabled="!forgetPasswordFormValid"
                            variant="text"
                            text="Dërgo Fjalëkalimin"
                            type="submit"
                            color="grey-darken-3"
                            class="text-capitalize"
                            >
                            </v-btn>
                        </v-card-actions>
                    </v-form>
                </v-card>
            </v-expand-transition>
        </v-card>
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

//get logo
const logoSrc='/linda_logo.png'

//Logo click — already on sign-in, nothing to do
const switchToLanding=()=>{
    // landing page removed; stay on sign-in
}

//password forget submit
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

//submit sign in
const submitSignInForm=()=>{
    login()
}

//sign in
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
.login-page{
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
}
</style>