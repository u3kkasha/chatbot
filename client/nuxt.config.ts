// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui',
    '@comark/nuxt',
    'nuxt-auth-utils',
    'nuxt-csurf',
    '@pinia/nuxt',
    '@pinia/colada-nuxt'
  ],

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  routeRules: {
    '/api/chat/**': { proxy: 'http://localhost:5136/api/chat/**' },
    '/api/chats/**': { proxy: 'http://localhost:5136/api/chats/**' }
  },

  experimental: {
    viewTransition: true
  },

  compatibilityDate: '2024-07-11',

  vite: {
    optimizeDeps: {
      include: ['@vue/devtools-core', '@vue/devtools-kit', 'date-fns']
    }
  },

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  },

  icon: {
    clientBundle: {
      scan: true,
      includeCustomCollections: true
    }
  }
})
