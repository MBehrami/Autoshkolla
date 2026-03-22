import { defineStore } from 'pinia'
import { ref } from 'vue'
import api, { ApiResponseError } from '@/plugins/axios'

export interface ExamCategory {
  id: string // Category code (A, B, C, D)
  title: string
  description?: string
}

export interface ExamMeta {
  id: number // examId
  title: string
}

export interface ExamQuestion {
  id: number // examQuestionId
  text: string
  image?: string | null
  options: string[]
  correctOptionIndex: number
}

export interface ExamPayload {
  category: string
  examId: number
  title: string
  durationMinutes: number
  passPercent: number
  rules: string[]
  questions: ExamQuestion[]
}

export interface ExamSubmitAnswer {
  questionId: number
  selectedOptionIndex: number
}

export interface SubmitExamPayload {
  categoryCode: string
  examId: number
  durationSeconds: number
  answers: ExamSubmitAnswer[]
}

export interface SubmitExamResult {
  submissionId: number
  scorePercent: number
  isPassed: boolean
  totalQuestions: number
  correctAnswers: number
  passPercentRequired: number
}

export interface CategoryPerformanceItem {
  categoryCode: string
  attempts: number
  averageScorePercent: number
}

export interface CandidateDashboardStats {
  totalAttempts: number
  passedAttempts: number
  passRatePercent: number
  averageScorePercent: number
  categoryPerformance: CategoryPerformanceItem[]
}

export const useExamStore = defineStore('exam', () => {
  // State
  const categories = ref<ExamCategory[]>([])
  const exams = ref<Record<string, ExamMeta[]>>({}) // Keyed by category code
  const currentExam = ref<ExamPayload | null>(null)
  const candidateStats = ref<CandidateDashboardStats | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const categoryAccessDeniedMessage = ref<string | null>(null)

  const setErrorState = (err: any, fallbackMessage: string) => {
    const message = err?.message || fallbackMessage
    error.value = message

    const statusType = err instanceof ApiResponseError ? err.statusType : ''
    const statusCode = err instanceof ApiResponseError ? err.statusCode : undefined
    const lowerMessage = typeof message === 'string' ? message.toLowerCase() : ''

    const isAccessDenied =
      statusType === 'forbidden' ||
      statusCode === 403 ||
      lowerMessage.includes('nuk keni akses') ||
      lowerMessage.includes('nuk keni qasje') ||
      lowerMessage.includes('access denied')

    categoryAccessDeniedMessage.value = isAccessDenied ? message : null
  }

  // Actions
  const fetchCategories = async (forceRefresh = false) => {
    if (categories.value.length > 0 && !forceRefresh) {
      return categories.value
    }

    isLoading.value = true
    error.value = null
    categoryAccessDeniedMessage.value = null

    try {
      const response = await api.get<ExamCategory[]>('/api/Exams/GetCategoriesForCandidate')
      categories.value = response.data
      return response.data
    } catch (err: any) {
      setErrorState(err, 'Deshtoi ngarkimi i kategorive.')
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const fetchExamsByCategory = async (categoryCode: string, forceRefresh = false) => {
    const code = categoryCode.toUpperCase()
    
    if (exams.value[code] && !forceRefresh) {
      return exams.value[code]
    }

    isLoading.value = true
    error.value = null
    categoryAccessDeniedMessage.value = null

    try {
      const response = await api.get<ExamMeta[]>(`/api/Exams/GetExamsForCandidate/${encodeURIComponent(code)}`)
      exams.value[code] = response.data
      return response.data
    } catch (err: any) {
      setErrorState(err, 'Deshtoi ngarkimi i testeve.')
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const fetchExam = async (categoryCode: string, examId: number, forceRefresh = false) => {
    const code = categoryCode.toUpperCase()
    
    if (currentExam.value?.examId === examId && currentExam.value?.category === code && !forceRefresh) {
      return currentExam.value
    }

    isLoading.value = true
    error.value = null
    categoryAccessDeniedMessage.value = null

    try {
      const response = await api.get<ExamPayload>(`/api/Exams/GetExamForCandidate/${encodeURIComponent(code)}/${examId}`)
      currentExam.value = response.data
      return response.data
    } catch (err: any) {
      setErrorState(err, 'Deshtoi ngarkimi i testit.')
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const submitExamAttempt = async (payload: SubmitExamPayload) => {
    isLoading.value = true
    error.value = null
    categoryAccessDeniedMessage.value = null

    try {
      const response = await api.post<SubmitExamResult>('/api/Exams/SubmitExam', payload)
      return response.data
    } catch (err: any) {
      setErrorState(err, 'Deshtoi dorezimi i testit.')
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const fetchCandidateStats = async (forceRefresh = false) => {
    if (candidateStats.value && !forceRefresh) {
      return candidateStats.value
    }

    isLoading.value = true
    error.value = null
    categoryAccessDeniedMessage.value = null

    try {
      const response = await api.get<CandidateDashboardStats>('/api/Exams/GetCandidateStats')
      candidateStats.value = response.data
      return response.data
    } catch (err: any) {
      setErrorState(err, 'Deshtoi ngarkimi i statistikave te kandidatit.')
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const clearCurrentExam = () => {
    currentExam.value = null
  }

  const clearError = () => {
    error.value = null
  }

  const clearCategoryAccessDenied = () => {
    categoryAccessDeniedMessage.value = null
  }

  const resetStore = () => {
    categories.value = []
    exams.value = {}
    currentExam.value = null
    candidateStats.value = null
    error.value = null
    categoryAccessDeniedMessage.value = null
  }

  return {
    // State
    categories,
    exams,
    currentExam,
    candidateStats,
    isLoading,
    error,
    categoryAccessDeniedMessage,
    
    // Actions
    fetchCategories,
    fetchExamsByCategory,
    fetchExam,
    submitExamAttempt,
    fetchCandidateStats,
    clearCurrentExam,
    clearError,
    clearCategoryAccessDenied,
    resetStore
  }
})
