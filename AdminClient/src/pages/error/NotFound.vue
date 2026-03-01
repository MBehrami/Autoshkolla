<template>
    <div class="mx-10 my-10">
        <div class="text-h2">Faqja nuk u gjet</div>
        <div class="text-subtitle-2 text-grey-darken-1 pt-1">Faqja që ju kërkuat nuk ekziston ose është fshirë. Ju lutemi kontrolloni URL-në dhe provoni përsëri.</div>
        <div class="pt-12">Provoni të ktheheni prapa duke klikuar <a href="#" @click.prevent="goBack">këtu</a></div>.
    </div>
</template>

<script setup>
import { ref, onUnmounted } from 'vue';
import { useSettingStore } from '@/store/SettingStore';
import { useUserStore } from '@/store/UserStore';
import { useRoute, useRouter } from 'vue-router'

const settingStore = useSettingStore()
const userStore = useUserStore()
const route = useRoute()
const router = useRouter()

const errorForm = ref({
    status: 404,
    statusText: 'NotFound',
    url: route.fullPath,
    message: 'Page not exists',
    addedBy: localStorage.getItem('userId') || 0
})

//hide Navbar and Footer
userStore.setVisibility(false)

//save error
settingStore.createErrorLog(errorForm.value)

//Reset visibility when leaving this page
onUnmounted(() => {
    userStore.setVisibility(true)
})

//Go back to previous page
const goBack = () => {
    router.back()
}
</script>