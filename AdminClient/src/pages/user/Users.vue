<template>
    <v-container>
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Përdoruesit</div>
        </div>
        <v-data-table :headers="headers" :items="items" :loading="loading" class="elevation-2">
            <template v-slot:top>
                <v-toolbar density="comfortable" flat class="users-toolbar">
                    <div class="users-actions-wrap">
                        <div class="users-export-group">
                            <v-btn class="users-action-btn text-none" variant="outlined" color="success" prepend-icon="mdi-file-excel">
                                <download-excel :data="items" :fields="headersExcel" type="xlsx" worksheet="all-data"
                                    name="user_excel.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn class="users-action-btn text-none" variant="outlined" color="info" prepend-icon="mdi-file-delimited">
                                <download-excel :data="items" :fields="headersExcel" type="csv"
                                    name="user_csv.xls">CSV</download-excel>
                            </v-btn>
                            <v-btn class="users-action-btn text-none" variant="outlined" color="error" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-dialog v-model="dialog" max-width="850">
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" text="Shto Përdorues" color="primary" variant="elevated"
                                    prepend-icon="mdi-plus" class="users-add-btn text-none">
                                </v-btn>
                            </template>
                            <template v-slot:default="{ isActive }">
                                <v-card :title="formTitle">
                                    <v-form v-model="valid" @submit.prevent="saveUser">
                                        <v-card-text>
                                            <v-row>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="userForm.fullName" label="Emri"
                                                        :rules="[rules.required]" variant="underlined" clearable
                                                        required>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-select v-model="roleSelect" :items="itemsRole"
                                                        item-title="roleName" item-value="userRoleId" label="Roli"
                                                        variant="underlined" :rules="[rules.required]" return-object
                                                        clearable required>
                                                    </v-select>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="userForm.mobile" label="Numri i Telefonit"
                                                        variant="underlined" clearable>
                                                    </v-text-field>
                                                </v-col>
                                            </v-row>
                                            <v-row>
                                                <v-col cols="12" md="4">
                                                    <v-text-field v-model="userForm.email" label="Email"
                                                        :rules="[rules.required, rules.emailChk]" variant="underlined"
                                                        clearable required>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4" v-if="editedIndex == -1">
                                                    <v-text-field v-model="userForm.password"
                                                        :rules="[rules.required, rules.passwordChk]" label="Fjalëkalim"
                                                        variant="underlined" type="password" required clearable>
                                                    </v-text-field>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-menu v-model="birthMenu" :close-on-content-click="false"
                                                        :nudge-right="40" transition="scale-transition" offset-y
                                                        min-width="auto">
                                                        <template v-slot:activator="{ props }">
                                                            <v-text-field v-model="userForm.dateOfBirth"
                                                                label="Data e Lindjes" prepend-icon="mdi-calendar" readonly
                                                                v-bind="props" variant="underlined"
                                                                clearable></v-text-field>
                                                        </template>
                                                        <v-date-picker v-model="userForm.dateOfBirth" color="primary"
                                                            @input="birthMenu = false"></v-date-picker>
                                                    </v-menu>
                                                </v-col>
                                            </v-row>
                                            <v-row>
                                                <v-col cols="12" md="4">
                                                    <v-file-input accept="image/*" label="Foto e Profilit"
                                                        prepend-icon="mdi-camera" variant="underlined"
                                                        @update:model-value="onImageChange" show-size>
                                                    </v-file-input>
                                                </v-col>
                                                <v-col cols="12" md="4">
                                                    <v-img :src="imagePreviewSrc" max-height="100" max-width="150"
                                                        contain>
                                                    </v-img>
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
                                <v-card-title class="text-wrap">A jeni i sigurt që dëshironi të fshini këtë element?</v-card-title>
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
</template>

<script setup>
import { useUserStore } from '@/store/UserStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { resetObject } from '@/helper/ResetObject';
import { computed, ref, watch } from 'vue';
import { useDate } from 'vuetify';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const userStore = useUserStore()
const settingStore = useSettingStore()
const { loading } = storeToRefs(userStore)
const date = useDate()
const headers = ref([
    { title: 'Emri', key: 'fullName' },
    { title: 'Roli', key: 'roleName' },
    { title: 'Numri i Telefonit', key: 'mobile' },
    { title: 'Email', key: 'email' },
    { title: 'Data e Lindjes', key: 'dateOfBirth' },
    { title: 'Veprimet', key: 'actions', sortable: false }
])
const headersExcel = ref({
    'Id': 'userId',
    'Emri': 'fullName',
    'Roli': 'roleName',
    'Numri i Telefonit': 'mobile',
    'Email': 'email',
    'Data e Lindjes': 'dateOfBirth',
})
const headersPdf = ['Id', 'Emri', 'Roli', 'Numri i Telefonit', 'Email', 'Data e Lindjes']
const items = ref([])
const itemsRole = ref([])
const roleSelect = ref(null)
const dialog = ref(false)
const dialogDelete = ref(false)
const valid = ref(false)
const birthMenu = ref(false)
const imagePreviewSrc = ref('')
const editedIndex = ref(-1)
const rules = {
    required: (v) => !!v || 'E detyrueshme',
    lengthChk: (v) => (v && v.length >= 3) || 'Emri duhet të jetë më i madh ose i barabartë me 3 karaktere',
    emailChk: (v) => /.+@.+\..+/.test(v) || 'Email duhet të jetë i vlefshëm',
    passwordChk: (v) => (v && v.length >= 6) || 'Fjalëkalimi duhet të jetë më i gjatë se 6 karaktere',
}
const userForm = ref({
    userId: '',
    userRoleId: '',
    roleName: '',
    fullName: '',
    mobile: '',
    email: '',
    imagePath: '',
    dateOfBirth: null,
    password: '',
})

