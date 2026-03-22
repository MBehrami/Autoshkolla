<template>
  <FullScreenLayout>
    <div class="relative p-6 bg-white z-1 dark:bg-gray-900 sm:p-0">
      <div
        class="relative flex flex-col justify-center w-full h-screen lg:flex-row dark:bg-gray-900"
      >
        <div class="flex flex-col flex-1 w-full lg:w-1/2">
          <div class="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
            <div>
              <div class="mb-5 sm:mb-8">
                <h1
                  class="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md"
                >
                  Hyrje
                </h1>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  Shkruani numrin e telefonit dhe fjalëkalimin për t'u kyçur!
                </p>
              </div>
              <div>
                <form @submit.prevent="handleSubmit">
                  <div class="space-y-5">
                    <!-- Phone -->
                    <div>
                      <label
                        for="phone"
                        class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400"
                      >
                        Numri i telefonit<span class="text-error-500">*</span>
                      </label>
                      <input
                        v-model="phoneNumber"
                        type="tel"
                        id="phone"
                        name="phone"
                        placeholder="P.sh. 044123123"
                        :class="phoneError ? 'border-error-500 focus:border-error-500 focus:ring-error-500/10' : 'border-gray-300 focus:border-brand-300 focus:ring-brand-500/10'"
                        class="dark:bg-dark-900 h-11 w-full rounded-lg border bg-transparent px-4 py-2.5 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                        @blur="validatePhone"
                        @input="clearPhoneError"
                      />
                      <p v-if="phoneError" class="mt-1 text-xs text-error-500">{{ phoneError }}</p>
                    </div>
                    <!-- Password -->
                    <div>
                      <label
                        for="password"
                        class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400"
                      >
                        Fjalëkalimi<span class="text-error-500">*</span>
                      </label>
                      <div class="relative">
                        <input
                          v-model="password"
                          :type="showPassword ? 'text' : 'password'"
                          id="password"
                          placeholder="Shkruani fjalëkalimin"
                          :class="passwordError ? 'border-error-500 focus:border-error-500 focus:ring-error-500/10' : 'border-gray-300 focus:border-brand-300 focus:ring-brand-500/10'"
                          class="dark:bg-dark-900 h-11 w-full rounded-lg border bg-transparent py-2.5 pl-4 pr-11 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                          @blur="validatePassword"
                          @input="clearPasswordError"
                        />
                        <button
                          type="button"
                          @click="togglePasswordVisibility"
                          class="absolute z-30 text-gray-500 -translate-y-1/2 cursor-pointer right-4 top-1/2 dark:text-gray-400"
                          :aria-label="showPassword ? 'Fsheh fjalëkalimin' : 'Shfaq fjalëkalimin'"
                        >
                          <EyeCloseIcon v-if="!showPassword" class="h-5 w-5" />
                          <EyeIcon v-else class="h-5 w-5" />
                        </button>
                      </div>
                      <p v-if="passwordError" class="mt-1 text-xs text-error-500">{{ passwordError }}</p>
                    </div>
                    <!-- Checkbox -->
                    <div class="flex items-center justify-between">
                      <div>
                        <label
                          for="keepLoggedIn"
                          class="flex items-center text-sm font-normal text-gray-700 cursor-pointer select-none dark:text-gray-400"
                        >
                          <div class="relative">
                            <input
                              v-model="keepLoggedIn"
                              type="checkbox"
                              id="keepLoggedIn"
                              class="sr-only"
                            />
                            <div
                              :class="
                                keepLoggedIn
                                  ? 'border-brand-500 bg-brand-500'
                                  : 'bg-transparent border-gray-300 dark:border-gray-700'
                              "
                              class="mr-3 flex h-5 w-5 items-center justify-center rounded-md border-[1.25px]"
                            >
                              <span :class="keepLoggedIn ? '' : 'opacity-0'">
                                <svg
                                  width="14"
                                  height="14"
                                  viewBox="0 0 14 14"
                                  fill="none"
                                  xmlns="http://www.w3.org/2000/svg"
                                >
                                  <path
                                    d="M11.6666 3.5L5.24992 9.91667L2.33325 7"
                                    stroke="white"
                                    stroke-width="1.94437"
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                  />
                                </svg>
                              </span>
                            </div>
                          </div>
                          Më mbaj të kyçur
                        </label>
                      </div>
                      <button
                        type="button"
                        @click="openHelpModal"
                        class="text-sm text-brand-500 hover:text-brand-600 dark:text-brand-400"
                      >
                        Keni nevojë për ndihmë?
                      </button>
                    </div>
                    <!-- Button -->
                    <div>
                      <button
                        type="submit"
                        :disabled="isSubmitting"
                        class="flex items-center justify-center w-full px-4 py-3 text-sm font-medium text-white transition rounded-lg bg-brand-500 shadow-theme-xs hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-70"
                      >
                        {{ isSubmitting ? 'Duke u kyçur...' : 'Kyçu' }}
                      </button>
                    </div>
                    <Alert
                      v-if="formError"
                      variant="error"
                      title="Kyçja dështoi"
                      :message="formError"
                    />
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
        <div
          class="relative hidden w-full h-full lg:w-1/2 lg:block overflow-hidden"
        >
          <img
            src="/images/logo/linda_carss.png"
            alt="Auto Shkolla Linda"
            class="absolute inset-0 w-full h-full object-cover"
          />
        </div>
      </div>
    </div>

    <Modal v-if="isHelpModalOpen" :full-screen-backdrop="true" @close="closeHelpModal">
      <template #body>
        <div class="relative w-full max-w-md rounded-2xl bg-white p-6 dark:bg-gray-900">
          <button
            type="button"
            class="absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded-full text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-white/5 dark:hover:text-gray-300"
            @click="closeHelpModal"
          >
            <span class="text-lg">×</span>
          </button>

          <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">Keni nevojë për ndihmë?</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Kontaktoni administratorin e sistemit me të dhënat më poshtë.
          </p>

          <div class="mt-5 space-y-3">
            <div class="rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-sm text-gray-600 dark:border-gray-800 dark:bg-white/5 dark:text-gray-300">
              Email: admin@system.local
            </div>

            <div class="rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-sm text-gray-600 dark:border-gray-800 dark:bg-white/5 dark:text-gray-300">
              Telefoni: 044123456
            </div>

            <div class="flex items-center justify-end pt-1">
              <button
                type="button"
                class="rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-white/5"
                @click="closeHelpModal"
              >
                Mbyll
              </button>
            </div>
          </div>
        </div>
      </template>
    </Modal>
  </FullScreenLayout>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import CommonGridShape from '@/components/common/CommonGridShape.vue'
