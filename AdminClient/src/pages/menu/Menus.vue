<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Menus</div>
        </div>
        <v-data-table :headers="headers" :items="items" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="menu-toolbar">
                    <div class="menu-actions-wrap">
                        <div class="menu-export-group">
                            <v-btn class="menu-action-btn text-none" variant="outlined" prepend-icon="mdi-file-excel">
                                <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="menu_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="menu-action-btn text-none" variant="outlined" prepend-icon="mdi-file-delimited">
                                <download-excel :data="items" :fields="headersExcel" type="csv"
                                    name="menu_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="menu-action-btn text-none" variant="outlined" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-dialog v-model="dialog" max-width="800">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Add Menu" color="primary" variant="elevated"
                                    prepend-icon="mdi-plus" class="menu-add-btn text-none"></v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveMenu">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="menuForm.menuTitle" label="Name"
                                                        :rules="[rules.required]" variant="underlined" clearable
                                                        required>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-select v-model="parentMenuSelect" :items="itemsParentMenu"
                                                        item-title="text" item-value="id" label="Parent"
                                                        variant="underlined" :rules="[rules.required]"
                                                        @update:model-value="chkOption" return-object clearable
                                                        required>
                                                    </v-select>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="menuForm.url" label="URL"
                                                        variant="underlined">
                                                    </v-text-field>
                                                </v-col>
                                            </v-row>
                                            <v-row v-if="visible">
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="menuForm.sortOrder" label="Order"
                                                        :rules="[rules.required]" variant="underlined" type="number"
                                                        clearable required>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="menuForm.iconClass" label="Icon Class"
                                                        variant="underlined" :rules="[rules.required]" clearable
                                                        required>
                                                    </v-text-field>
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
            <template v-slot:[`item.isSubMenu`]="{ item }">
                <span>{{ subMenuText(item) }}</span>
            </template>
            <template v-slot:[`item.sortOrder`]="{ item }">
                <span>{{ OrderText(item) }}</span>
            </template>
            <template v-slot:[`item.parentMenuName`]="{ item }">
                <span>{{ parentMenuText(item) }}</span>
            </template>
            <template v-slot:[`item.actions`]="{ item }">
                <v-icon size="small" class="mr-1" icon="mdi-pencil" @click="editItem(item)">
                </v-icon>
                <v-icon size="small" icon="mdi-delete" @click="deleteItem(item)">
                </v-icon>
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
const headers = ref([
    { title: 'Title', key: 'menuTitle' },
    { title: 'URL', key: 'url' },
    { title: 'Is Sub Menu?', key: 'isSubMenu' },
    { title: 'Order', key: 'sortOrder' },
    { title: 'Icon Class', key: 'iconClass' },
    { title: 'Parent Menu', key: 'parentMenuName' },
    { title: 'Actions', key: 'actions', sortable: false }
])
const headersExcel = ref({
    'Id': 'menuID',
    'Title': 'menuTitle',
    'URL': 'url',
    'Is Sub Menu?': 'isSubMenu',
    'Menu Order': 'sortOrder',
    'Icon Class': 'iconClass',
    'Parent Menu': 'parentMenuName',
})
const headersPdf = ['Id', 'Title', 'URL', 'Is Sub Menu?', 'Menu Order', 'Icon Class', 'Parent Menu']
const items = ref([])
const itemsParentMenu = ref([])
const parentMenuSelect = ref(null)
const dialog = ref(false)
const dialogDelete = ref(false)
const valid = ref(false)
const visible = ref(false)
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'Required',
    lengthChk: (v) => (v && v.length >= 3) || 'Name must be greater than or equal to 3 characters'
}
const menuForm = ref({
    menuID: '',
    menuTitle: '',
    url: '',
    isSubMenu: '',
    sortOrder: '',
    iconClass: '',
})

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Menu', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: items.value.map((row) => [row.menuID, row.menuTitle, row.url, row.isSubMenu, row.sortOrder, row.iconClass, row.parentMenuName])
    })
    doc.save('menu.pdf')
}

