<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Error Log</div>
        </div>
        <v-data-table
            :headers="headersLog"
            :items="itemsLog"
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
                <v-toolbar density="comfortable" flat class="error-toolbar">
                    <div class="error-actions-wrap">
                        <div class="error-export-group">
                            <v-btn class="error-action-btn text-none" variant="outlined" prepend-icon="mdi-file-excel">
                                <download-excel :data="itemsLog" :fields="headersExcel" type="xlsx"
                                    worksheet="all-data" name="errors_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="error-action-btn text-none" variant="outlined" prepend-icon="mdi-file-delimited">
                                <download-excel :data="itemsLog" :fields="headersExcel" type="csv"
                                    name="errors_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="error-action-btn text-none" variant="outlined" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                    </div>
                </v-toolbar>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';
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
const page = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const pageSizeOptions = [10, 25, 50]

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

const loadErrorLog = () => {
    settingStore.getAllErrors(page.value, pageSize.value)
        .then((response) => {
            itemsLog.value = response.data?.data ?? []
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
    loadErrorLog()
}

const onPageSizeChange = (newPageSize) => {
    const parsed = Number(newPageSize)
    const nextPageSize = pageSizeOptions.includes(parsed) ? parsed : 10
    if (pageSize.value === nextPageSize) return
    pageSize.value = nextPageSize
    page.value = 1
    loadErrorLog()
}

onMounted(() => {
    loadErrorLog()
})
</script>

<style scoped>
.error-toolbar {
    padding-inline: 4px;
}

.error-actions-wrap {
    width: 100%;
}

.error-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.error-action-btn {
    border-radius: 12px;
}

.error-toolbar :deep(.v-btn),
.error-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .error-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .error-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }
}
</style>
