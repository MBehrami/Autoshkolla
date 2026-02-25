<template>
    <v-container v-if="profileInfo.obj.roleName == 'Admin' || profileInfo.obj.roleName == 'SuperAdmin'">
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Faq</div>
        </div>
        <v-data-table :headers="headersFaq" :items="itemsFaq" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="faq-toolbar">
                    <div class="faq-actions-wrap">
                        <div class="faq-export-group">
                            <v-btn class="faq-action-btn text-none" variant="outlined" color="success" prepend-icon="mdi-file-excel">
                                <download-excel :data="itemsFaq" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="faq_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="faq-action-btn text-none" variant="outlined" color="info" prepend-icon="mdi-file-delimited">
                                <download-excel :data="itemsFaq" :fields="headersExcel" type="csv"
                                    name="faq_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="faq-action-btn text-none" variant="outlined" color="error" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-dialog v-model="dialog" max-width="800">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Add Faq" color="primary" variant="elevated"
                                    prepend-icon="mdi-plus" class="faq-add-btn text-none"></v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveFaq">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12">
                                                    <v-text-field v-model="faqForm.title" label="Title"
                                                        :rules="[rules.required]" variant="underlined" clearable
                                                        required>
                                                    </v-text-field>
                                                </v-col>
                                            </v-row>
                                            <v-row>
                                                <v-col cols="12">
                                                    <v-textarea v-model="faqForm.description" label="Description"
                                                        :rules="[rules.required, rules.lengthChk]" variant="underlined"
                                                        auto-grow clearable required>
                                                    </v-textarea>
                                                </v-col>
                                            </v-row>
                                        </v-card-text>
                                        <v-card-actions>
                                            <v-row justify="end">
                                                <v-btn text="Cancel" color="primary" class="text-capitalize"
                                                    @click="isActive.value = false">
                                                </v-btn>
                                                <v-btn :disabled="!valid" :loading="loading" text="Save" type="submit"
                                                    color="grey-darken-4" class="text-capitalize">
                                                </v-btn>
                                            </v-row>
                                        </v-card-actions>
                                    </v-form>
                                </v-card>
                            </template>
                        </v-dialog>
                        <v-dialog v-model="dialogDelete" max-width="330">
                            <v-card>
                                <v-card-title>Are you sure to delete this item?</v-card-title>
                                <v-card-actions>
                                    <v-row justify="end" class="pr-4">
                                        <v-btn text="Cancel" color="primary" class="text-capitalize"
                                            @click="dialogDelete = false">
                                        </v-btn>
                                        <v-btn :loading="loading" text="Delete" color="grey-darken-4"
                                            class="text-capitalize" @click="deleteConform">
                                        </v-btn>
                                    </v-row>
                                </v-card-actions>
                            </v-card>
                        </v-dialog>
                    </div>
                </v-toolbar>
            </template>
            <template v-slot:[`item.actions`]="{ item }">
                <div class="d-flex align-center ga-1">
                    <v-btn icon variant="tonal" color="secondary" size="36" @click="editItem(item)">
                        <v-icon size="20">mdi-pencil</v-icon>
                        <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                    </v-btn>
                    <v-btn icon variant="tonal" color="error" size="36" @click="deleteItem(item)">
                        <v-icon size="20">mdi-delete</v-icon>
                        <v-tooltip activator="parent" location="top">Fshi</v-tooltip>
                    </v-btn>
                </div>
            </template>
        </v-data-table>
    </v-container>
    <v-container v-else>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Faq</div>
        </div>
        <v-expansion-panels>
            <v-expansion-panel v-for="item in itemsFaq" :key="item.faqId" :title="item.title" :text="item.description">
            </v-expansion-panel>
        </v-expansion-panels>
    </v-container>
</template>

<script setup>
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { resetObject } from '@/helper/ResetObject';
import { computed, ref, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const settingStore = useSettingStore()
const { loading } = storeToRefs(settingStore)
const headersFaq = ref([
    { title: 'Title', key: 'title' },
    { title: 'Description', key: 'description' },
    { title: 'Actions', key: 'actions', sortable: false }
])
const headersExcel = ref({
    'Id': 'faqId',
    'Title': 'title',
    'Description': 'description',
})
const headersPdf = ['Id', 'Title', 'Description']
const itemsFaq = ref([])
const dialog = ref(false)
const dialogDelete = ref(false)
const valid = ref(false)
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'Required',
    lengthChk: (v) => (v && v.length >= 10) || 'Name must be greater than or equal to 10 characters'
}
const faqForm = ref({
    faqId: '',
    title: '',
    description: ''
})
const profileInfo = JSON.parse(localStorage.getItem('profile'))

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Faq', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: itemsFaq.value.map((row) => [row.faqId, row.title, row.description])
    })
    doc.save('faq.pdf')
}

//initialize faqs
const initializeFaq = () => {
    settingStore.getAllFaq()
        .then((response) => {
            itemsFaq.value = response.data
        })
}

//get all faq
initializeFaq()

//edit initialize
const editItem = (item) => {
    editedIndex.value = itemsFaq.value.indexOf(item)
    faqForm.value = Object.assign({}, item)
    dialog.value = true
}

//delete dialog
const deleteItem = (item) => {
    faqForm.value = Object.assign({}, item)
    dialogDelete.value = true
}

//delete confirm
const deleteConform = () => {
    settingStore.deleteSingleFaq(faqForm.value.faqId)
        .then((response) => {
            settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            closeDialog()
            initializeFaq()
        })
}

//save faq
const saveFaq = () => {
    if (editedIndex.value > -1) {
        //update faq
        const updateObj = {
            faqId: faqForm.value.faqId,
            title: faqForm.value.title,
            description: faqForm.value.description,
            lastUpdatedBy: localStorage.getItem('userId')
        }
        settingStore.updateSingleFaq(updateObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeFaq()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    } else {
        //insert faq
        const insertObj = {
            title: faqForm.value.title,
            description: faqForm.value.description,
            addedBy: localStorage.getItem('userId')
        }
        settingStore.createSingleFaq(insertObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeFaq()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    }
}

//close dialog
const closeDialog = () => {
    dialogDelete.value = false
    dialog.value = false
    editedIndex.value = -1
    resetObject(faqForm.value)
}

//dialog false execute closeDialog()
watch(dialog, () => {
    dialog.value || closeDialog()
})

//dialogDelete false execute closeDialog()
watch(dialogDelete, () => {
    dialogDelete.value || closeDialog()
})

//get form title
const formTitle = computed(() => {
    return editedIndex.value == -1 ? 'Add Faq' : 'Edit Faq'
})
</script>

<style scoped>
.faq-toolbar {
    padding-inline: 4px;
}

.faq-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.faq-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.faq-action-btn {
    border-radius: 12px;
}

.faq-add-btn {
    margin-left: auto;
    border-radius: 12px;
    min-height: 40px;
}

.faq-toolbar :deep(.v-btn),
.faq-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .faq-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        padding-block: 4px;
    }

    .faq-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .faq-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .faq-add-btn {
        width: 100%;
        margin-left: 0;
        min-height: 44px;
    }
}
</style>
