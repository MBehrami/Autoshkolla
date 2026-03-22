import axios, { AxiosError, type InternalAxiosRequestConfig, type AxiosResponse } from 'axios'
import router from '@/router'

// API Response Envelopes
export interface ApiEnvelope<T = any> {
  data?: T
  obj?: T
  token?: string
  status?: string
  responseMsg?: string
}

export interface ApiErrorResponse {
  Status?: string
  ResponseMsg?: string
  status?: string
  responseMsg?: string
}

// Create axios instance
const api = axios.create({
  baseURL: (import.meta.env.VITE_API_URL as string)?.replace(/\/$/, '') || 'http://localhost:5002',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor - Add auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('candidateToken')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error: AxiosError) => {
    return Promise.reject(error)
  }
)

// Response interceptor - Handle errors and unwrap envelopes
api.interceptors.response.use(
  (response: AxiosResponse<ApiEnvelope>) => {
    // Unwrap the envelope if it exists
    const data = response.data

    // Check for error status in the envelope
    if (data?.status === 'error') {
      const errorMsg = data.responseMsg || 'Ndodhi nje gabim.'
      throw new ApiResponseError(errorMsg, 'error', response.status)
    }

    // Check for other status types (inactive, expired, incorrect, duplicate)
    if (data?.status && data.status !== 'success') {
      const errorMsg = data.responseMsg || 'Kerkesa deshtoi.'
      throw new ApiResponseError(errorMsg, data.status, response.status)
    }

    // Keep login/token envelope intact so auth store can persist token reliably.
    if (data?.token !== undefined) {
      return response
    }

    // Return unwrapped data or original response
    if (data?.data !== undefined) {
      return { ...response, data: data.data }
    }
    if (data?.obj !== undefined) {
      return { ...response, data: data.obj }
    }

    return response
  },
  (error: AxiosError<ApiErrorResponse>) => {
    // Handle network errors
    if (!error.response) {
      console.error('Gabim rrjeti:', error.message)
      return Promise.reject(new ApiResponseError('Gabim rrjeti. Ju lutem kontrolloni lidhjen tuaj.', 'error'))
    }

    // Handle HTTP errors
    const status = error.response.status
    const data = error.response.data

    // Extract error message
    const errorMessage = 
      data?.responseMsg || 
      data?.ResponseMsg || 
      data?.status ||
      data?.Status ||
      error.message || 
      'Ndodhi nje gabim i papritur.'

    // Handle 401 Unauthorized
    if (status === 401) {
      localStorage.removeItem('candidateToken')
      localStorage.removeItem('candidateProfile')
      
      // Only redirect if not already on signin page
      if (router.currentRoute.value.name !== 'Signin') {
        router.push({ name: 'Signin' })
      }
      
      return Promise.reject(new ApiResponseError('Sesioni ka skaduar. Ju lutem kycuni perseri.', 'unauthorized', status))
    }

    // Handle 403 Forbidden
    if (status === 403) {
      const forbiddenMessage =
        data?.responseMsg ||
        data?.ResponseMsg ||
        'Nuk keni qasje.'

      return Promise.reject(new ApiResponseError(forbiddenMessage, 'forbidden', status))
    }

    // Handle 404 Not Found
    if (status === 404) {
      return Promise.reject(new ApiResponseError('Burimi nuk u gjet.', 'not_found', status))
    }

    // Handle 500 Server Error
    if (status >= 500) {
      return Promise.reject(new ApiResponseError('Gabim ne server. Ju lutem provoni perseri me vone.', 'server_error', status))
    }

    return Promise.reject(new ApiResponseError(errorMessage, data?.status || 'error', status))
  }
)

// Custom error class
export class ApiResponseError extends Error {
  constructor(
    message: string,
    public statusType: string = 'error',
    public statusCode?: number
  ) {
    super(message)
    this.name = 'ApiResponseError'
  }
}

// Helper function to handle API responses with token
export function extractTokenFromResponse<T>(response: AxiosResponse<ApiEnvelope<T>>): { data: T; token?: string } {
  const envelope = response.data as ApiEnvelope<T>
  return {
    data: (envelope.obj ?? envelope.data) as T,
    token: envelope.token
  }
}

export default api
