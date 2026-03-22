<template>
  <admin-layout>
    <div class="space-y-6">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <h2 class="text-xl font-semibold text-gray-800 dark:text-white/90">{{ exam?.title || 'Test' }}</h2>
        <router-link
          :to="categoryBackLink"
          class="inline-flex items-center justify-center rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 lg:text-base dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/5"
        >
          Kthehu te Testet
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

      <component-card v-if="loading" title="Duke u ngarkuar..." desc="Ju lutem prisni ndërsa ngarkohet testi.">
        <div class="py-8 text-center text-gray-500 dark:text-gray-400">Duke u ngarkuar...</div>
      </component-card>

      <component-card v-else-if="!exam" title="Testi nuk u gjet" desc="Ju lutem zgjidhni një kategori dhe test të vlefshëm.">
        <router-link
          to="/exams"
          class="inline-flex items-center justify-center rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600"
        >
          Shko te Testet
        </router-link>
      </component-card>

      <template v-else>
        <component-card
          v-if="step === 'rules'"
          title="Udhëzime"
          :desc="`Lexoni udhëzimet para se të filloni ${exam.title}.`"
        >
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <div
              v-for="item in instructions"
              :key="item.title"
              class="flex items-start gap-3 rounded-xl border border-gray-200 p-4 dark:border-gray-800"
            >
              <div
                class="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full border-2 border-brand-500 text-brand-500"
              >
                <component :is="item.icon" class="h-6 w-6" />
              </div>

              <div>
                <p class="text-sm font-semibold text-gray-800 dark:text-white/90">{{ item.title }}</p>
                <p class="text-sm text-gray-600 dark:text-gray-300">{{ item.description }}</p>
              </div>
            </div>
          </div>

          <div class="flex justify-center pt-1">
            <button
              type="button"
              class="inline-flex items-center justify-center rounded-lg bg-brand-500 px-8 py-3 text-sm font-semibold text-white hover:bg-brand-600"
              @click="startExam"
            >
              FILLO →
            </button>
          </div>

          <p class="text-center text-sm text-gray-600 dark:text-gray-400">
            Pyetjet me përgjigje dhe të drejtën e autorisë e posedon ekskluzivisht MI-RKS.
          </p>
        </component-card>

        <div
          v-else-if="step === 'questions' && currentQuestion"
          class="overflow-hidden rounded-2xl border border-gray-200 bg-white dark:border-gray-800 dark:bg-white/3"
        >
          <div class="flex items-center justify-between gap-3 border-b border-gray-100 px-6 py-4 dark:border-gray-800">
            <h3 class="text-lg font-semibold text-gray-800 dark:text-white/90">
              Test {{ exam.examId }} - Pyetja {{ currentQuestionIndex + 1 }}/{{ exam.questions.length }}
            </h3>
            <div class="flex items-start gap-6 text-right">
              <div>
                <p class="text-xl font-semibold text-gray-800 sm:text-2xl dark:text-white/90">{{ remainingMinutes }}</p>
                <p class="text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">Minuta</p>
              </div>
              <div>
                <p class="text-xl font-semibold text-gray-800 sm:text-2xl dark:text-white/90">{{ remainingDisplaySeconds }}</p>
                <p class="text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">Sekonda</p>
              </div>
            </div>
          </div>

          <div class="grid min-h-95 grid-cols-1 gap-6 p-6 lg:grid-cols-12">
            <div class="lg:col-span-4">
              <img
                :src="currentQuestion.image || DEFAULT_QUESTION_IMAGE"
                :alt="`Question ${currentQuestionIndex + 1}`"
                class="h-60 w-full rounded-lg object-cover"
              />
            </div>

            <div class="space-y-4 lg:col-span-8">
              <p class="text-2xl font-semibold leading-tight text-gray-800 lg:text-3xl dark:text-white/90">
                {{ currentQuestion.text }}
              </p>

              <div class="grid gap-3">
                <button
                  v-for="(option, optionIndex) in currentQuestion.options"
                  :key="optionIndex"
                  type="button"
                  class="flex items-center gap-4 rounded-lg border px-5 py-4 text-left text-base transition sm:text-lg lg:text-xl"
                  :class="
                    selectedOptionIndex === optionIndex
                      ? 'border-brand-500 bg-brand-50 text-brand-700 dark:border-brand-700 dark:bg-brand-500/10 dark:text-brand-300'
                      : 'border-gray-300 text-gray-700 hover:border-brand-300 dark:border-gray-700 dark:text-gray-300 dark:hover:border-brand-700 dark:hover:bg-white/5'
                  "
                  @click="selectAnswer(optionIndex)"
                >
                  <span
                    class="inline-block h-6 w-6 rounded-full border-2"
                    :class="
                      selectedOptionIndex === optionIndex
                        ? 'border-brand-500 bg-brand-500'
                        : 'border-gray-400 dark:border-gray-500'
                    "
                  ></span>
                  <span>{{ option }}</span>
                </button>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-between gap-3 border-t border-gray-100 px-6 py-4 dark:border-gray-800">
            <button
              type="button"
              class="inline-flex min-w-28 items-center justify-center rounded-lg border border-gray-300 px-6 py-3 text-sm font-semibold text-gray-600 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-white/5"
              :disabled="currentQuestionIndex === 0"
              @click="prevQuestion"
            >
              ← PAS
            </button>

            <p class="text-2xl font-semibold text-gray-800 lg:text-4xl dark:text-white/90">4 pikë</p>

            <button
              v-if="isLastQuestion"
              type="button"
              class="inline-flex min-w-28 items-center justify-center rounded-lg bg-brand-500 px-6 py-3 text-sm font-semibold text-white hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-70"
              :disabled="isSubmitting"
              @click="submitExam"
            >
              {{ isSubmitting ? 'Duke dërguar...' : 'DORËZO →' }}
            </button>
            <button
              v-else
              type="button"
              class="inline-flex min-w-28 items-center justify-center rounded-lg bg-brand-500 px-6 py-3 text-sm font-semibold text-white hover:bg-brand-600"
              @click="nextQuestion"
            >
              PARA →
            </button>
          </div>
        </div>

        <component-card
          v-else
          title="Rezultati i Testit"
          :desc="`Ju u përgjigjët saktë në ${correctCount} nga ${exam.questions.length} pyetje.`"
        >
          <div class="flex flex-wrap items-center gap-3">
            <span class="rounded-full bg-success-50 px-3 py-1 text-sm font-medium text-success-600 lg:text-base dark:bg-success-500/10 dark:text-success-400">
              Saktë: {{ correctCount }}
            </span>
            <span class="rounded-full bg-error-50 px-3 py-1 text-sm font-medium text-error-600 lg:text-base dark:bg-error-500/10 dark:text-error-400">
              Gabim: {{ wrongCount }}
            </span>
          </div>

          <div class="space-y-4 pt-2">
            <div
              v-for="(reviewItem, index) in reviewItems"
              :key="reviewItem.question.id"
              class="rounded-xl border border-gray-200 p-4 dark:border-gray-800"
            >
              <div class="mb-3 flex items-center justify-between gap-3">
                <p class="text-sm font-semibold text-gray-800 lg:text-base dark:text-white/90">Pyetja {{ index + 1 }}</p>
                <span
                  class="rounded-full px-3 py-1 text-xs font-medium lg:text-sm"
                  :class="
                    reviewItem.isCorrect
                      ? 'bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-400'
                      : 'bg-error-50 text-error-600 dark:bg-error-500/10 dark:text-error-400'
                  "
                >
                  {{ reviewItem.isCorrect ? 'Saktë' : 'Gabim' }}
                </span>
              </div>

              <img
                :src="reviewItem.question.image || DEFAULT_QUESTION_IMAGE"
                :alt="`Review question ${index + 1}`"
                class="mb-3 h-32 w-32 rounded-lg object-cover sm:h-36 sm:w-36"
              />

              <p class="text-sm text-gray-700 lg:text-base dark:text-gray-300">
                {{ reviewItem.question.text }}
              </p>

              <div class="mt-3 grid gap-3">
                <button
                  v-for="(option, optionIndex) in reviewItem.question.options"
                  :key="optionIndex"
                  type="button"
                  disabled
                  class="flex items-center gap-4 rounded-lg border px-4 py-3 text-left text-sm font-medium lg:text-base"
                  :class="getReviewOptionClasses(reviewItem, optionIndex)"
                >
                  <span
                    class="inline-block h-5 w-5 rounded-full border-2"
                    :class="getReviewIndicatorClasses(reviewItem, optionIndex)"
                  ></span>
                  <span class="flex-1">{{ option }}</span>
                  <span
                    v-if="reviewItem.selectedOption === optionIndex"
                    class="rounded-full border border-brand-200 bg-brand-50 px-2 py-0.5 text-xs font-semibold text-brand-700 dark:border-brand-700 dark:bg-brand-500/10 dark:text-brand-300"
                  >
                    Zgjedhja juaj
                  </span>
                  <span
                    v-if="reviewItem.question.correctOptionIndex === optionIndex"
                    class="rounded-full border border-success-200 bg-success-50 px-2 py-0.5 text-xs font-semibold text-success-700 dark:border-success-700 dark:bg-success-500/10 dark:text-success-400"
                  >
                    E saktë
                  </span>
                </button>
              </div>
            </div>
          </div>

          <button
            type="button"
            class="inline-flex items-center justify-center rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 lg:text-base"
            @click="retakeExam"
          >
            Riprovo Testin
          </button>
        </component-card>
      </template>
    </div>
  </admin-layout>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import ComponentCard from '@/components/common/ComponentCard.vue'
