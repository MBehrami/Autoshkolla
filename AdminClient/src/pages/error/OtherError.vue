<template>
    <div class="mx-10 my-10">
        <div class="text-h2">{{ errorHeading }}</div>
        <div class="text-subtitle-2 text-grey-darken-1 pt-1">{{ errorDetail }}</div>
        <div class="pt-12">Try going back to sign in page by clicking <router-link to="/">here</router-link></div>.
    </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import { useUserStore } from '@/store/UserStore';

const errorCode = ref(localStorage.getItem('http_error'))
const userStore = useUserStore()

//hide Navbar and Footer
userStore.setVisibility(false)

//get error heading
const errorHeading = computed(() => {
    if (errorCode.value == '401') {
        return 'Token Expired'
    } else if (errorCode.value == '403') {
        return 'Limited Access'
    } else if (errorCode.value == '0') {
        return 'No Internet'
    } else {
        return 'Unknown Error'
    }
})

//get error detail
const errorDetail = computed(() => {
    if (errorCode.value == '401') {
        return 'Your current token has expired.Sign in again to get a new one.'
    } else if (errorCode.value == '403') {
        return 'You have limited access to this resource.Contact your admin to get permission here.'
    } else if (errorCode.value == '0') {
        return 'Looks like your internet connectivity is down.Please wait until get that back.'
    } else {
        return 'We have no clue about this error.Please start a new journey by sign in again.'
    }
})

</script>