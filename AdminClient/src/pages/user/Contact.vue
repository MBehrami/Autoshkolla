<template>
    <v-container>
        <v-data-table :headers="headersContact" :items="itemsContact" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat>
                    <template v-slot:prepend>
                        <div class="d-flex flex-row ga-1">
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsContact"
                                    :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="contact_excel.xlsx">Excel</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsContact"
                                    :fields="headersExcel" type="csv"
                                    name="contact_csv.xls">Csv</download-excel></v-btn>
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
const headersContact = ref([
    { title: 'Name', key: 'name' },
    { title: 'Email', key: 'email' },
    { title: 'Message', key: 'message' },
])
const headersExcel = ref({
    'Name': 'name',
    'Email': 'email',
    'Message': 'message'
})
const headersPdf = ['Name', 'Email', 'Message']
const itemsContact = ref([])

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Contacts', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: itemsContact.value.map((row) => [row.name, row.email, row.message])
    })
    doc.save('contacts.pdf')
}

//get all contacts
settingStore.getContacts()
    .then((response) => {
        itemsContact.value = response.data
    })
</script>