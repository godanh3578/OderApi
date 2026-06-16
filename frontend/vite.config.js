import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

const devApiProxy = process.env.VITE_DEV_API_PROXY ?? 'http://127.0.0.1:5000'
const devOrderApiProxy = process.env.VITE_DEV_ORDER_API_PROXY ?? 'http://127.0.0.1:5022'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    host: '0.0.0.0',
    port: 3000,
    strictPort: true,
    proxy: {
      '/api': {
        target: devApiProxy,
        changeOrigin: true
      },
      '/health': {
        target: devOrderApiProxy,
        changeOrigin: true
      }
    }
  }
})

