<template>
    <div class="d-flex justify-end pr-2 pt-8 pb-2">
        {{ copyrightText }}
    </div>
</template>

<script setup>
const appInitialData = JSON.parse(localStorage.getItem('allSettings')) || {}
const copyrightText = appInitialData.copyRightText || '© 2026 Autoshkolla Linda'
</script>