<template>
  <div ref="fullscreenElement">
    <v-app>
      <Navbar v-if="isVisible" :isFullscreen="fullscreenStatus" @toggle-screen="toggleFullscreen" />
      <Message />
      <v-main>
        <RouterView />
      </v-main>
      <Loading v-if="isVisible" />
      <Footer v-if="isVisible" />
    </v-app>
  </div>
</template>

<script setup>
import Loading from './components/common/Loading.vue';
import Navbar from './components/common/Navbar.vue';
import Footer from './components/common/Footer.vue';
import Message from './components/common/Message.vue';
import API from './store/API';
import { useUserStore } from './store/UserStore';
import { useSettingStore } from './store/SettingStore';
import { useRouter } from 'vue-router'
import { computed, onErrorCaptured, onMounted, onUnmounted, ref } from 'vue';
import { useRoute } from 'vue-router';

const fullscreenElement = ref(null)
const fullscreenStatus = ref(false)
const userStore = useUserStore()
const settingStore = useSettingStore()
const router = useRouter()
const route = useRoute()

// Dashboard layout (navbar, sidebar, footer) should NEVER show on public pages
const isVisible = computed({
  get: () => {
    // Hide layout on any public route (sign-in, password-reset, error pages)
    if (route.meta?.public) return false
    return userStore.visible
  },
  set: (newVal) => userStore.setVisibility(newVal)
})

//toggle full screen
const toggleFullscreen = async (isFullscreen) => {
  if (!isFullscreen) {
    try {
      await fullscreenElement.value.requestFullscreen();
    } catch (error) {
      console.error("Failed to enter fullscreen mode:", error);
    }
  } else {
    try {
      await document.exitFullscreen();
    } catch (error) {
      console.error("Failed to exit fullscreen mode:", error);
    }
  }
}

//full screen status update
const handleFullscreenChange = () => {
  fullscreenStatus.value = !!document.fullscreenElement
}

//Mount life cycle hook
onMounted(() => {
  document.addEventListener('fullscreenchange', handleFullscreenChange)
})

//Un-Mount life cycle hook
onUnmounted(() => {
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
})

//App Error Capture life cycle hook
onErrorCaptured((err) => {
  console.log('onErrorCaptured', err)
})

// Guard flag to prevent recursive interceptor loops when the API is unreachable
let isLoggingError = false

API.interceptors.response.use(
  (response) => response,
  (err) => {
    const status = err?.response?.status || err?.request?.status || 0
    localStorage.setItem('http_error', status)

    // ── 401 Unauthorized — clear session, redirect to sign-in (no full-page reload) ──
    if (status === 401) {
      localStorage.removeItem('profile')
      localStorage.removeItem('userId')
      localStorage.removeItem('logCode')
      localStorage.removeItem('visible')
      userStore.visible = false
      // Only redirect if not already on sign-in to avoid loops
      if (router.currentRoute.value.name !== 'SignIn') {
        router.replace({ name: 'SignIn' })
      }
      return Promise.reject(err)
    }

    // ── Network error (status 0) — API/database unreachable ──
    if (status === 0) {
      // Don't try to log to an unreachable API — just show a notification
      settingStore.toggleSnackbar({
        status: true,
        msg: 'Server is unreachable. Please check your connection and try again.'
      })
      // Clear session and hide dashboard layout, then go to sign-in
      localStorage.removeItem('profile')
      localStorage.removeItem('userId')
      localStorage.removeItem('logCode')
      localStorage.removeItem('visible')
      userStore.visible = false
      if (router.currentRoute.value.name !== 'SignIn') {
        router.replace({ name: 'SignIn' })
      }
      return Promise.reject(err)
    }

    // ── 5xx server errors — try to log once, then redirect ──
    if (status >= 500) {
      const objErrorLog = {
        status: status,
        statusText: err?.response?.statusText || err?.request?.statusText,
        url: err?.response?.config?.url || err?.request?.responseURL,
        message: err.message,
        addedBy: localStorage.getItem('userId') || 0
      }
      // Guard: only attempt one error log at a time to prevent recursive calls
      if (!isLoggingError) {
        isLoggingError = true
        settingStore.createErrorLog(objErrorLog)
          .catch(() => { /* API may be down — silently ignore */ })
          .finally(() => { isLoggingError = false })
      }
      settingStore.toggleSnackbar({
        status: true,
        msg: 'A server error occurred. Please try again later.'
      })
      router.push({ name: 'OtherError' })
    }

    return Promise.reject(err)
  }
)
</script>

