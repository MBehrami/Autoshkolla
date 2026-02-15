<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">User Roles</div>
        </div>
        <v-data-table :headers="headers" :items="items" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="roles-toolbar">
                    <div class="roles-actions-wrap">
                        <div class="roles-export-group">
                            <v-btn class="roles-action-btn text-none" variant="outlined" prepend-icon="mdi-file-excel">
                                <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="role_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="roles-action-btn text-none" variant="outlined" prepend-icon="mdi-file-delimited">
                                <download-excel :data="items" :fields="headersExcel" type="csv"
                                    name="role_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="roles-action-btn text-none" variant="outlined" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-dialog v-model="dialog" max-width="800">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Add Role" color="primary" variant="elevated"
                                    prepend-icon="mdi-plus" class="roles-add-btn text-none"></v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveRole">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="roleForm.roleName" label="Name"
                                                        :rules="[rules.required]" variant="underlined" clearable
                                                        required>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="roleForm.roleDesc" label="Description"
                                                        variant="underlined" clearable>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-select v-model="menuGroupSelect" :items="itemsMenuGroup"
                                                        item-title="menuGroupName" item-value="menuGroupID"
                                                        label="Menu Group" variant="underlined"
                                                        :rules="[rules.required]" return-object clearable required>
                                                    </v-select>
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
                <v-icon size="small" class="mr-1" icon="mdi-pencil" @click="editItem(item)">
                </v-icon>
                <v-icon size="small" icon="mdi-delete" @click="deleteItem(item)">
                </v-icon>
            </template>
        </v-data-table>
    </v-container>
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { useMenuStore } from '@/store/MenuStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { resetObject } from '@/helper/ResetObject';
import { computed, ref, watch } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const userStore = useUserStore()
const menuStore = useMenuStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(userStore)
const headers = ref([
    { title: 'Role', key: 'roleName' },
    { title: 'Description', key: 'roleDesc' },
    { title: 'Menu Group', key: 'menuGroupName' },
    { title: 'Actions', key: 'actions', sortable: false }
])
const headersExcel = ref({
    'Id': 'userRoleId',
    'Name': 'roleName',
    'Description': 'roleDesc',
    'Menu Group': 'menuGroupName'
})
const headersPdf = ['Id', 'Name', 'Description', 'Menu Group']
const items = ref([])
const itemsMenuGroup = ref([])
const menuGroupSelect = ref(null)
const dialog = ref(false)
const dialogDelete = ref(false)
const valid = ref(false)
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'Required',
    lengthChk: (v) => (v && v.length >= 3) || 'Name must be greater than or equal to 3 characters'
}
const roleForm = ref({
    userRoleId: '',
    roleName: '',
    roleDesc: '',
    menuGroupName: '',
    menuGroupID: ''
})

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Role', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: items.value.map((row) => [row.userRoleId, row.roleName, row.roleDesc, row.menuGroupName])
    })
    doc.save('role.pdf')
}

//initialize role
const initializeRole = () => {
    userStore.getUserRoles()
        .then((response) => {
            items.value = response.data.data
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

//get user roles
initializeRole()

//edit initialize
const editItem = (item) => {
    menuGroupSelect.value = { menuGroupName: item.menuGroupName, menuGroupID: item.menuGroupID }
    editedIndex.value = items.value.indexOf(item)
    roleForm.value = Object.assign({}, item)
    dialog.value = true
}

//delete dialog
const deleteItem = (item) => {
    roleForm.value = Object.assign({}, item)
    dialogDelete.value = true
}

//delete confirm
const deleteConform = () => {
    userStore.deleteSingleRole(roleForm.value.userRoleId)
        .then((response) => {
            settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            closeDialog()
            initializeRole()
        })
}

//save menu group
const saveRole = () => {
    if (editedIndex.value > -1) {
        //update
        const updateObj = {
            userRoleId: roleForm.value.userRoleId,
            roleName: roleForm.value.roleName,
            roleDesc: roleForm.value.roleDesc,
            menuGroupId: menuGroupSelect.value.menuGroupID,
            lastUpdatedBy: localStorage.getItem('userId'),
        }
        userStore.updateSingleRole(updateObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeRole()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    } else {
        //insert
        const insertObj = {
            roleName: roleForm.value.roleName,
            roleDesc: roleForm.value.roleDesc,
            menuGroupId: menuGroupSelect.value.menuGroupID,
            addedBy: localStorage.getItem('userId'),
        }
        userStore.createSingleRole(insertObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeRole()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    }
}

//close dialog
const closeDialog = () => {
    menuGroupSelect.value = null
    dialogDelete.value = false
    dialog.value = false
    editedIndex.value = -1
    resetObject(roleForm.value)
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
    return editedIndex.value == -1 ? 'Add Role' : 'Edit Role'
})
</script>

<style scoped>
.roles-toolbar {
    padding-inline: 4px;
}

.roles-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.roles-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.roles-action-btn {
    border-radius: 12px;
}

.roles-add-btn {
    margin-left: auto;
    border-radius: 12px;
    min-height: 40px;
}

.roles-toolbar :deep(.v-btn),
.roles-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .roles-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        padding-block: 4px;
    }

    .roles-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .roles-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .roles-add-btn {
        width: 100%;
        margin-left: 0;
        min-height: 44px;
    }
}
</style>
