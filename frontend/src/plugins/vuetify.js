import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import { createVuetify } from 'vuetify'

export const vuetify = createVuetify({
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        dark: false,
        colors: {
          primary: '#e11d48',
          secondary: '#0f172a',
          surface: '#ffffff'
        }
      }
    }
  }
})
