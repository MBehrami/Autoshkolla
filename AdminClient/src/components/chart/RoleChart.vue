<template>
    <Pie v-if="loaded" :data="chartData" :options="chartOptions" />
</template>

<script setup>
import { Pie } from 'vue-chartjs';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'
ChartJS.register(ArcElement, Tooltip, Legend)
import { onMounted, ref } from 'vue';
import { useUserStore } from '@/store/UserStore';

const userStore = useUserStore()
const loaded = ref(false)
const chartData = ref(null)
const chartOptions = ref({
    responsive: true,
    maintainAspectRatio: false
})

onMounted(() => {
    userStore.getRoleChart()
        .then((res) => {
            chartData.value = {
                labels: res.data.map(x => x.roleName),
                datasets: [
                    {
                        label: 'Login(Role Wise)',
                        backgroundColor: '#212121',
                        data: res.data.map(x => x.count)
                    }
                ]
            }
            loaded.value = true
        })
})

</script>