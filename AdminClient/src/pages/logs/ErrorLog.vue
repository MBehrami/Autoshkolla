<template>
    <v-container>
        <v-data-table :headers="headersLog" :items="itemsLog" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat>
                    <template v-slot:prepend>
                        <div class="d-flex flex-row ga-1">
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsLog"
                                    :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="errors_excel.xlsx">Excel</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsLog"
                                    :fields="headersExcel" type="csv" name="errors_csv.xls">Csv</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined" @click.stop="exportPdf">Pdf</v-btn>
                        </div>
                    </template>
                </v-toolbar>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const settingStore = useSettingStore()
const { loading } = storeToRefs(settingStore)
const headersLog = ref([
    { title: 'Date-Time', key: 'dateAdded' },
    { title: 'Error Code', key: 'status' },
    { title: 'Error Name', key: 'statusText' },
    { title: 'URL', key: 'url' },
    { title: 'Message', key: 'message' },
])
const headersExcel = ref({
    'Date Added': 'dateAdded',
    'Error Code': 'status',
    'Error Name': 'statusText',
    'URL': 'url',
    'Message': 'message'
})
const headersPdf = ['Date Added', 'Error Code', 'Error Name', 'URL', 'Message']
const itemsLog = ref([])

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Error Log', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: itemsLog.value.map((row) => [row.dateAdded, row.status, row.statusText, row.url, row.message])
    })
    doc.save('errors.pdf')
}

//get all errors
settingStore.getAllErrors()
    .then((response) => {
        itemsLog.value = response.data
    })
</script>