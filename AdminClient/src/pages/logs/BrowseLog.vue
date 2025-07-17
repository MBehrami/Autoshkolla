<template>
    <v-container>
        <v-data-table :headers="headersBrowse" :items="itemsBrowse" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat>
                    <template v-slot:prepend>
                        <div class="d-flex flex-row ga-1">
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsBrowse"
                                    :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="browse_excel.xlsx">Excel</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsBrowse"
                                    :fields="headersExcel" type="csv" name="browse_csv.xls">Csv</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined" @click.stop="exportPdf">Pdf</v-btn>
                        </div>
                    </template>
                </v-toolbar>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const userStore = useUserStore()
const { loading } = storeToRefs(userStore)
const headersBrowse = ref([
    { title: 'Name', key: 'fullName' },
    { title: 'Email', key: 'email' },
    { title: 'LogIn Time', key: 'logInTime' },
    { title: 'LogOut Time', key: 'logOutTime' },
    { title: 'IP', key: 'ip' },
    { title: 'Browser', key: 'browser' },
    { title: 'Browser Version', key: 'browserVersion' },
    { title: 'OS', key: 'platform' },
])
const headersExcel = ref({
    'Name': 'fullName',
    'Email': 'email',
    'Log In Time': 'logInTime',
    'Log Out Time': 'logOutTime',
    'IP': 'ip',
    'Browser': 'browser',
    'Browser Version': 'browserVersion',
    'OS': 'platform'
})
const headersPdf = ['Name', 'Email', 'Log In Time', 'Log Out Time', 'IP', 'Browser', 'Browser Version', 'OS']
const itemsBrowse = ref([])

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Browsing History', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: itemsBrowse.value.map((row) => [row.fullName, row.email, row.logInTime, row.logOutTime, row.ip, row.browser, row.browserVersion, row.platform])
    })
    doc.save('browse.pdf')
}
//get all browsing history
userStore.getBrowseList(localStorage.getItem('userId'))
    .then((response) => {
        itemsBrowse.value = response.data.data
    })
</script>