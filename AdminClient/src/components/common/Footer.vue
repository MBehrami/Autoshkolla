<template>
    <div class="app-footer">
        {{ copyrightText }}
    </div>
</template>

<script setup>
const appInitialData = JSON.parse(localStorage.getItem('allSettings')) || {}
const copyrightText = appInitialData.copyRightText || '© 2026 Autoshkolla Linda'
</script>

<style scoped>
.app-footer {
    text-align: center;
    padding: 16px 24px;
    font-size: 0.75rem;
    color: var(--slate-400);
    font-weight: 500;
    letter-spacing: 0.01em;
}
</style>