<style>
/* ═══════════════════════════════════════════════════════
   Modern SaaS Design System — Global Styles
   ═══════════════════════════════════════════════════════ */

:root {
  --sidebar-width: 260px;
  --header-height: 64px;
  --radius-sm: 6px;
  --radius-md: 10px;
  --radius-lg: 14px;
  --radius-xl: 18px;
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.04), 0 1px 2px rgba(0,0,0,0.06);
  --shadow-md: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -2px rgba(0,0,0,0.05);
  --shadow-lg: 0 10px 15px -3px rgba(0,0,0,0.06), 0 4px 6px -4px rgba(0,0,0,0.04);
  --shadow-card: 0 1px 3px rgba(0,0,0,0.05), 0 1px 2px rgba(0,0,0,0.04);
  --font-family: 'Inter', 'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --slate-50: #f8fafc;
  --slate-100: #f1f5f9;
  --slate-200: #e2e8f0;
  --slate-300: #cbd5e1;
  --slate-400: #94a3b8;
  --slate-500: #64748b;
  --slate-600: #475569;
  --slate-700: #334155;
  --slate-800: #1e293b;
  --slate-900: #0f172a;
}

*, *::before, *::after {
  box-sizing: border-box;
}

html, body {
  margin: 0;
  padding: 0;
  font-family: var(--font-family);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

body {
  background: var(--slate-100);
}

/* ─── Main Content Area ─── */
.v-main {
  background: var(--slate-100) !important;
}

/* ─── Card System ─── */
.v-card {
  border: 1px solid var(--slate-200) !important;
  box-shadow: var(--shadow-card) !important;
  transition: box-shadow 0.2s ease, transform 0.2s ease;
}

.v-card:not(.v-card--flat):not(.v-card--outlined):hover {
  box-shadow: var(--shadow-md) !important;
}

/* ═══ Login Page — full isolation from redesign ═══ */
.login-page {
  position: fixed;
  inset: 0;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: #f8fafc !important;
  padding: 24px;
}

.login-page .login-wrapper {
  width: 100%;
  max-width: 420px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 24px;
}

.login-page .login-card {
  width: 100%;
  position: relative;
  overflow: hidden;
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06), 0 6px 16px rgba(0,0,0,0.06);
  padding: 36px 32px 32px;
}

.login-page .login-card-header {
  text-align: center;
  margin-bottom: 28px;
}

.login-page .login-title {
  font-size: 1.375rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 6px;
  line-height: 1.3;
}

.login-page .login-subtitle {
  font-size: 0.875rem;
  color: #94a3b8;
  margin: 0;
  font-weight: 400;
}

.login-page .login-fields {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 24px;
}

.login-page .field-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.login-page .field-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #334155;
  letter-spacing: 0.01em;
}

.login-page .login-input .v-field {
  border-radius: 10px !important;
  min-height: 46px;
}

.login-page .login-input .v-field--variant-outlined .v-field__outline {
  --v-field-border-opacity: 0.18;
}

.login-page .login-input .v-field--variant-outlined:hover .v-field__outline {
  --v-field-border-opacity: 0.35;
}

.login-page .login-input .v-field--focused .v-field__outline {
  --v-field-border-opacity: 1;
  color: #2563eb;
}

.login-page .login-input .v-field .v-icon {
  color: #94a3b8;
  font-size: 20px;
}

.login-page .login-input .v-field--focused .v-icon {
  color: #2563eb;
}

.login-page .login-input input::placeholder {
  color: #cbd5e1;
  font-weight: 400;
}

.login-page .login-actions {
  margin-bottom: 20px;
}

.login-page .login-btn {
  border-radius: 10px !important;
  text-transform: none !important;
  font-weight: 600 !important;
  font-size: 0.9375rem !important;
  letter-spacing: 0.01em !important;
  min-height: 46px !important;
}

.login-page .login-footer {
  text-align: center;
  padding-top: 4px;
}

.login-page .forgot-link {
  font-size: 0.8125rem;
  color: #64748b;
  background: none;
  border: none;
  cursor: pointer;
  font-weight: 500;
  transition: color 0.15s ease;
  padding: 4px 8px;
  border-radius: 6px;
}

.login-page .forgot-link:hover {
  color: #2563eb;
}

.login-page .forgot-overlay {
  position: absolute;
  inset: 0;
  background: #ffffff;
  border-radius: 16px;
  padding: 36px 32px 32px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  z-index: 2;
}