import CategoryAccessDeniedModal from '@/components/common/CategoryAccessDeniedModal.vue'
import CalenderIcon from '@/icons/CalenderIcon.vue'
import DocsIcon from '@/icons/DocsIcon.vue'
import PieChartIcon from '@/icons/PieChartIcon.vue'
import SuccessIcon from '@/icons/SuccessIcon.vue'
import { useExamStore } from '@/stores/exam'

const DEFAULT_QUESTION_IMAGE = '/images/default-question.jpg'

const route = useRoute()
const examStore = useExamStore()

type Step = 'rules' | 'questions' | 'result'
const EXAM_DURATION_SECONDS = 45 * 60

const step = ref<Step>('rules')
const currentQuestionIndex = ref(0)
const answers = ref<Record<number, number>>({})
const remainingSeconds = ref(EXAM_DURATION_SECONDS)
const loading = ref(true)
const isSubmitting = ref(false)
const localErrorMessage = ref('')
const showCategoryAccessDenied = ref(false)
let timer: ReturnType<typeof setInterval> | null = null

const exam = computed(() => examStore.currentExam)
const errorMessage = computed(() => {
  if (examStore.categoryAccessDeniedMessage) {
    return ''
  }

  return localErrorMessage.value || examStore.error || ''
})
const accessDeniedMessage = computed(() => examStore.categoryAccessDeniedMessage || 'Nuk keni akses në këtë kategori, kontaktoni administratorin.')

