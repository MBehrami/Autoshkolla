<template>
    <div class="login-page">
        <v-card elevation="10" rounded="shaped" min-width="380" min-height="450">
            <v-card-title class="text-center">
                <div class="d-flex justify-center"><v-img @click="switchToLanding" :src="logoSrc" max-height="50" max-width="70" class="cursor-pointer"></v-img></div>                
                <div class="text-h6 font-weight-bold">{{ appInitialData.siteTitle }}</div>
                <div class="text-subtitle-2 text-grey-darken-1">{{ appInitialData.welComeMessage }}</div>
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
                    label="Password"
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
                    text="Sign In"
                    type="submit"
                    color="grey-darken-3"
                    class="text-capitalize mt-4"
                    >
                    </v-btn>
                    <v-btn
                    @click="toggleRegistration"
                    block
                    rounded
                    text="Register"
                    class="text-capitalize"
                    color="grey-lighten-1 mt-1"
                    >
                    </v-btn>
                </v-form>
            </v-card-text>
            <v-card-actions>
                <v-row justify="center">
                    <v-btn @click="forget=true" variant="text" text="Forget Password" class="text-capitalize"></v-btn>
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
                        <div class="text-h6 font-weight-bold">Forget Password</div>
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
                            text="Close"
                            color="grey-darken-3"
                            class="text-capitalize"
                            @click="forget=false"
                            >
                            </v-btn>
                            <v-spacer></v-spacer>
                            <v-btn
                            :disabled="!forgetPasswordFormValid"
                            variant="text"
                            text="Send Password"
                            type="submit"
                            color="grey-darken-3"
                            class="text-capitalize"
                            >
                            </v-btn>
                        </v-card-actions>
                    </v-form>
                </v-card>
            </v-expand-transition>

            <v-expand-transition>
                <v-card 
                v-if="register"
                class="position-absolute w-100"
                height="100%"
                style="bottom: 0"
                >
                    <v-card-title class="text-center">
                        <div class="d-flex justify-center"><v-img @click="switchToLanding" :src="logoSrc" max-height="50" max-width="70" class="cursor-pointer"></v-img></div>                
                        <div class="text-h6 font-weight-bold">Register</div>
                    </v-card-title>
                    <v-form v-model="registerFormValid" @submit.prevent="submitRegisterForm">
                        <v-card-text>                           
                            <v-text-field
                            v-model="registerForm.fullName"
                            :rules="passwordRules"
                            label="Name"
                            variant="underlined"
                            append-inner-icon="mdi-account"
                            required
                            >
                            </v-text-field>
                            <v-text-field
                            v-model="registerForm.email"
                            :rules="emailRules"
                            label="Email"
                            variant="underlined"
                            append-inner-icon="mdi-email"
                            required
                            >
                            </v-text-field>
                            <v-text-field
                            v-model="registerForm.password"
                            :rules="passwordRules"
                            label="Password"
                            variant="underlined"
                            append-inner-icon="mdi-lock"
                            type="password"
                            required
                            >
                            </v-text-field>                                                                          
                        </v-card-text>                           
                        <v-card-actions class="pt-0">
                            <v-btn
                            variant="text"
                            text="Close"
                            color="grey-darken-3"
                            class="text-capitalize"
                            @click="toggleRegistration"
                            >
                            </v-btn>
                            <v-spacer></v-spacer>
                            <v-btn
                            :disabled="!registerFormValid"
                            :loading="loading"
                            variant="text"
                            text="Register"
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
import logo from '@/assets/vue-admin-logo.png'
import { computed } from 'vue';
import { storeToRefs } from 'pinia';

const browser=detect()
const userStore=useUserStore()
const {loading}=storeToRefs(userStore)
const settingStore=useSettingStore()
const signInFormValid=ref(false)
const forgetPasswordFormValid=ref(false)
const registerFormValid=ref(false)
const forget=ref(false)
//const register=ref(false)
const passwordRules = [
    (v) => !!v || 'Password is required',
    (v) => (v && v.length >= 6) || 'Password must be more than 6 characters',
]
const emailRules = [
    (v) => !!v || 'E-mail is required',
    (v) => /.+@.+\..+/.test(v) || 'E-mail must be valid',
]
const router=useRouter()
const forgetEmail=ref('')
const signInForm=ref({
    email:'',
    password:''
})
const registerForm=ref({
    fullName:'',
    email:'',
    password:''
})
const appInitialData=JSON.parse(localStorage.getItem('allSettings'))

//get logo
const logoSrc=computed(()=>{
    return appInitialData.logoPath==''? logo :import.meta.env.VITE_API_URL+appInitialData.logoPath
})

//Switch to Landing
const switchToLanding=()=>{
    router.push({name:'Landing'})
}

//registration window open
const toggleRegistration=()=>{
    userStore.setRegistration()
}

//registration window open status
const register=computed(()=>{
    return userStore.registrationOpen
})

//registration submit
const submitRegisterForm=()=>{
    userStore.createRegistration(registerForm.value)
    .then((response)=>{
        if(response.status==200){
            if(appInitialData.allowWelcomeEmail==true){                           
                const objEmail={
                    toEmail:registerForm.value.email,
                    name:registerForm.value.fullName,
                    logoPath:logoSrc.value,
                    siteUrl:window.location.origin,
                    siteTitle:appInitialData.siteTitle,
                    password:registerForm.value.password,
                }                           
                settingStore.welcomeEmailSent(objEmail)
            }
            signInForm.value={
                email:registerForm.value.email,
                password:registerForm.value.password
            }
            login()           
        }else if(response.status==202){
            settingStore.toggleSnackbar({status:true,msg:response.data.responseMsg})
        }
    })
    .catch(error=>{
        console.log('error',error)
    })
}

//password forget submit
const submitForgetPasswordForm=()=>{
    if(appInitialData.defaultEmail=='' || appInitialData.password=='' || appInitialData.host=='' || appInitialData.port==null){
        settingStore.toggleSnackbar({status:true,msg:'Email settings not done yet! Do that then send email from here.'})
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
                        settingStore.toggleSnackbar({status:true,msg:"Can't send email now! please try later."})
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
        console.log('error',error)
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