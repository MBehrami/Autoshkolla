import { apiRequest } from './api'

export interface ExamCategory {
  id: string // This is the category code (A, B, C, D)
  title: string
  description?: string
}

export interface ExamMeta {
  id: number // This is examId
  title: string
}

export interface ExamQuestion {
  id: number // This is examQuestionId
  text: string
  image?: string | null
  options: string[] // Array of option text strings
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

export const getCandidateCategories = () =>
  apiRequest<ExamCategory[]>('/api/Exams/GetCategoriesForCandidate', { method: 'GET' })

export const getCandidateExamsByCategory = (categoryCode: string) =>
  apiRequest<ExamMeta[]>(`/api/Exams/GetExamsForCandidate/${encodeURIComponent(categoryCode)}`, { method: 'GET' })

export const getCandidateExam = (categoryCode: string, examId: number) =>
  apiRequest<ExamPayload>(`/api/Exams/GetExamForCandidate/${encodeURIComponent(categoryCode)}/${examId}`, { method: 'GET' })

