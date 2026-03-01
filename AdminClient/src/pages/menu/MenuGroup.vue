<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Grupet e menyve </div>
        </div>
        <v-data-table :headers="headersMenuGroup" :items="itemsMenuGroup" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="menu-group-toolbar">
                    <div class="menu-group-actions-wrap">
                        <div class="menu-group-export-group">
                            <v-btn class="menu-group-action-btn text-none" variant="outlined" color="success" prepend-icon="mdi-file-excel">
                                <download-excel :data="itemsMenuGroup" :fields="headersExcel" type="xlsx"
                                    worksheet="all-data" name="menu_group_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="menu-group-action-btn text-none" variant="outlined" color="info" prepend-icon="mdi-file-delimited">
                                <download-excel :data="itemsMenuGroup" :fields="headersExcel" type="csv"
                                    name="menu_group_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="menu-group-action-btn text-none" variant="outlined" color="error" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-dialog v-model="dialog" max-width="550">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Shto Grup Menuje" color="primary" variant="elevated"
                                    prepend-icon="mdi-plus" class="menu-group-add-btn text-none"></v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveMenuGroup">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12">
                                                    <v-text-field v-model="menuGroupForm.menuGroupName"
                                                        label="Grupi i Menueve" :rules="[rules.required, rules.lengthChk]"
                                                        variant="underlined" clearable required>
                                                    </v-text-field>
                                                </v-col>
                                            </v-row>
                                        </v-card-text>
                                        <v-card-actions>
                                            <v-row justify="end">
                                                <v-btn text="Anulo" color="primary" class="text-capitalize"
                                                    @click="isActive.value = false">
                                                </v-btn>
                                                <v-btn :disabled="!valid" :loading="loading" text="Ruaj" type="submit"
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
                                <v-card-title class="text-wrap">A jeni i sigurt që dëshironi të fshini këtë të dhënë ?</v-card-title>
                                <v-card-actions>
                                    <v-row justify="end" class="pr-4">
                                        <v-btn text="Anulo" color="primary" class="text-capitalize"
                                            @click="dialogDelete = false">
                                        </v-btn>
                                        <v-btn :loading="loading" text="Fshi" color="grey-darken-4"
                                            class="text-capitalize" @click="deleteConform">
                                        </v-btn>
                                    </v-row>
                                </v-card-actions>
                            </v-card>
                        </v-dialog>
                        <v-dialog v-model="dialogMenu" max-width="800" theme="dark">
                            <v-list density="comfortable">
                                <v-list-item v-for="item in allMenu" :key="item.id">
                                    <v-list-item-title v-if="item.children.length == 0" class="pl-4">{{ item.title
                                        }}</v-list-item-title>
                                    <template v-if="item.children.length == 0" v-slot:append>
                                        <v-checkbox-btn class="pr-2" v-model="item.isParentSelected"
                                            @update:model-value="assignNewMenu(item)"></v-checkbox-btn>
                                    </template>
                                    <v-list-group :value="item.title" v-if="item.children.length > 0">
                                        <template v-slot:activator="{ props }">
                                            <v-list-item v-bind="props" :title="item.title"></v-list-item>
                                        </template>
                                        <v-list-item v-for="val in item.children" :key="val.id" :title="val.title"
                                            :value="val.title">
                                            <template v-slot:append>
                                                <v-checkbox-btn v-model="val.isSelected"
                                                    @update:model-value="assignNewMenu(val)"></v-checkbox-btn>
                                            </template>
                                        </v-list-item>
                                    </v-list-group>
                                </v-list-item>
                            </v-list>
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
                    <v-btn icon variant="tonal" color="info" size="36" @click="openMenuAssign(item)">
                        <v-icon size="20">mdi-menu</v-icon>
                        <v-tooltip activator="parent" location="top">Meny</v-tooltip>
                    </v-btn>
                </div>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useMenuStore } from '@/store/MenuStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { resetObject } from '@/helper/ResetObject';