import FullScreenLayout from '@/components/layout/FullScreenLayout.vue'
import Modal from '@/components/ui/Modal.vue'
import Alert from '@/components/ui/Alert.vue'
import { EyeIcon, EyeCloseIcon } from '@/icons'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const phoneNumber = ref('')
const password = ref('')
const showPassword = ref(false)
const keepLoggedIn = ref(false)
const isHelpModalOpen = ref(false)
const phoneError = ref('')
const passwordError = ref('')
const router = useRouter()

const isSubmitting = computed(() => authStore.isLoading)
const formError = computed(() => authStore.error || '')

const togglePasswordVisibility = () => {
  showPassword.value = !showPassword.value
}

const validatePhone = () => {
  phoneError.value = ''
  
  if (!phoneNumber.value?.trim()) {
    phoneError.value = 'Numri i telefonit është i detyrueshëm.'
    return false
  }
  
  // Basic phone validation (allow various formats)
  const phoneRegex = /^[\d\s+()-]{8,20}$/
  if (!phoneRegex.test(phoneNumber.value.trim())) {
    phoneError.value = 'Formati i numrit të telefonit është i pasaktë.'
    return false
  }
  
  return true
}

const validatePassword = () => {
  passwordError.value = ''
  
  if (!password.value) {
    passwordError.value = 'Fjalëkalimi është i detyrueshëm.'
    return false
  }
  
  if (password.value.length < 4) {
    passwordError.value = 'Fjalëkalimi duhet të ketë të paktën 4 karaktere.'
    return false
  }
  
  return true
}

const clearPhoneError = () => {
  if (phoneError.value) phoneError.value = ''
}

const clearPasswordError = () => {
  if (passwordError.value) passwordError.value = ''
}

const handleSubmit = async () => {
  authStore.clearError()
  
  // Validate both fields
  const isPhoneValid = validatePhone()
  const isPasswordValid = validatePassword()
  
  if (!isPhoneValid || !isPasswordValid) {
    return
  }

  try {
    await authStore.signIn(phoneNumber.value.trim(), password.value)
    router.push('/exams')
  } catch (error: any) {
    // Error is already in authStore.error, just handle display
    console.error('Gabim gjatë kyçjes:', error)
  }
}

const openHelpModal = () => {
  isHelpModalOpen.value = true
}

const closeHelpModal = () => {
  isHelpModalOpen.value = false
}
</script>
