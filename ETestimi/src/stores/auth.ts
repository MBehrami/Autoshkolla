import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api, { type ApiEnvelope } from '@/plugins/axios'

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
const TOKEN_KEY = 'candidateToken'

export const useAuthStore = defineStore('auth', () => {
  // State
  const profile = ref<CandidateProfile | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!profile.value && !!localStorage.getItem(TOKEN_KEY))
  const token = computed(() => localStorage.getItem(TOKEN_KEY))
  const fullName = computed(() => profile.value?.fullName || 'Kandidat')
  const phoneNumber = computed(() => profile.value?.phoneNumber || '')

  // Actions
  const loadProfileFromStorage = () => {
    try {
      const stored = localStorage.getItem(PROFILE_KEY)
      if (stored) {
        profile.value = JSON.parse(stored) as CandidateProfile
      }
    } catch (err) {
      console.error('Deshtoi ngarkimi i profilit nga ruajtja lokale:', err)
      clearAuth()
    }
  }

  const signIn = async (phoneNumber: string, password: string) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await api.post<ApiEnvelope<CandidateProfile>>('/api/CandidateAuth/Login', {
        phoneNumber,
        password
      })

      const envelope = response.data as any
      const resolvedToken = envelope?.token || envelope?.Token
      const resolvedProfile = envelope?.obj || envelope?.data || envelope

      if (!resolvedToken) {
        throw new Error('Kyçja u krye por tokeni nuk u kthye nga API.')
      }

      const loginData: LoginResponse = {
        token: resolvedToken,
        obj: resolvedProfile
      }

      // Store token
      localStorage.setItem(TOKEN_KEY, loginData.token)
      
      // Store profile
      profile.value = loginData.obj
      localStorage.setItem(PROFILE_KEY, JSON.stringify(loginData.obj))

      return loginData.obj
    } catch (err: any) {
      error.value = err.message || 'Kyçja deshtoi.'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const fetchProfile = async () => {
    isLoading.value = true
    error.value = null

    try {
      const response = await api.get<CandidateProfile>('/api/CandidateAuth/Me')
      profile.value = response.data
      localStorage.setItem(PROFILE_KEY, JSON.stringify(response.data))
      return response.data
    } catch (err: any) {
      error.value = err.message || 'Deshtoi marrja e profilit.'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const changePassword = async (currentPassword: string, newPassword: string) => {
    isLoading.value = true
    error.value = null

    try {
      await api.patch('/api/CandidateAuth/ChangePassword', {
        currentPassword,
        newPassword
      })
    } catch (err: any) {
      error.value = err.message || 'Deshtoi ndryshimi i fjalekalimit.'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const signOut = () => {
    profile.value = null
    error.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(PROFILE_KEY)
  }

  const clearAuth = () => {
    signOut()
  }

  const clearError = () => {
    error.value = null
  }

  // Initialize auth state from storage
  loadProfileFromStorage()

  return {
    // State
    profile,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    token,
    fullName,
    phoneNumber,
    
    // Actions
    signIn,
    fetchProfile,
    changePassword,
    signOut,
    clearAuth,
    clearError,
    loadProfileFromStorage
  }
})
