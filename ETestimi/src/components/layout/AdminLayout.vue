<template>
  <div class="min-h-screen flex flex-col">
    <TopNavbar />
    <div class="flex-1">
      <div class="p-4 mx-auto max-w-(--breakpoint-2xl) md:p-6">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script setup>
import TopNavbar from './TopNavbar.vue'
</script>
