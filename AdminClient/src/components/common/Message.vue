<template>
    <v-snackbar v-model="snackbar" timeout="5000">
        {{ snackbarMsg }}
        <template v-slot:actions>
            <v-btn color="pink" variant="text" text="Close" @click="reset">
            </v-btn>
        </template>
    </v-snackbar>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';

const resetObj = ref({
    status: false,
    msg: ''
})
const settingStore = useSettingStore()
const { snackbar, snackbarMsg } = storeToRefs(settingStore)
const reset = () => {
    settingStore.toggleSnackbar(resetObj.value)
}
</script>