.login-page .forgot-actions {
  display: flex;
  gap: 12px;
}

.login-page .login-copyright {
  font-size: 0.75rem;
  color: #94a3b8;
  margin: 0;
  text-align: center;
}

/* Login page — prevent global card styles from applying */
.login-page .v-card {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
}

@media (max-width: 480px) {
  .login-page {
    padding: 16px;
  }
  .login-page .login-card {
    padding: 28px 20px 24px;
    border-radius: 12px;
  }
  .login-page .login-wrapper {
    max-width: 100%;
  }
}

/* ─── Table System ─── */
.v-data-table {
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
  border: 1px solid var(--slate-200);
  background: #fff !important;
}

.v-data-table .v-table__wrapper {
  overflow-x: auto;
}

.v-data-table thead th {
  background: var(--slate-50) !important;
  color: var(--slate-600) !important;
  font-weight: 600 !important;
  font-size: 0.75rem !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 2px solid var(--slate-200) !important;
  padding: 14px 16px !important;
  white-space: nowrap;
}

.v-data-table tbody tr {
  transition: background 0.15s ease;
}

.v-data-table tbody tr:hover {
  background: rgba(37, 99, 235, 0.03) !important;
}

.v-data-table tbody td {
  color: var(--slate-700) !important;
  font-size: 0.875rem !important;
  padding: 12px 16px !important;
  border-bottom: 1px solid var(--slate-100) !important;
}

.v-data-table-footer {
  border-top: 1px solid var(--slate-200) !important;
  padding: 8px 16px !important;
}

/* ─── Button System ─── */
.v-btn {
  font-weight: 500 !important;
  letter-spacing: 0.01em !important;
  text-transform: none !important;
}

/* ─── Input / Field System ─── */
.v-field--variant-outlined .v-field__outline {
  --v-field-border-opacity: 0.2;
}

.v-field--variant-outlined:hover .v-field__outline {
  --v-field-border-opacity: 0.35;
}

.v-field--focused .v-field__outline {
  --v-field-border-opacity: 1;
}

/* ─── Dialog / Modal System ─── */
.v-overlay__content > .v-card {
  border: none !important;
  background: #ffffff !important;
  color: var(--slate-800) !important;
  box-shadow: 0 20px 60px -12px rgba(0,0,0,0.2), 0 8px 20px -8px rgba(0,0,0,0.12) !important;
  border-radius: var(--radius-lg) !important;
}

.v-overlay__content > .v-card .v-card-title {
  color: var(--slate-800) !important;
  font-weight: 600 !important;
  font-size: 1.05rem !important;
}

.v-overlay__content > .v-card .v-card-text {
  color: var(--slate-700) !important;
}

.v-overlay__content > .v-card .v-card-actions {
  border-top: 1px solid var(--slate-100);
}

/* ─── Snackbar — high contrast ─── */
.v-snackbar > .v-snackbar__wrapper {
  background: var(--slate-800) !important;
  color: #ffffff !important;
  border-radius: 10px !important;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2) !important;
}

.v-snackbar .v-snackbar__content {
  color: #ffffff !important;
  font-size: 0.875rem !important;
}

.v-snackbar .v-btn {
  color: #f472b6 !important;
}

/* ─── Toolbar ─── */
.v-toolbar {
  box-shadow: none !important;
}

/* ─── Scrollbar ─── */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: var(--slate-300);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: var(--slate-400);
}

/* ─── Page Container Utility ─── */
.page-container {
  width: 100%;
  max-width: 100%;
  padding: 24px 28px 48px;
  margin: 0;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 1.5rem !important;
  font-weight: 700 !important;
  color: var(--slate-800) !important;
  line-height: 1.3;
}

.page-subtitle {
  font-size: 0.875rem;
  color: var(--slate-500);
  margin-top: 4px;
}

/* ─── Table Card Wrapper ─── */
.table-card {
  border: 1px solid var(--slate-200) !important;
  border-radius: var(--radius-lg) !important;
  overflow: hidden;
  background: #fff !important;
  box-shadow: var(--shadow-card) !important;
}

.table-card .v-data-table {
  border: none !important;
  box-shadow: none !important;
}

/* ─── Filter Bar ─── */
.filter-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 16px 20px;
  background: #fff;
  border-bottom: 1px solid var(--slate-100);
  flex-wrap: wrap;
}

