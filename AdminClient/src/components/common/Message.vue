<template>
    <v-snackbar v-model="snackbar" timeout="5000" location="bottom" rounded="lg">
        <div class="d-flex align-center ga-2">
            <v-icon v-if="isSuccess" icon="mdi-check-circle" color="success" size="20"></v-icon>
            <v-icon v-else icon="mdi-information" size="20"></v-icon>
            <span>{{ snackbarMsg }}</span>
        </div>
        <template v-slot:actions>
            <v-btn color="pink-lighten-2" variant="text" size="small" class="text-none" @click="reset">
                Mbyll
            </v-btn>
        </template>
    </v-snackbar>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed } from 'vue';

const resetObj = ref({
    status: false,
    msg: ''
})
const settingStore = useSettingStore()
const { snackbar, snackbarMsg } = storeToRefs(settingStore)

const isSuccess = computed(() => {
    const msg = (snackbarMsg.value || '').toLowerCase()
    return msg.includes('success') || msg.includes('sukses') || msg.includes('saved') || msg.includes('ruajt') || msg.includes('created') || msg.includes('updated') || msg.includes('deleted') || msg.includes('regjistrua')
})

const reset = () => {
    settingStore.toggleSnackbar(resetObj.value)
}
</script>