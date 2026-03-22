import { apiRequest, clearAuthToken, setAuthToken } from './api'

export interface CandidateProfile {
  candidateAccountId: number
  candidateId: number
  fullName: string
  phoneNumber: string
  email?: string | null
  validTo: string
  categoryId?: number
  roleName?: string
}

interface LoginResponse {
  token: string
  obj: CandidateProfile
}

const PROFILE_KEY = 'candidateProfile'

export const getStoredProfile = (): CandidateProfile | null => {
  try {
    const raw = localStorage.getItem(PROFILE_KEY)
    return raw ? (JSON.parse(raw) as CandidateProfile) : null
  } catch {
    return null
  }
}

export const isAuthenticated = () => !!localStorage.getItem('candidateToken')

export const signOut = () => {
  clearAuthToken()
  localStorage.removeItem(PROFILE_KEY)
}

export async function signIn(phoneNumber: string, password: string): Promise<CandidateProfile> {
  const response = await apiRequest<LoginResponse>(
    '/api/CandidateAuth/Login',
    {
      method: 'POST',
      body: JSON.stringify({ phoneNumber, password }),
    },
    false
  )

  setAuthToken(response.token)
  localStorage.setItem(PROFILE_KEY, JSON.stringify(response.obj))
  return response.obj
}

export async function fetchMe(): Promise<CandidateProfile> {
  const profile = await apiRequest<CandidateProfile>('/api/CandidateAuth/Me', { method: 'GET' })
  localStorage.setItem(PROFILE_KEY, JSON.stringify(profile))
  return profile
}
