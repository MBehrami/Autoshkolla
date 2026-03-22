/**
 * main.js
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Plugins
import { registerPlugins } from '@/plugins'

//Router
import router from './router'

// Components
import App from './App.vue'

// Composables
import { createApp } from 'vue'
import { createPinia } from 'pinia'

const app = createApp(App)
const pinia=createPinia()

app.config.errorHandler=()=>{
}

registerPlugins(app)

app.use(router)

app.use(pinia)

app.mount('#app')
