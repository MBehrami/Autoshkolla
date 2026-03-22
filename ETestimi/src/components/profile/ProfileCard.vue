<template>
  <div>
    <div class="p-5 mb-6 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6">
      <div class="flex flex-col gap-5 xl:flex-row xl:items-center">
        <div class="flex flex-col items-center w-full gap-6 xl:flex-row">
          <div
            class="flex w-20 h-20 items-center justify-center overflow-hidden border border-gray-200 rounded-full dark:border-gray-800"
          >
            <UserCircleIcon class="w-12 h-12 text-gray-500 dark:text-gray-400" />
          </div>
          <div class="order-3 xl:order-2">
            <h4
              class="mb-2 text-lg font-semibold text-center text-gray-800 dark:text-white/90 xl:text-left"
            >
              {{ authStore.fullName || 'Kandidat' }}
            </h4>
            <div
              class="flex flex-col items-center gap-1 text-center xl:flex-row xl:gap-3 xl:text-left"
            >
              <p class="text-sm text-gray-500 dark:text-gray-400">{{ authStore.phoneNumber || 'N/A' }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { UserCircleIcon } from '@/icons'

const authStore = useAuthStore()
</script>