const category = computed(() => {
  const value = route.params.category
  return typeof value === 'string' ? value.toUpperCase() : ''
})

const examId = computed(() => Number(route.params.examId))

onMounted(async () => {
  if (!category.value || !examId.value) {
    loading.value = false
    return
  }

  examStore.clearError()
  examStore.clearCategoryAccessDenied()
  try {
    await examStore.fetchExam(category.value, examId.value)
    
    if (examStore.currentExam?.durationMinutes) {
      remainingSeconds.value = examStore.currentExam.durationMinutes * 60
    }
  } catch (error: any) {
    if (examStore.categoryAccessDeniedMessage) {
      showCategoryAccessDenied.value = true
    }
    console.error('Deshtoi ngarkimi i testit:', error)
  } finally {
    loading.value = false
  }
})

const categoryBackLink = computed(() =>
  category.value ? `/exams/${category.value}` : '/exams'
)

const currentQuestion = computed(() => {
  if (!exam.value) {
    return null
  }

  return exam.value.questions[currentQuestionIndex.value]
})

const selectedOptionIndex = computed(() => {
  if (!currentQuestion.value) {
    return null
  }

  return answers.value[currentQuestion.value.id]
})

const isLastQuestion = computed(() => {
  if (!exam.value) {
    return false
  }

  return currentQuestionIndex.value === exam.value.questions.length - 1
})

const reviewItems = computed(() => {
  if (!exam.value) {
    return []
  }

  return exam.value.questions.map((question) => {
    const selectedOption = answers.value[question.id]
    const selectedText = selectedOption !== undefined ? question.options[selectedOption] : 'Asnjë përgjigje e zgjedhur'

    return {
      question,
      selectedOption,
      selectedText,
      correctText: question.options[question.correctOptionIndex],
      isCorrect: selectedOption === question.correctOptionIndex,
    }
  })
})

const instructions = computed(() => {
  const questionCount = exam.value?.questions.length ?? 0

  return [
    {
      title: 'Kohëzgjatja',
      description: 'Kohëzgjatja e testit është 45 minuta.',
      icon: CalenderIcon,
    },
    {
      title: 'Përmbajtja',
      description: `Testi përmban ${questionCount} pyetje.`,
      icon: DocsIcon,
    },
    {
      title: 'Kalueshmëria',
      description: 'Kalueshmëria e testit është 85%.',
      icon: PieChartIcon,
    },
    {
      title: 'Pyetjet',
      description: 'Pyetjet kanë vetëm një alternativë të saktë!',
      icon: SuccessIcon,
    },
  ]
})

