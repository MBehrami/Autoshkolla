<template>
    <Line v-if="loaded" :data="chartData" :options="chartOptions" />
</template>

<script setup>
import { Line } from 'vue-chartjs';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js'
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)
import { onMounted, ref } from 'vue';
import { useUserStore } from '@/store/UserStore';

const userStore = useUserStore()
const loaded = ref(false)
const chartData = ref(null)
const chartOptions = ref({
    responsive: true,
    maintainAspectRatio: true
})

onMounted(() => {
    userStore.getDateChart(localStorage.getItem('userId'))
        .then((res) => {
            chartData.value = {
                labels: res.data.map(x => x.date.substr(0, 10)),
                datasets: [
                    {
                        label: 'Login(Date Wise)',
                        backgroundColor: '#212121',
                        data: res.data.map(x => x.count)
                    }
                ]
            }
            loaded.value = true
        })
})
</script>