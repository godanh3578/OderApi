import './polyfills/cryptoRandomUUID'
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(router)
app.directive('click-outside', {
  mounted(el, binding) {
    el._clickOutside = (event) => {
      if (!el.contains(event.target)) binding.value(event)
    }
    document.addEventListener('click', el._clickOutside)
  },
  unmounted(el) {
    document.removeEventListener('click', el._clickOutside)
  }
})
app.mount('#app')
