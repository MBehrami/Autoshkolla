<template>
  <admin-layout>
    <div class="grid grid-cols-12 gap-4 md:gap-6">
      <div class="col-span-12 xl:col-span-8">
        <component-card
          title="Parapamje e Analitikës së Testeve"
          desc="Një pasqyrë e shpejtë e aktivitetit të fundit në vetë-vlerësim."
        >
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div
              v-for="metric in analyticsPreview"
              :key="metric.title"
              class="rounded-xl border border-gray-200 p-4 dark:border-gray-800"
            >
              <div class="flex items-center justify-between gap-3">
                <p class="text-sm text-gray-500 dark:text-gray-400">{{ metric.title }}</p>
                <span
                  class="rounded-full px-2.5 py-1 text-xs font-medium"
                  :class="
                    metric.trendType === 'up'
                      ? 'bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-400'
                      : 'bg-warning-50 text-warning-600 dark:bg-warning-500/10 dark:text-warning-400'
                  "
                >
                  {{ metric.trend }}
                </span>
              </div>
              <p class="mt-2 text-2xl font-semibold text-gray-800 dark:text-white/90">{{ metric.value }}</p>
              <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">{{ metric.subtitle }}</p>
            </div>
          </div>
        </component-card>
      </div>

      <div class="col-span-12 xl:col-span-4">
        <component-card title="Veprime të Shpejta" desc="Nis ushtrimet menjëherë.">
          <div class="space-y-3">
            <router-link
              to="/exams"
              class="inline-flex w-full items-center justify-center rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600"
            >
              Hap Testet
            </router-link>
            <router-link
              to="/exams/A"
              class="inline-flex w-full items-center justify-center rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/5"
            >
              Fillo Kategorinë A
            </router-link>
          </div>
        </component-card>
      </div>

      <div class="col-span-12">
        <component-card title="Performanca sipas Kategorive" desc="Parapamje e thjeshtë e pikëve sipas kategorisë nga tentimet e fundit.">
          <div class="grid grid-cols-1 gap-4 xl:grid-cols-2">
            <div
              v-for="item in categoryPerformance"
              :key="item.id"
              class="rounded-xl border border-gray-200 p-4 dark:border-gray-800"
            >
              <div class="mb-2 flex items-center justify-between gap-3">
                <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Kategoria {{ item.id }}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">{{ item.averageScore }}% mesatare</p>
              </div>
              <div class="h-2 w-full rounded-full bg-gray-100 dark:bg-gray-800">
                <div
                  class="h-2 rounded-full bg-brand-500"
                  :style="{ width: `${item.averageScore}%` }"
                ></div>
              </div>
              <div class="mt-3 flex items-center justify-between text-sm">
                <p class="text-gray-500 dark:text-gray-400">{{ item.attempts }} tentativa</p>
                <router-link
                  :to="`/exams/${item.id}`"
                  class="font-medium text-brand-500 hover:text-brand-600 dark:text-brand-400"
                >
                  Vazhdo
                </router-link>
              </div>
            </div>
          </div>
        </component-card>
      </div>
    </div>
  </admin-layout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminLayout from '../components/layout/AdminLayout.vue'
import ComponentCard from '../components/common/ComponentCard.vue'
import { useExamStore } from '@/stores/exam'

const examStore = useExamStore()
const loadingStats = ref(true)

onMounted(async () => {
  try {
    await examStore.fetchCandidateStats(true)
  } catch (error) {
    console.error('Deshtoi ngarkimi i statistikave te kandidatit:', error)
  } finally {
    loadingStats.value = false
  }
})

const stats = computed(() => examStore.candidateStats)

const analyticsPreview = computed(() => {
  const totalAttempts = stats.value?.totalAttempts ?? 0
  const passedAttempts = stats.value?.passedAttempts ?? 0
  const averageScore = stats.value?.averageScorePercent ?? 0
  const passRate = stats.value?.passRatePercent ?? 0

  return [
    {
      title: 'Teste të Përfunduara',
      value: totalAttempts.toString(),
      subtitle: 'Totali i tentimeve të ruajtura',
      trend: loadingStats.value ? 'Duke ngarkuar...' : 'Përditësuar në kohë reale',
      trendType: 'up',
    },
    {
      title: 'Rezultati Mesatar',
      value: `${averageScore.toFixed(2)}%`,
      subtitle: 'Mesatarja nga të gjitha tentimet',
      trend: loadingStats.value ? 'Duke ngarkuar...' : 'Bazuar në historikun tuaj',
      trendType: 'up',
    },
    {
      title: 'Norma e Kalimit',
      value: `${passRate.toFixed(2)}%`,
      subtitle: `${passedAttempts} nga ${totalAttempts} tentativa të kaluara`,
      trend: loadingStats.value ? 'Duke ngarkuar...' : 'Bazuar në tentimet e ruajtura',
      trendType: 'up',
    },
    {
      title: 'Statusi',
      value: loadingStats.value ? '...' : 'Aktiv',
      subtitle: 'Statistikat sinkronizohen pas çdo dorëzimi',
      trend: loadingStats.value ? 'Duke ngarkuar...' : 'Fushë për fokus',
      trendType: 'focus',
    },
  ]
})

const categoryPerformance = computed(() => {
  const fallback = ['A', 'B', 'C', 'D'].map((code) => ({
    id: code,
    averageScore: 0,
    attempts: 0,
  }))

  const apiItems = stats.value?.categoryPerformance ?? []
  if (apiItems.length === 0) {
    return fallback
  }

  const byCode = new Map(apiItems.map((item) => [item.categoryCode.toUpperCase(), item]))

  return fallback.map((item) => {
    const apiItem = byCode.get(item.id)
    if (!apiItem) {
      return item
    }

    return {
      id: item.id,
      averageScore: Number(apiItem.averageScorePercent.toFixed(2)),
      attempts: apiItem.attempts,
    }
  })
})
</script>
