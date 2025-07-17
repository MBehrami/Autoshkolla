<template>
    <div class="mx-10 my-10">
        <div class="text-h2">Page not found</div>
        <div class="text-subtitle-2 text-grey-darken-1 pt-1">The page you are trying to get to never existed in this
            reality,or has migrated to a parallel universe.</div>
        <div class="pt-12">Try going back to sign in page by clicking <router-link to="/">here</router-link></div>.
    </div>
</template>

<script setup>
import { ref } from 'vue';
import { useSettingStore } from '@/store/SettingStore';
import { useUserStore } from '@/store/UserStore';
import { useRoute } from 'vue-router'

const settingStore = useSettingStore()
const userStore = useUserStore()
const route = useRoute()
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
</script>