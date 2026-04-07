export interface ApiEnvelope<T> {
  data?: T
  obj?: T
  token?: string
  status?: string
  responseMsg?: string
}

export class ApiError extends Error {
  constructor(message: string, public status?: number) {
    super(message)
  }
}

const API_BASE = import.meta.env.VITE_API_URL?.replace(/\/$/, '') ?? ''

export const getApiBaseUrl = () => API_BASE

export const getAuthToken = () => localStorage.getItem('candidateToken') || ''

export const setAuthToken = (token: string) => {
  localStorage.setItem('candidateToken', token)
}

export const clearAuthToken = () => {
  localStorage.removeItem('candidateToken')
}

export async function apiRequest<T>(path: string, init?: RequestInit, requiresAuth = true): Promise<T> {
  const headers = new Headers(init?.headers || {})
  headers.set('Content-Type', 'application/json')

  if (requiresAuth) {
    const token = getAuthToken()
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers,
  })

  const text = await response.text()
  const payload = text ? (JSON.parse(text) as ApiEnvelope<T> | T) : ({} as ApiEnvelope<T>)

  if (!response.ok) {
    const msg = (payload as ApiEnvelope<T>)?.responseMsg || `Kerkesa deshtoi me status ${response.status}`
    throw new ApiError(msg, response.status)
  }

  if ((payload as ApiEnvelope<T>)?.status === 'error') {
    throw new ApiError((payload as ApiEnvelope<T>).responseMsg || 'Serveri ktheu nje gabim.')
  }

  if ((payload as ApiEnvelope<T>)?.data !== undefined) {
    return (payload as ApiEnvelope<T>).data as T
  }

  if ((payload as ApiEnvelope<T>)?.obj !== undefined) {
    return (payload as ApiEnvelope<T>).obj as T
  }

  return payload as T
}
