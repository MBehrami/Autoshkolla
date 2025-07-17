<template>
    <v-container>
        <v-data-table :headers="headersMenuGroup" :items="itemsMenuGroup" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat>
                    <template v-slot:prepend>
                        <div class="d-flex flex-row ga-1">
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsMenuGroup"
                                    :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="menu_group_excel.xlsx">Excel</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined"><download-excel :data="itemsMenuGroup"
                                    :fields="headersExcel" type="csv"
                                    name="menu_group_csv.xls">Csv</download-excel></v-btn>
                            <v-btn class="text-capitalize" variant="outlined" @click.stop="exportPdf">Pdf</v-btn>
                        </div>
                    </template>
                    <template v-slot:append>
                        <v-dialog v-model="dialog" max-width="550">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Add Menu Group" color="primary" variant="elevated"
                                    class="text-capitalize"></v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveMenuGroup">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12">
                                                    <v-text-field v-model="menuGroupForm.menuGroupName"
                                                        label="Menu Group" :rules="[rules.required, rules.lengthChk]"
                                                        variant="underlined" clearable required>
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
                    </template>
                </v-toolbar>
            </template>
            <template v-slot:[`item.actions`]="{ item }">
                <v-icon size="small" class="mr-1" icon="mdi-pencil" @click="editItem(item)">
                </v-icon>
                <v-icon size="small" icon="mdi-delete" class="mr-1" @click="deleteItem(item)">
                </v-icon>
                <v-icon size="small" icon="mdi-menu" @click="openMenuAssign(item)">
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
const headersExcel = ref({
    'Id': 'menuGroupID',
    'Menu Group': 'menuGroupName'
})
const headersPdf = ['Id', 'Menu Group']
const headersMenuGroup = ref([
    { title: 'Menu Group', key: 'menuGroupName' },
    { title: 'Actions', key: 'actions', sortable: false }
])
const itemsMenuGroup = ref([])
const allMenu = ref([])
const dialog = ref(false)
const dialogDelete = ref(false)
const dialogMenu = ref(false)
const valid = ref(false)
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'Required',
    lengthChk: (v) => (v && v.length >= 3) || 'Name must be greater than or equal to 3 characters'
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
    doc.text('Menu Group', 14, 10)
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
    return editedIndex.value == -1 ? 'Add Menu Group' : 'Edit Menu Group'
})
</script>