const correctCount = computed(() => reviewItems.value.filter((item) => item.isCorrect).length)
const wrongCount = computed(() => reviewItems.value.length - correctCount.value)

const getReviewOptionClasses = (
  reviewItem: (typeof reviewItems.value)[number],
  optionIndex: number
) => {
  const isSelected = reviewItem.selectedOption === optionIndex
  const isCorrect = reviewItem.question.correctOptionIndex === optionIndex

  if (isCorrect) {
    return 'border-success-300 bg-success-50 text-success-700 dark:border-success-700 dark:bg-success-500/10 dark:text-success-400'
  }

  if (isSelected) {
    return 'border-error-300 bg-error-50 text-error-700 dark:border-error-700 dark:bg-error-500/10 dark:text-error-400'
  }

  return 'border-gray-200 text-gray-700 dark:border-gray-800 dark:text-gray-300'
}

const getReviewIndicatorClasses = (
  reviewItem: (typeof reviewItems.value)[number],
  optionIndex: number
) => {
  const isSelected = reviewItem.selectedOption === optionIndex
  const isCorrect = reviewItem.question.correctOptionIndex === optionIndex

  if (isCorrect) {
    return 'border-success-500 bg-success-500'
  }

  if (isSelected) {
    return 'border-error-500 bg-error-500'
  }

  return 'border-gray-400 dark:border-gray-500'
}

const remainingMinutes = computed(() => Math.floor(remainingSeconds.value / 60).toString().padStart(2, '0'))
const remainingDisplaySeconds = computed(() => (remainingSeconds.value % 60).toString().padStart(2, '0'))

const stopTimer = () => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

const startTimer = () => {
  stopTimer()
  timer = setInterval(() => {
    if (remainingSeconds.value <= 1) {
      remainingSeconds.value = 0
      stopTimer()
      submitExam()
      return
    }

    remainingSeconds.value -= 1
  }, 1000)
}

const startExam = () => {
  if (!exam.value) {
    return
  }

  localErrorMessage.value = ''
  
  step.value = 'questions'
  currentQuestionIndex.value = 0
  answers.value = {}
  
  const duration = exam.value.durationMinutes ?? 45
  remainingSeconds.value = duration * 60
  startTimer()
}

const selectAnswer = (optionIndex: number) => {
  if (!currentQuestion.value) {
    return
  }

  answers.value[currentQuestion.value.id] = optionIndex
}

const nextQuestion = () => {
  if (!exam.value || isLastQuestion.value) {
    return
  }

  currentQuestionIndex.value += 1
}

const prevQuestion = () => {
  if (currentQuestionIndex.value === 0) {
    return
  }

  currentQuestionIndex.value -= 1
}

const submitExam = async () => {
  if (!exam.value || isSubmitting.value || step.value !== 'questions') {
    return
  }

  localErrorMessage.value = ''
  isSubmitting.value = true
  stopTimer()

  try {
    const examDurationSeconds = (exam.value.durationMinutes ?? 45) * 60
    const durationSeconds = Math.max(0, examDurationSeconds - remainingSeconds.value)

    const submittedAnswers = Object.entries(answers.value).map(([questionId, selectedOptionIndex]) => ({
      questionId: Number(questionId),
      selectedOptionIndex,
    }))

    await examStore.submitExamAttempt({
      categoryCode: category.value,
      examId: exam.value.examId,
      durationSeconds,
      answers: submittedAnswers,
    })

    step.value = 'result'

    try {
      await examStore.fetchCandidateStats(true)
    } catch {
      // Non-blocking for exam result view; dashboard will retry loading stats.
    }
  } catch (error: any) {
    if (examStore.categoryAccessDeniedMessage) {
      showCategoryAccessDenied.value = true
      localErrorMessage.value = ''
      return
    }

    localErrorMessage.value = error?.message || 'Dërgimi i testit dështoi. Ju lutem provoni përsëri.'
    startTimer()
  } finally {
    isSubmitting.value = false
  }
}

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

const retakeExam = () => {
  if (!exam.value) {
    return
  }

  localErrorMessage.value = ''
  
  stopTimer()
  step.value = 'rules'
  currentQuestionIndex.value = 0
  answers.value = {}
  
  const duration = exam.value.durationMinutes ?? 45
  remainingSeconds.value = duration * 60
}

onBeforeUnmount(() => {
  stopTimer()
})
</script>