//export to pdf
const exportPdf = () => {
    const doc = new jsPDF({
        orientation: 'landscape'
    })
    doc.text('Përdoruesit', 14, 10)
    autoTable(doc, {
        head: [headersPdf],
        body: items.value.map((row) => [row.userId, row.fullName, row.roleName, row.mobile, row.email, row.dateOfBirth])
    })
    doc.save('user.pdf')
}

//initialize role
const initializeRole = () => {
    userStore.getUserRoles()
        .then((response) => {
            itemsRole.value = response.data.data
        })
}

//initialize users
const initializeUsers = () => {
    userStore.getUsers()
        .then((response) => {
            items.value = response.data.data
        })
}

//get all menu group
initializeUsers()

//get user roles
initializeRole()

//edit initialize
const editItem = (item) => {
    userForm.value = Object.assign({}, item)
    roleSelect.value = { roleName: item.roleName, userRoleId: item.userRoleId }
    userForm.value.dateOfBirth = item.dateOfBirth != null ? new Date(item.dateOfBirth.substr(0, 10)) : null
    imagePreviewSrc.value = item.imagePath != null ? import.meta.env.VITE_API_URL + item.imagePath : ''
    editedIndex.value = items.value.indexOf(item)
    dialog.value = true
}

//delete dialog
const deleteItem = (item) => {
    userForm.value = Object.assign({}, item)
    dialogDelete.value = true
}

//delete confirm
const deleteConform = () => {
    userStore.deleteSingleUser(userForm.value.userId)
        .then((response) => {
            settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            closeDialog()
            initializeUsers()
        })
}

//image change event
const onImageChange = (event) => {
    if (event != null) {
        const fd = new FormData()
        fd.append('image', event)
        userStore.imageUpload(fd)
            .then((res) => {
                if (res.status == 200) {
                    userForm.value.imagePath = '/' + res.data.dbPath
                    const reader = new FileReader()
                    reader.readAsDataURL(event)
                    reader.onload = (event) => {
                        imagePreviewSrc.value = event.target.result
                    }
                }
            })
    }
}

//save user
const saveUser = () => {
    if (editedIndex.value > -1) {
        //update
        const updateObj = {
            userId: userForm.value.userId,
            userRoleId: roleSelect.value.userRoleId,
            fullName: userForm.value.fullName,
            mobile: userForm.value.mobile,
            email: userForm.value.email,
            imagePath: userForm.value.imagePath,
            dateOfBirth: userForm.value.dateOfBirth != null ? date.toISO(userForm.value.dateOfBirth) : null,
            password: userForm.value.password,
            lastUpdatedBy: localStorage.getItem('userId'),
        }
        userStore.updateSingleUser(updateObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeUsers()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    } else {
        //insert
        const insertObj = {
            userRoleId: roleSelect.value.userRoleId,
            fullName: userForm.value.fullName,
            mobile: userForm.value.mobile,
            email: userForm.value.email,
            imagePath: userForm.value.imagePath,
            dateOfBirth: userForm.value.dateOfBirth != null ? date.toISO(userForm.value.dateOfBirth) : null,
            password: userForm.value.password,
            addedBy: localStorage.getItem('userId'),
        }
        userStore.createSingleUser(insertObj)
            .then((response) => {
                if (response.status == 200) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                    closeDialog()
                    initializeUsers()
                } else if (response.status == 202) {
                    settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
                }
            })
    }
}

//close dialog
const closeDialog = () => {
    imagePreviewSrc.value = ''
    roleSelect.value = null
    dialogDelete.value = false
    dialog.value = false
    editedIndex.value = -1
    resetObject(userForm.value)
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
    return editedIndex.value == -1 ? 'Shto Përdorues' : 'Ndrysho Përdorues'
})
</script>

<style scoped>
.users-toolbar {
    padding-inline: 4px;
}

.users-actions-wrap {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
}

.users-export-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.users-action-btn {
    border-radius: 12px;
}

.users-add-btn {
    margin-left: auto;
    border-radius: 12px;
    min-height: 40px;
}

.users-toolbar :deep(.v-btn),
.users-toolbar :deep(.v-field) {
    border-radius: 4px !important;
}

@media (max-width: 600px) {
    .users-actions-wrap {
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
        padding-block: 4px;
    }

    .users-export-group {
        width: 100%;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .users-action-btn {
        width: 100%;
        min-width: 0;
        min-height: 40px;
        padding-inline: 8px;
    }

    .users-add-btn {
        width: 100%;
        margin-left: 0;
        min-height: 44px;
    }
}
</style>
