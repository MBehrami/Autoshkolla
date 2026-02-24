<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Browse Log</div>
        </div>
        <v-data-table
            :headers="headersBrowse"
            :items="itemsBrowse"
            :loading="loading"
            :page="page"
            :items-per-page="pageSize"
            :items-length="totalItems"
            :items-per-page-options="pageSizeOptions"
            class="elevation-2"
            @update:page="onPageChange"
            @update:items-per-page="onPageSizeChange"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="browse-toolbar">
                    <div class="browse-actions-wrap">
                        <div class="browse-export-group">
                            <v-btn class="browse-action-btn text-none" variant="outlined" color="success" prepend-icon="mdi-file-excel">
                                <download-excel :data="itemsBrowse" :fields="headersExcel" type="xlsx"
                                    worksheet="all-data" name="browse_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="browse-action-btn text-none" variant="outlined" color="info" prepend-icon="mdi-file-delimited">
                                <download-excel :data="itemsBrowse" :fields="headersExcel" type="csv"
                                    name="browse_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="browse-action-btn text-none" variant="outlined" color="error" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                    </div>
                </v-toolbar>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';
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
const page = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const pageSizeOptions = [10, 25, 50]

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

const loadBrowseList = () => {
    userStore.getBrowseList(localStorage.getItem('userId'), page.value, pageSize.value)
        .then((response) => {
            itemsBrowse.value = response.data?.data ?? []
            totalItems.value = response.data?.recordsTotal ?? 0
            const maxPage = Math.max(1, Math.ceil(totalItems.value / pageSize.value))
            if (page.value > maxPage) {
                page.value = maxPage
            }
        })
}

const onPageChange = (newPage) => {
    if (page.value === newPage) return
    page.value = newPage
    loadBrowseList()
}

const onPageSizeChange = (newPageSize) => {
    const parsed = Number(newPageSize)
    const nextPageSize = pageSizeOptions.includes(parsed) ? parsed : 10
    if (pageSize.value === nextPageSize) return
    pageSize.value = nextPageSize
    page.value = 1
    loadBrowseList()
}

onMounted(() => {
    loadBrowseList()
})
</script>

<style scoped>
.browse-toolbar {
    padding-inline: 4px;
}

.browse-actions-wrap {
    width: 100%;
}

.browse-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.browse-action-btn {
    border-radius: 12px;
}

.browse-toolbar :deep(.v-btn),
.browse-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .browse-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .browse-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }
}
</style>
