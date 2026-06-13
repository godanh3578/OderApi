import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

const devApiProxy = process.env.VITE_DEV_API_PROXY ?? 'http://localhost:5002'

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
    port: 5173,
    proxy: {
<<<<<<< HEAD
    '/api': {
      target: 'http://localhost:5002',
      changeOrigin: true,
      configure: (proxy) => {
      proxy.on('error', (err) => console.log('Proxy error:', err))
=======
      '/api': {
        target: devApiProxy,
        changeOrigin: true
>>>>>>> c5051e7ab76feabc925b3217d4c544efb417dbb6
      }
    }
  }
  },
})
