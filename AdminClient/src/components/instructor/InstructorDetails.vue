<template>
    <v-card>
        <v-card-title class="d-flex justify-space-between align-center">
            <span>Instructor Details</span>
            <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4" v-if="loading">
            <v-progress-linear indeterminate></v-progress-linear>
        </v-card-text>
        <v-card-text class="pa-4" v-else-if="instructor">
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.firstName" label="First Name" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.parentName" label="Parent Name" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.lastName" label="Last Name" variant="outlined" readonly></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.dateOfBirth" label="Date of Birth" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.personalNumber" label="Personal Number" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.phoneNumber" label="Phone" variant="outlined" readonly></v-text-field>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.email" label="Email" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.scheduleType" label="Schedule Type" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-chip :color="instructor.isActive ? 'success' : 'default'" size="small">{{ instructor.isActive ? 'Active' : 'Inactive' }}</v-chip>
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.licenseNumber" label="License Number" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                    <v-text-field :model-value="instructor.licenseValidityDate" label="License Validity Date" variant="outlined" readonly></v-text-field>
                </v-col>
                <v-col cols="12" md="4" v-if="instructor.licensePhotoPath">
                    <span class="text-caption">License Photo</span>
                    <v-img :src="licensePhotoUrl" max-height="120" contain class="mt-1"></v-img>
                </v-col>
            </v-row>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-4">
            <v-spacer></v-spacer>
            <v-btn text="Close" color="primary" class="text-capitalize" @click="closeDialog"></v-btn>
        </v-card-actions>
    </v-card>
</template>

<script setup>
import { useInstructorStore } from '@/store/InstructorStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, watch, onMounted } from 'vue';

const props = defineProps({
    instructorId: { type: Number, required: true }
})

const emit = defineEmits(['close'])
const instructorStore = useInstructorStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(instructorStore)
const instructor = ref(null)

const licensePhotoUrl = computed(() => {
    const p = instructor.value?.licensePhotoPath
    if (!p) return ''
    return p.startsWith('http') ? p : (import.meta.env.VITE_API_URL || '') + (p.startsWith('/') ? '' : '/') + p
})

const loadDetails = () => {
    instructorStore.getInstructorDetails(props.instructorId)
        .then((response) => {
            instructor.value = response.data
        })
        .catch(() => {
            settingStore.toggleSnackbar({ status: true, msg: 'Error loading instructor' })
        })
}

const closeDialog = () => emit('close')

watch(() => props.instructorId, () => { if (props.instructorId) loadDetails() }, { immediate: true })
onMounted(() => { if (props.instructorId) loadDetails() })
</script>
