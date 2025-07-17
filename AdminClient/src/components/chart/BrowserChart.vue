<template>
    <Bar v-if="loaded" :data="chartData" :options="chartOptions" :style="style" />
</template>

<script setup>
import { Bar } from 'vue-chartjs';
import { Chart as ChartJS, Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale } from 'chart.js'
ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)
import { computed, onMounted, ref } from 'vue';
import { useUserStore } from '@/store/UserStore';

const userStore = useUserStore()
const loaded = ref(false)
const chartData = ref(null)
const chartOptions = ref({
    responsive: true,

})

const style = computed(() => {
    return {
        height: '400px',
        width: '600px',
        position: 'relative'
    }
})

onMounted(() => {
    userStore.getBrowserChart(localStorage.getItem('userId'))
        .then((res) => {
            chartData.value = {
                labels: res.data.map(x => x.browser),
                datasets: [
                    {
                        label: 'Login(Browser Wise)',
                        backgroundColor: '#212121',
                        data: res.data.map(x => x.count)
                    }
                ]
            }
            loaded.value = true
        })
})

</script>