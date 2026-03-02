<template>
    <div class="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6">
        <h4 class="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-6">Ndrysho Fjalëkalimin</h4>

        <form class="grid grid-cols-1 gap-5 lg:grid-cols-2" @submit.prevent="handleSubmit">
            <div class="">
                <label class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
                    Fjalëkalimi aktual
                </label>
                <div class="relative">
                    <input v-model="currentPassword" :type="showCurrentPassword ? 'text' : 'password'"
                        class="dark:bg-dark-900 h-11 w-full appearance-none rounded-lg border border-gray-300 bg-transparent px-4 py-2.5 pr-11 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                        placeholder="Shkruani fjalëkalimin aktual" />
                    <button type="button" @click="showCurrentPassword = !showCurrentPassword"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        :aria-label="showCurrentPassword ? 'Fsheh fjalëkalimin aktual' : 'Shfaq fjalëkalimin aktual'">
                        <EyeCloseIcon v-if="!showCurrentPassword" class="h-5 w-5" />
                        <EyeIcon v-else class="h-5 w-5" />
                    </button>
                </div>
            </div>

            <div>
                <label class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
                    Fjalëkalimi i ri
                </label>
                <div class="relative">
                    <input v-model="newPassword" :type="showNewPassword ? 'text' : 'password'"
                        class="dark:bg-dark-900 h-11 w-full appearance-none rounded-lg border border-gray-300 bg-transparent px-4 py-2.5 pr-11 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                        placeholder="Shkruani fjalëkalimin e ri" />
                    <button type="button" @click="showNewPassword = !showNewPassword"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        :aria-label="showNewPassword ? 'Fsheh fjalëkalimin e ri' : 'Shfaq fjalëkalimin e ri'">
                        <EyeCloseIcon v-if="!showNewPassword" class="h-5 w-5" />
                        <EyeIcon v-else class="h-5 w-5" />
                    </button>
                </div>
            </div>

            <div>
                <label class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
                    Konfirmo fjalëkalimin e ri
                </label>
                <div class="relative">
                    <input v-model="confirmPassword" :type="showConfirmPassword ? 'text' : 'password'"
                        class="dark:bg-dark-900 h-11 w-full appearance-none rounded-lg border border-gray-300 bg-transparent px-4 py-2.5 pr-11 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                        placeholder="Rishkruani fjalëkalimin" />
                    <button type="button" @click="showConfirmPassword = !showConfirmPassword"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        :aria-label="showConfirmPassword ? 'Fsheh konfirmimin e fjalëkalimit' : 'Shfaq konfirmimin e fjalëkalimit'">
                        <EyeCloseIcon v-if="!showConfirmPassword" class="h-5 w-5" />
                        <EyeIcon v-else class="h-5 w-5" />
                    </button>
                </div>
            </div>

            <div v-if="errorMessage" class="lg:col-span-2">
                <Alert variant="error" title="Gabim" :message="errorMessage" />
            </div>

            <div v-if="successMessage" class="lg:col-span-2">
                <Alert variant="success" title="Sukses" :message="successMessage" />
            </div>

            <div class="lg:col-span-2 flex justify-end">
                <button type="submit"
                    class="inline-flex justify-center rounded-lg bg-brand-500 px-4 py-2.5 text-sm font-medium text-white hover:bg-brand-600">
                    Ruaj fjalëkalimin
                </button>
            </div>
        </form>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import Alert from '@/components/ui/Alert.vue'
import { EyeIcon, EyeCloseIcon } from '@/icons'

const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const handleSubmit = () => {
    errorMessage.value = ''
    successMessage.value = ''

    if (!currentPassword.value.trim() || !newPassword.value.trim() || !confirmPassword.value.trim()) {
        errorMessage.value = 'Ju lutem plotësoni të gjitha fushat e fjalëkalimit.'
        return
    }

    if (newPassword.value.length < 6) {
        errorMessage.value = 'Fjalëkalimi i ri duhet të ketë të paktën 6 karaktere.'
        return
    }

    if (newPassword.value !== confirmPassword.value) {
        errorMessage.value = 'Fjalëkalimi i ri dhe konfirmimi nuk përputhen.'
        return
    }

    successMessage.value = 'Fjalëkalimi u ndryshua me sukses.'

    currentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
}
</script>