import { computed, ref, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const menuStore = useMenuStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(menuStore)
const headersExcel = ref({
    'Id': 'menuGroupID',
    'Grupi i Menueve': 'menuGroupName'
})
const headersPdf = ['Id', 'Grupi i Menueve']
const headersMenuGroup = ref([
    { title: 'Grupi i Menueve', key: 'menuGroupName' },
    { title: 'Veprimet', key: 'actions', sortable: false }
])
const itemsMenuGroup = ref([])
const allMenu = ref([])
const dialog = ref(false)
const dialogDelete = ref(false)
const dialogMenu = ref(false)
const valid = ref(false)
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'E detyrueshme',
    lengthChk: (v) => (v && v.length >= 3) || 'Emri duhet të jetë më i madh ose i barabartë me 3 karaktere'
}
const menuGroupForm = ref({
    menuGroupID: '',
    menuGroupName: ''
})

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Grupi i Menueve', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: itemsMenuGroup.value.map((row) => [row.menuGroupID, row.menuGroupName])
    })
    doc.save('menu_group.pdf')
}

//menu assign open
const openMenuAssign = (item) => {
    menuStore.getAllMenuByMenuGroup(item.menuGroupID)
        .then((response) => {
            dialogMenu.value = true
            allMenu.value = response.data
        })
}

//assign menu to menu group 
const assignNewMenu = (item) => {
    const obj = {
        menuId: item.id,
        menuGroupId: item.groupId,
        isSelected: item.isParentSelected == undefined ? item.isSelected : item.isParentSelected,
        addedBy: parseInt(localStorage.getItem('userId'))
    }
    menuStore.assignNewMenu(obj)
        .then((response) => {
            if (response.status == 200) {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            }
        })
}

//initialize menu group
const initializeMenuGroup = () => {
    menuStore.getAllMenuGroup()
        .then((response) => {
            itemsMenuGroup.value = response.data.data
        })
}

//get all menu group
initializeMenuGroup()

//edit initialize
const editItem = (item) => {
    editedIndex.value = itemsMenuGroup.value.indexOf(item)
    menuGroupForm.value = Object.assign({}, item)
    dialog.value = true
}

//delete dialog
const deleteItem = (item) => {
    menuGroupForm.value = Object.assign({}, item)
    dialogDelete.value = true
}

//delete confirm
const deleteConform = () => {
    menuStore.deleteSingleMenuGroup(menuGroupForm.value.menuGroupID)
        .then((response) => {
            settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            closeDialog()
            initializeMenuGroup()
        })
}

//save menu group
const saveMenuGroup = () => {
    if (editedIndex.value > -1) {
        //update
        const updateObj = {
            menuGroupID: menuGroupForm.value.menuGroupID,
            menuGroupName: menuGroupForm.value.menuGroupName,
            lastUpdatedBy: localStorage.getItem('userId')
        }
        menuStore.updateSingleMenuGroup(updateObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeMenuGroup()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    } else {
        //insert
        const insertObj = {
            menuGroupName: menuGroupForm.value.menuGroupName,
            addedBy: localStorage.getItem('userId')
        }
        menuStore.createSingleMenuGroup(insertObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeMenuGroup()
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
    resetObject(menuGroupForm.value)
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
    return editedIndex.value == -1 ? 'Shto Grup Menyje' : 'Ndrysho Grup Menyje'
})
</script>

<style scoped>
.menu-group-toolbar {
    padding-inline: 4px;
}

.menu-group-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.menu-group-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.menu-group-action-btn {
    border-radius: 12px;
}

.menu-group-add-btn {
    margin-left: auto;
    border-radius: 12px;
    min-height: 40px;
}

.menu-group-toolbar :deep(.v-btn),
.menu-group-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .menu-group-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        padding-block: 4px;
    }

    .menu-group-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .menu-group-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .menu-group-add-btn {
        width: 100%;
        margin-left: 0;
        min-height: 44px;
    }
}
</style>
