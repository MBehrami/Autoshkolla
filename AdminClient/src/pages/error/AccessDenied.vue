<template>
    <div class="access-denied-wrap">
        <v-card class="access-denied-card" rounded="lg">
            <div class="access-denied-icon">
                <v-icon icon="mdi-shield-lock-outline" size="48" color="error"></v-icon>
            </div>
            <div class="access-denied-title">Qasja u refuzua</div>
            <div class="access-denied-text">
                Nuk keni leje për të hapur këtë faqe ose për të kryer këtë veprim.
                Nëse mendoni se është gabim, kontaktoni administratorin.
            </div>
            <div class="access-denied-actions">
                <v-btn color="primary" variant="flat" class="text-none" prepend-icon="mdi-view-dashboard"
                    @click="goDashboard">
                    Kthehu te Paneli
                </v-btn>
            </div>
        </v-card>
    </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const router = useRouter();

function goDashboard() {
    router.replace({ name: 'Dashboard' });
}
</script>

<style scoped>
.access-denied-wrap {
    min-height: 70vh;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px;
}

.access-denied-card {
    max-width: 460px;
    width: 100%;
    text-align: center;
    padding: 40px 32px;
    border: 1px solid var(--slate-200);
}

.access-denied-icon {
    width: 88px;
    height: 88px;
    border-radius: 50%;
    background: #fef2f2;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 20px;
}

.access-denied-title {
    font-size: 1.5rem;
    font-weight: 800;
    color: var(--slate-800);
    margin-bottom: 10px;
}

.access-denied-text {
    color: var(--slate-500);
    font-size: 0.9375rem;
    line-height: 1.5;
    margin-bottom: 24px;
}

.access-denied-actions {
    display: flex;
    justify-content: center;
}
</style>
