<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Contact</div>
        </div>
        <v-data-table :headers="headersContact" :items="itemsContact" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="contact-toolbar">
                    <div class="contact-actions-wrap">
                        <div class="contact-export-group">
                            <v-btn class="contact-action-btn text-none" variant="outlined" prepend-icon="mdi-file-excel">
                                <download-excel :data="itemsContact" :fields="headersExcel" type="xlsx"
                                    worksheet="all-data" name="contact_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="contact-action-btn text-none" variant="outlined" prepend-icon="mdi-file-delimited">
                                <download-excel :data="itemsContact" :fields="headersExcel" type="csv"
                                    name="contact_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="contact-action-btn text-none" variant="outlined" prepend-icon="mdi-file-pdf-box"
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

<style scoped>
.contact-toolbar {
    padding-inline: 4px;
}

.contact-actions-wrap {
    width: 100%;
}

.contact-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.contact-action-btn {
    border-radius: 12px;
}

.contact-toolbar :deep(.v-btn),
.contact-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .contact-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .contact-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }
}
</style>
