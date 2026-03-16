/**
 * plugins/vuetify.js
 */
import '@mdi/font/css/materialdesignicons.css'
import { aliases, mdi } from 'vuetify/iconsets/mdi'
import 'vuetify/styles'
import { createVuetify } from 'vuetify'

export default createVuetify({
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: { mdi },
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#2563eb',
          'primary-darken-1': '#1d4ed8',
          secondary: '#64748b',
          'secondary-darken-1': '#475569',
          accent: '#8b5cf6',
          success: '#10b981',
          warning: '#f59e0b',
          error: '#ef4444',
          info: '#06b6d4',
          background: '#f1f5f9',
          surface: '#ffffff',
          'surface-variant': '#f8fafc',
          'on-background': '#1e293b',
          'on-surface': '#334155',
        },
      },
    },
  },
  defaults: {
    VCard: {
      rounded: 'lg',
      elevation: 0,
    },
    VBtn: {
      rounded: 'lg',
      fontWeight: 500,
    },
    VTextField: {
      variant: 'outlined',
      density: 'compact',
      hideDetails: 'auto',
      color: 'primary',
    },
    VSelect: {
      variant: 'outlined',
      density: 'compact',
      hideDetails: 'auto',
      color: 'primary',
    },
    VAutocomplete: {
      variant: 'outlined',
      density: 'compact',
      hideDetails: 'auto',
      color: 'primary',
    },
    VDataTable: {
      hover: true,
    },
  },
})
