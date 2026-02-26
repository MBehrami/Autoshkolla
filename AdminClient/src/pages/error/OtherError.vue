<template>
    <div class="mx-10 my-10">
        <div class="text-h2">{{ errorHeading }}</div>
        <div class="text-subtitle-2 text-grey-darken-1 pt-1">{{ errorDetail }}</div>
        <div class="pt-12">Provoni të kaloni në faqen e hyrjes duke klikuar <router-link to="/">këtu</router-link></div>.
    </div>
</template>

<script setup>
import { computed, ref, onUnmounted } from 'vue';
import { useUserStore } from '@/store/UserStore';

const errorCode = ref(localStorage.getItem('http_error'))
const userStore = useUserStore()

//hide Navbar and Footer
userStore.setVisibility(false)

//Reset visibility when leaving this page
onUnmounted(() => {
    userStore.setVisibility(true)
})

//get error heading
const errorHeading = computed(() => {
    if (errorCode.value == '401') {
        return 'Token ka skaduar'
    } else if (errorCode.value == '403') {
        return 'Qasje e kufizuar'
    } else if (errorCode.value == '0') {
        return 'Nuk ka internet'
    } else {
        return 'Gabim i panjohur'
    }
})

//get error detail
const errorDetail = computed(() => {
    if (errorCode.value == '401') {
        return 'Token juaj aktual ka skaduar. Hyni përsëri për të marrë atë të ri.'
    } else if (errorCode.value == '403') {
        return 'Keni qasje të kufizuar në këtë burim. Kontaktoni administratorin tuaj për të marrë leje këtu.'
    } else if (errorCode.value == '0') {
        return 'Duket se lidhja juaj me internetin është e ndërprerë. Ju lutemi prisni derisa të kthehet.'
    } else {
        return 'Ne s\'ka idenë për këtë gabim. Ju lutemi filloni një udhëtim të ri duke hyrë përsëri.'
    }
})

</script>