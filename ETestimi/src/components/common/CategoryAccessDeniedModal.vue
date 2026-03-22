<template>
  <Modal :full-screen-backdrop="true" @close="emit('close')">
    <template #body>
      <div class="relative w-full max-w-md rounded-2xl bg-white p-6 dark:bg-gray-900">
        <button
          type="button"
          class="absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded-full text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-white/5 dark:hover:text-gray-300"
          @click="emit('close')"
        >
          <span class="text-lg">x</span>
        </button>

        <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">Njoftim</h3>
        <p class="mt-3 text-sm text-gray-600 dark:text-gray-300">
          {{ message }}
        </p>

        <div class="mt-5 flex items-center justify-end">
          <button
            type="button"
            class="rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-white/5"
            @click="emit('close')"
          >
            Mbyll
          </button>
        </div>
      </div>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import Modal from '@/components/ui/Modal.vue'

interface Props {
  message: string
}

defineProps<Props>()
const emit = defineEmits<{ (e: 'close'): void }>()
</script>
