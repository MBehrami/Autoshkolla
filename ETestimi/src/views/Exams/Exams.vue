<template>
  <admin-layout>
    <div class="space-y-6">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <h2 class="text-xl font-semibold text-gray-800 dark:text-white/90">Testet</h2>
        <router-link
          v-if="selectedCategory"
          to="/exams"
          class="inline-flex items-center justify-center rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/5"
        >
          Kthehu te Kategoritë
        </router-link>
      </div>

      <div v-if="errorMessage" class="rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/20 dark:text-red-400">
        {{ errorMessage }}
      </div>

      <CategoryAccessDeniedModal
        v-if="showCategoryAccessDenied"
        :message="accessDeniedMessage"
        @close="closeCategoryAccessDeniedModal"
      />

      <component-card title="Zgjidh Kategorinë" desc="Zgjidh një kategori patente për të parë testet e disponueshme.">
        <div v-if="loadingCategories" class="py-8 text-center text-gray-500 dark:text-gray-400">Duke u ngarkuar...</div>
        <div v-else class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <router-link
            v-for="category in examCategories"
            :key="category.id"
            :to="`/exams/${category.id}`"
            class="rounded-xl border border-gray-200 p-4 transition hover:border-brand-300 hover:bg-gray-50 dark:border-gray-800 dark:hover:border-brand-700 dark:hover:bg-white/5"
            :class="{
              'border-brand-500 bg-brand-50/40 dark:border-brand-700 dark:bg-brand-500/10':
                selectedCategory?.id === category.id,
            }"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-sm text-gray-500 dark:text-gray-400">{{ category.title }}</p>
                <h3 class="mt-1 text-lg font-semibold text-gray-800 dark:text-white/90">{{ category.id }}</h3>
              </div>
              <div class="flex h-8 w-8 shrink-0 items-center justify-center">
                <component :is="categoryIcons[category.id]" class="h-full w-full" />
              </div>
            </div>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-300">{{ category.description }}</p>
          </router-link>
        </div>
      </component-card>

      <component-card
        v-if="selectedCategory"
        :title="`Teste për ${selectedCategory.title}`"
        desc="Zgjidh një test për të nisur vetë-vlerësimin."
      >
        <div v-if="loadingExams" class="py-8 text-center text-gray-500 dark:text-gray-400">Duke u ngarkuar...</div>
        <div v-else class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5 xl:grid-cols-6">
          <router-link
            v-for="exam in exams"
            :key="exam.id"
            :to="`/exams/${selectedCategory.id}/${exam.id}`"
            class="rounded-lg border border-gray-200 p-4 text-center text-sm font-medium text-gray-700 transition hover:border-brand-300 hover:bg-gray-50 dark:border-gray-800 dark:text-gray-300 dark:hover:border-brand-700 dark:hover:bg-white/5"
          >
            {{ exam.title }}
          </router-link>
        </div>
      </component-card>
    </div>
  </admin-layout>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import ComponentCard from '@/components/common/ComponentCard.vue'
import CategoryAccessDeniedModal from '@/components/common/CategoryAccessDeniedModal.vue'
import { MotorcycleIcon, CarIcon, BusIcon, TruckIcon } from '@/icons'
import { useExamStore } from '@/stores/exam'

const categoryIcons: Record<string, any> = {
  A: MotorcycleIcon,
  B: CarIcon,
  C: TruckIcon,
  D: BusIcon,
}

const route = useRoute()
const examStore = useExamStore()
const loadingCategories = ref(false)
const loadingExams = ref(false)
const showCategoryAccessDenied = ref(false)

const examCategories = computed(() => examStore.categories)
const errorMessage = computed(() => (examStore.categoryAccessDeniedMessage ? '' : examStore.error || ''))
const accessDeniedMessage = computed(() => examStore.categoryAccessDeniedMessage || 'Nuk keni akses në këtë kategori, kontaktoni administratorin.')

const selectedCategoryId = computed(() => {
  const value = route.params.category
  if (typeof value !== 'string') {
    return null
  }
  return value.toUpperCase()
})

const selectedCategory = computed(() => {
  if (!selectedCategoryId.value) {
    return null
  }
  return examCategories.value.find(c => c.id === selectedCategoryId.value) ?? null
})

const exams = computed(() => {
  if (!selectedCategoryId.value) {
    return []
  }
  return examStore.exams[selectedCategoryId.value] || []
})

onMounted(async () => {
  loadingCategories.value = true
  examStore.clearError()
  examStore.clearCategoryAccessDenied()
  try {
    await examStore.fetchCategories()
  } catch (error) {
    console.error('Deshtoi ngarkimi i kategorive:', error)
  } finally {
    loadingCategories.value = false
  }
})

watch(selectedCategoryId, async (newCode) => {
  if (!newCode) {
    return
  }
  
  loadingExams.value = true
  examStore.clearError()
  examStore.clearCategoryAccessDenied()
  try {
    await examStore.fetchExamsByCategory(newCode)
  } catch (error) {
    if (examStore.categoryAccessDeniedMessage) {
      showCategoryAccessDenied.value = true
    }
    console.error('Deshtoi ngarkimi i testeve:', error)
  } finally {
    loadingExams.value = false
  }
}, { immediate: true })

watch(
  () => examStore.categoryAccessDeniedMessage,
  (message) => {
    if (message) {
      showCategoryAccessDenied.value = true
    }
  }
)

const closeCategoryAccessDeniedModal = () => {
  showCategoryAccessDenied.value = false
  examStore.clearCategoryAccessDenied()
}
</script>
