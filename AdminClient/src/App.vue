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

const fullscreenElement = ref(null)
const fullscreenStatus = ref(false)
const userStore = useUserStore()
const settingStore = useSettingStore()
const router = useRouter()

//navbar visibility check
const isVisible = computed({
  get: () => userStore.visible,
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

//http errors – log and redirect only for unexpected server errors (5xx / network),
//but re-throw 4xx so callers (.catch) can handle validation / auth errors gracefully
API.interceptors.response.use(
  (response) => response,
  (err) => {
    const status = err?.response?.status || err?.request?.status || 0
    localStorage.setItem('http_error', status)
    const objErrorLog = {
      status: status,
      statusText: err?.response?.statusText || err?.request?.statusText,
      url: err?.response?.config?.url || err?.request?.responseURL,
      message: err.message,
      addedBy: localStorage.getItem('userId') || 0
    }
    // Only redirect for 5xx / network errors; let 400/401/403/404 be handled by callers
    if (status >= 500 || status === 0) {
      settingStore.createErrorLog(objErrorLog)
      router.push({ name: 'OtherError' })
    }
    return Promise.reject(err)
  }
)
</script>

<style>
body {
  position: relative;
  height: 100%;
  width: 100%;
  overflow: hidden;
}

/* width */
::-webkit-scrollbar {
  width: 5px;
}

/* Track */
::-webkit-scrollbar-track {
  background: #f1f1f1;
}

/* Handle */
::-webkit-scrollbar-thumb {
  background: #a3a3c2;
}

/* Handle on hover */
::-webkit-scrollbar-thumb:hover {
  background: #555;
}
</style>