.filter-bar .v-field {
  border-radius: var(--radius-sm) !important;
}

/* ─── Export Buttons Group ─── */
.export-group {
  display: flex;
  align-items: center;
  gap: 6px;
}

.export-group .v-btn {
  font-size: 0.8125rem;
}

/* ─── Action Icon Buttons ─── */
.action-btn {
  width: 34px !important;
  height: 34px !important;
  min-width: 34px !important;
}

/* ─── Chip / Badge ─── */
.v-chip {
  font-weight: 500 !important;
}

/* ─── Snackbar ─── */
.v-snackbar__content {
  font-weight: 500 !important;
}

/* ─── Container Override ─── */
.v-container {
  max-width: 100% !important;
}

/* ═══ Responsive ═══ */
@media (max-width: 600px) {
  .page-container {
    padding: 16px 10px !important;
  }

  .page-title {
    font-size: 1.25rem !important;
  }

  .filter-bar {
    padding: 12px !important;
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }

  .export-group {
    width: 100%;
    justify-content: stretch;
  }

  .export-group .v-btn {
    flex: 1;
    min-width: 0;
  }

  .v-data-table thead th,
  .v-data-table tbody td {
    padding: 8px 8px !important;
    font-size: 0.75rem !important;
  }

  .v-data-table .v-table__wrapper {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .v-btn {
    min-height: 40px;
  }

  .v-text-field input,
  .v-select .v-field__input,
  .v-autocomplete .v-field__input {
    font-size: 16px !important;
  }

  /* ─── Mobile Dialogs ─── */
  .v-dialog > .v-overlay__content {
    max-width: calc(100vw - 16px) !important;
    max-height: calc(100vh - 24px) !important;
    margin: 8px !important;
    width: 100% !important;
  }

  .v-overlay__content > .v-card {
    border-radius: var(--radius-md) !important;
  }

  .v-overlay__content > .v-card .v-card-title {
    font-size: 0.95rem !important;
    padding: 14px 16px !important;
  }

  .v-overlay__content > .v-card .v-card-text {
    padding: 16px !important;
  }

  .v-overlay__content > .v-card .v-card-actions {
    padding: 12px 16px !important;
  }

  .v-overlay__content > .v-card .v-card-text .v-row .v-col {
    padding: 4px 8px !important;
  }

  .v-app-bar .v-btn {
    min-width: 36px !important;
    padding: 0 6px !important;
  }

  .v-toolbar__content {
    flex-wrap: wrap !important;
    height: auto !important;
  }

  /* ─── Mobile: table action buttons smaller ─── */
  .v-data-table .action-btn {
    width: 28px !important;
    height: 28px !important;
    min-width: 28px !important;
  }
  .v-data-table .action-btn .v-icon {
    font-size: 16px !important;
  }
}

@media (max-width: 960px) {
  .page-container {
    padding: 20px 14px !important;
  }

  /* ─── Tablet Dialogs ─── */
  .v-dialog > .v-overlay__content {
    max-width: calc(100vw - 32px) !important;
    margin: 16px !important;
  }
}

/* ═══ Table Action Buttons — visible hover states ═══ */
.v-data-table .action-btn {
  width: 32px !important;
  height: 32px !important;
  min-width: 32px !important;
  border-radius: 8px !important;
  transition: all 0.15s ease !important;
  background: transparent !important;
}

.v-data-table .action-btn:hover {
  background: rgba(37, 99, 235, 0.08) !important;
}

.v-data-table .action-btn .v-icon {
  opacity: 0.75;
  transition: opacity 0.15s ease;
}

.v-data-table .action-btn:hover .v-icon {
  opacity: 1;
}

/* Prevent row hover from altering button/chip/icon colors */
.v-data-table tbody tr:hover .v-btn .v-icon {
  color: currentColor !important;
  opacity: 1 !important;
}

.v-data-table tbody tr:hover .v-chip {
  opacity: 1 !important;
}

.v-data-table tbody tr:hover .v-btn--variant-tonal,
.v-data-table tbody tr:hover .v-btn--variant-text {
  opacity: 1 !important;
}

/* Global: visible hover feedback */
.v-btn--variant-text:hover {
  background: rgba(0,0,0,0.04) !important;
}

.v-btn--variant-elevated:hover,
.v-btn--variant-flat:hover {
  filter: brightness(1.06);
}

/* Section title used inside cards/pages */
.section-title {
  font-size: 0.8125rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--slate-500);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--slate-100);
}
</style>