//check root menu
const chkOption = () => {
    //console.log(parentMenuSelect.value)
    if (parentMenuSelect.value == null) {
        visible.value = false
    } else if (parentMenuSelect.value.id == 0) {
        visible.value = true
    } else {
        visible.value = false
    }
}

//Sub Menu text format
const subMenuText = (item) => {
    if (item.isSubMenu == 0) {
        return 'no'
    } else {
        return 'yes'
    }
}

//Order text format
const OrderText = (item) => {
    if (item.sortOrder == 0) {
        return ''
    } else {
        return item.sortOrder
    }
}

//Parent Menu text format
const parentMenuText = (item) => {
    if (item.parentMenuName == null) {
        return 'Root'
    } else {
        return item.parentMenuName
    }
}

//initialize menu
const initilizeMenu = () => {
    menuStore.getAllMenu()
        .then((response) => {
            items.value = response.data.data
        })
}

//initialize parent menu
const initializeParentMenu = () => {
    menuStore.getParentMenus()
        .then((response) => {
            itemsParentMenu.value = response.data
        })
}

//get parent menu
initializeParentMenu()

//get menus
initilizeMenu()

//edit initialize
const editItem = (item) => {
    //console.log(item)
    item.parentID == 0 ? visible.value = true : visible.value = false
    parentMenuSelect.value = { text: item.parentMenuName == null ? 'Root' : item.parentMenuName, id: item.parentID }
    editedIndex.value = items.value.indexOf(item)
    menuForm.value = Object.assign({}, item)
    dialog.value = true
}

//delete dialog
const deleteItem = (item) => {
    menuForm.value = Object.assign({}, item)
    dialogDelete.value = true
}

//delete confirm
const deleteConform = () => {
    menuStore.deleteSingleMenu(menuForm.value.menuID)
        .then((response) => {
            settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            closeDialog()
            initilizeMenu()
        })
}

//save menu
const saveMenu = () => {
    if (editedIndex.value > -1) {
        //update
        const updateObj = {
            menuID: menuForm.value.menuID,
            parentID: parentMenuSelect.value.id,
            menuTitle: menuForm.value.menuTitle,
            url: menuForm.value.url,
            isSubMenu: (menuForm.value.url?.trim().length == 0 && parentMenuSelect.value.id == 0) ? 1 : 0,
            sortOrder: parentMenuSelect.value.id == 0 ? menuForm.value.sortOrder : 0,
            iconClass: menuForm.value.iconClass,
            isActive: false,
            lastUpdatedBy: localStorage.getItem('userId')
        }
        menuStore.updateSingleMenu(updateObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initilizeMenu()
                    initializeParentMenu()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    } else {
        //insert
        const insertObj = {
            parentID: parentMenuSelect.value.id,
            menuTitle: menuForm.value.menuTitle,
            url: menuForm.value.url,
            isSubMenu: (menuForm.value.url?.trim().length == 0 && parentMenuSelect.value.id == 0) ? 1 : 0,
            sortOrder: parentMenuSelect.value.id == 0 ? menuForm.value.sortOrder : 0,
            iconClass: menuForm.value.iconClass,
            isActive: false,
            addedBy: localStorage.getItem('userId')
        }
        //console.log(insertObj)
        menuStore.createSingleMenu(insertObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initilizeMenu()
                    initializeParentMenu()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    }
}

//close dialog
const closeDialog = () => {
    parentMenuSelect.value = null
    dialogDelete.value = false
    dialog.value = false
    editedIndex.value = -1
    resetObject(menuForm.value)
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
    return editedIndex.value == -1 ? 'Add Menu' : 'Edit Menu'
})
</script>

<style scoped>
.menu-toolbar {
    padding-inline: 4px;
}

.menu-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.menu-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.menu-action-btn {
    border-radius: 12px;
}

.menu-add-btn {
    margin-left: auto;
    border-radius: 12px;
    min-height: 40px;
}

.menu-toolbar :deep(.v-btn),
.menu-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .menu-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        padding-block: 4px;
    }

    .menu-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .menu-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .menu-add-btn {
        width: 100%;
        margin-left: 0;
        min-height: 44px;
    }
}
</style>
