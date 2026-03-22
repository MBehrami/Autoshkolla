<template>
    <div class="candidate-accounts-container">
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Llogaritë e Kandidatëve (E-Testimi)</div>
        </div>
        <v-data-table
            :headers="headers"
            :items="items"
            :loading="loading"
            class="elevation-2"
        >
            <template v-slot:top>
                <v-toolbar density="comfortable" flat>
                    <div class="d-flex align-center ga-3 flex-wrap w-100">
                        <v-text-field
                            v-model="searchText"
                            label="Kërko (Emri, Telefoni)"
                            variant="outlined"
                            density="compact"
                            hide-details
                            clearable
                            style="max-width: 300px;"
                            @update:model-value="handleSearch"
                        ></v-text-field>
                        <v-spacer></v-spacer>
                        <v-dialog v-model="dialog" max-width="700" scrollable persistent>
                            <template v-slot:activator="{ props: activatorProps }">
                                <v-btn v-bind="activatorProps" color="primary" variant="elevated"
                                    class="text-none" prepend-icon="mdi-plus">
                                    Krijo Llogari
                                </v-btn>
                            </template>
                            <v-card>
                                <v-card-title class="d-flex align-center justify-space-between">
                                    <span>{{ editedIndex === -1 ? 'Krijo Llogari të Re' : 'Ndrysho Llogarinë' }}</span>
                                    <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
                                </v-card-title>
                                <v-divider></v-divider>
                                <v-card-text style="max-height: 600px;">
                                    <v-form ref="form" v-model="formValid">
                                        <v-radio-group
                                            v-if="editedIndex === -1"
                                            v-model="manualMode"
                                            inline
                                            color="primary"
                                            class="mb-2"
                                        >
                                            <v-radio :value="false" label="Zgjedh nga kandidatët"></v-radio>
                                            <v-radio :value="true" label="Shto manualisht"></v-radio>
                                        </v-radio-group>
                                        <v-select
                                            v-if="editedIndex === -1 && !manualMode"
                                            v-model="editedItem.candidateId"
                                            :items="unlinkedCandidates"
                                            :item-title="formatCandidateOption"
                                            item-value="candidateId"
                                            label="Kandidati *"
                                            variant="outlined"
                                            density="compact"
                                            :rules="[v => !!v || 'Zgjedh kandidatin']"
                                        ></v-select>
                                        <template v-if="editedIndex > -1 || manualMode">
                                            <v-text-field
                                                v-model="editedItem.firstName"
                                                label="Emri *"
                                                variant="outlined"
                                                density="compact"
                                                :rules="[v => !!v || 'Vendos emrin']"
                                                :readonly="editedIndex > -1 && !!editedItem.candidateId"
                                            ></v-text-field>
                                            <v-text-field
                                                v-model="editedItem.lastName"
                                                label="Mbiemri *"
                                                variant="outlined"
                                                density="compact"
                                                :rules="[v => !!v || 'Vendos mbiemrin']"
                                                :readonly="editedIndex > -1 && !!editedItem.candidateId"
                                            ></v-text-field>
                                        </template>
                                        <v-text-field
                                            v-model="editedItem.phoneNumber"
                                            label="Numri i telefonit *"
                                            variant="outlined"
                                            density="compact"
                                            :rules="[v => !!v || 'Vendos numrin e telefonit']"
                                        ></v-text-field>
                                        <v-text-field
                                            v-model="editedItem.email"
                                            label="Email (opsionale)"
                                            variant="outlined"
                                            density="compact"
                                            type="email"
                                        ></v-text-field>
                                        <v-select
                                            v-model="editedItem.examCategoryIds"
                                            :items="examCategories"
                                            item-value="examCategoryId"
                                            :item-title="getExamCategoryLabel"
                                            label="Kategoritë e testit *"
                                            variant="outlined"
                                            density="compact"
                                            multiple
                                            chips
                                            closable-chips
                                            :rules="[v => Array.isArray(v) && v.length > 0 || 'Zgjedh të paktën një kategori testi']"
                                        ></v-select>
                                        <v-text-field
                                            v-if="editedIndex === -1"
                                            v-model="editedItem.password"
                                            label="Fjalëkalimi *"
                                            variant="outlined"
                                            density="compact"
                                            type="password"
                                            :rules="[v => !!v || 'Vendos fjalëkalimin']"
                                        ></v-text-field>
                                        <v-menu
                                            v-model="validToMenu"
                                            :close-on-content-click="false"
                                            location="bottom"
                                            transition="scale-transition"
                                            min-width="auto"
                                        >
                                            <template v-slot:activator="{ props }">
                                                <v-text-field
                                                    :model-value="editedItem.validTo"
                                                    label="Vlefshmëria deri më *"
                                                    prepend-inner-icon="mdi-calendar"
                                                    readonly
                                                    variant="outlined"
                                                    density="compact"
                                                    :rules="[v => !!v || 'Vendos datën e vlefshmërisë']"
                                                    v-bind="props"
                                                ></v-text-field>
                                            </template>
                                            <v-date-picker
                                                v-model="validToDateModel"
                                                color="primary"
                                                @update:model-value="handleValidToDateChange"
                                            ></v-date-picker>
                                        </v-menu>
                                    </v-form>
                                </v-card-text>
                                <v-divider></v-divider>
                                <v-card-actions>
                                    <v-spacer></v-spacer>
                                    <v-btn color="grey" variant="text" @click="closeDialog">Anulo</v-btn>
                                    <v-btn color="primary" variant="elevated" @click="saveItem" :loading="loading">Ruaj</v-btn>
                                </v-card-actions>
                            </v-card>
                        </v-dialog>
                    </div>
                </v-toolbar>
            </template>
            <template v-slot:item.isActive="{ item }">
                <v-chip :color="item.isActive ? 'success' : 'error'" size="small">
                    {{ item.isActive ? 'Aktiv' : 'Jo Aktiv' }}
                </v-chip>
            </template>
            <template v-slot:item.validTo="{ item }">
                {{ formatDate(item.validTo) }}
            </template>
            <template v-slot:item.lastLoginDate="{ item }">
                {{ item.lastLoginDate ? formatDateTime(item.lastLoginDate) : 'Asnjëherë' }}
            </template>
            <template v-slot:item.actions="{ item }">
                <div class="d-flex align-center ga-1">
                    <v-btn icon variant="tonal" color="secondary" size="36" @click.stop="editItem(item)">
                        <v-icon size="20">mdi-pencil</v-icon>
                        <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                    </v-btn>
                    <v-btn icon variant="tonal" :color="item.isActive ? 'error' : 'success'" size="36" @click.stop="toggleActiveItem(item)">
                        <v-icon size="20">{{ item.isActive ? 'mdi-close-circle' : 'mdi-check-circle' }}</v-icon>
                        <v-tooltip activator="parent" location="top">{{ item.isActive ? 'Çaktivizo' : 'Aktivizo' }}</v-tooltip>
                    </v-btn>
                    <v-btn icon variant="tonal" color="warning" size="36" @click.stop="resetPassword(item)">
                        <v-icon size="20">mdi-lock-reset</v-icon>
                        <v-tooltip activator="parent" location="top">Ndrysho Fjalëkalimin</v-tooltip>
                    </v-btn>
                </div>
            </template>
        </v-data-table>
        
        <!-- Password Reset Dialog -->
        <v-dialog v-model="passwordDialog" max-width="500" persistent>
            <v-card>
                <v-card-title>Ndrysho Fjalëkalimin</v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="passwordForm" v-model="passwordFormValid">
                        <v-text-field
                            v-model="newPassword"
                            label="Fjalëkalimi i Ri *"
                            variant="outlined"
                            density="compact"
                            type="password"
                            :rules="[v => !!v || 'Vendos fjalëkalimin e ri']"
                        ></v-text-field>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey" variant="text" @click="closePasswordDialog">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" @click="savePassword" :loading="loading">Ruaj</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        
        <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3000">
            {{ snackbarText }}
        </v-snackbar>
    </div>
</template>

<script setup>
import { useCandidateAccountStore } from '@/store/CandidateAccountStore';
import { storeToRefs } from 'pinia';
import { ref, onMounted, watch } from 'vue';

const candidateAccountStore = useCandidateAccountStore();
const { loading } = storeToRefs(candidateAccountStore);

const headers = [
    { title: 'Emri i Plotë', key: 'fullName' },
    { title: 'Telefoni', key: 'phoneNumber' },
    { title: 'Email', key: 'email' },
    { title: 'Kategoritë e Testit', key: 'examCategoryDisplay' },
    { title: 'Statusi', key: 'isActive' },
    { title: 'Vlefshmëria Deri', key: 'validTo' },
    { title: 'Hyrja e Fundit', key: 'lastLoginDate' },
    { title: 'Veprimet', key: 'actions', sortable: false }
];

const items = ref([]);
const searchText = ref('');
const dialog = ref(false);
const validToMenu = ref(false);
const validToDateModel = ref(null);
const manualMode = ref(false);
const passwordDialog = ref(false);
const formValid = ref(false);
const passwordFormValid = ref(false);
const editedIndex = ref(-1);
const editedItem = ref({
    candidateAccountId: 0,
    candidateId: null,
    firstName: '',
    lastName: '',
    phoneNumber: '',
    email: '',
    password: '',
    validTo: '',
    examCategoryIds: [],
});
const defaultItem = {
    candidateAccountId: 0,
    candidateId: null,
    firstName: '',
    lastName: '',
    phoneNumber: '',
    email: '',
    password: '',
    validTo: '',
    examCategoryIds: [],
};
const unlinkedCandidates = ref([]);
const examCategories = ref([]);
const newPassword = ref('');
const passwordCandidateAccountId = ref(0);
const snackbar = ref(false);
const snackbarText = ref('');
const snackbarColor = ref('success');

let searchTimeout = null;

const normalizeCandidate = (candidate) => ({
    candidateId: candidate?.candidateId ?? candidate?.CandidateId ?? null,
    firstName: candidate?.firstName ?? candidate?.FirstName ?? '',
    lastName: candidate?.lastName ?? candidate?.LastName ?? '',
    fullName: candidate?.fullName ?? candidate?.FullName ?? '',
    phoneNumber: candidate?.phoneNumber ?? candidate?.PhoneNumber ?? '',
});

const normalizeAccount = (account) => ({
    candidateAccountId: account?.candidateAccountId ?? account?.CandidateAccountId ?? 0,
    candidateId: account?.candidateId ?? account?.CandidateId ?? null,
    firstName: account?.firstName ?? account?.FirstName ?? '',
    lastName: account?.lastName ?? account?.LastName ?? '',
    fullName: account?.fullName ?? account?.FullName ?? '',
    phoneNumber: account?.phoneNumber ?? account?.PhoneNumber ?? '',
    email: account?.email ?? account?.Email ?? '',
    validTo: account?.validTo ?? account?.ValidTo ?? '',
    isActive: account?.isActive ?? account?.IsActive ?? false,
    lastLoginDate: account?.lastLoginDate ?? account?.LastLoginDate ?? null,
    examCategoryIds: account?.examCategoryIds ?? account?.ExamCategoryIds ?? [],
    examCategories: account?.examCategories ?? account?.ExamCategories ?? [],
    examCategoryDisplay: account?.examCategoryDisplay ?? account?.ExamCategoryDisplay ?? '',
});

const normalizeExamCategory = (category) => ({
    examCategoryId: category?.examCategoryId ?? category?.ExamCategoryId ?? 0,
    code: category?.code ?? category?.Code ?? '',
    title: category?.title ?? category?.Title ?? '',
});

const formatCandidateOption = (candidate) => {
    if (!candidate) return '';
    return candidate.phoneNumber ? `${candidate.fullName} (${candidate.phoneNumber})` : candidate.fullName;
};

const getExamCategoryLabel = (item) => {
    if (!item) return '';
    return item.code ? `${item.code} - ${item.title}` : item.title;
};

const formatDate = (dateStr) => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('sq-AL');
};

const formatDateTime = (dateStr) => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleString('sq-AL');
};

const toIsoDateOnly = (value) => {
    if (!value) return '';
    if (typeof value === 'string') return value.split('T')[0];

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const toDateObject = (value) => {
    if (!value) return null;
    if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value;
    if (typeof value === 'string') {
        const iso = value.split('T')[0];
        const date = new Date(`${iso}T00:00:00`);
        return Number.isNaN(date.getTime()) ? null : date;
    }
    return null;
};

const handleValidToDateChange = (value) => {
    const normalizedDate = toDateObject(value);
    validToDateModel.value = normalizedDate;
    editedItem.value.validTo = toIsoDateOnly(normalizedDate);
    validToMenu.value = false;
};

const handleSearch = () => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        loadAccounts();
    }, 500);
};

const loadAccounts = async () => {
    try {
        const response = await candidateAccountStore.getCandidateAccountsList(searchText.value);
        if (response?.data?.status && response.data.status !== 'success') {
            throw new Error(response.data.responseMsg || response.data.ResponseMsg || 'Gabim në ngarkimin e llogarive');
        }
        const list = response?.data?.data || response?.data?.obj || [];
        items.value = Array.isArray(list) ? list.map(normalizeAccount) : [];
    } catch (error) {
        console.error('Error loading accounts:');
        showSnackbar(error?.message || 'Gabim në ngarkimin e llogarive', 'error');
    }
};

const loadUnlinkedCandidates = async () => {
    try {
        const response = await candidateAccountStore.getUnlinkedCandidates();
        if (response?.data?.status && response.data.status !== 'success') {
            throw new Error(response.data.responseMsg || response.data.ResponseMsg || 'Gabim në ngarkimin e kandidatëve');
        }
        const list = response?.data?.data || response?.data?.obj || [];
        unlinkedCandidates.value = Array.isArray(list)
            ? list.map(normalizeCandidate).filter(x => !!x.candidateId)
            : [];
    } catch (error) {
        console.error('Error loading unlinked candidates:');
        showSnackbar(error?.message || 'Gabim në ngarkimin e kandidatëve', 'error');
    }
};

const loadExamCategories = async () => {
    try {
        const response = await candidateAccountStore.getExamCategories();
        if (response?.data?.status && response.data.status !== 'success') {
            throw new Error(response.data.responseMsg || response.data.ResponseMsg || 'Gabim në ngarkimin e kategorive të testit');
        }
        const list = response?.data?.data || response?.data?.obj || [];
        examCategories.value = Array.isArray(list)
            ? list.map(normalizeExamCategory).filter(x => !!x.examCategoryId && x.code)
            : [];
    } catch (error) {
        console.error('Error loading exam categories:');
        showSnackbar(error?.message || 'Gabim në ngarkimin e kategorive të testit', 'error');
    }
};

const editItem = (item) => {
    editedIndex.value = items.value.indexOf(item);
    editedItem.value = {
        candidateAccountId: item.candidateAccountId,
        candidateId: item.candidateId,
        firstName: item.firstName || '',
        lastName: item.lastName || '',
        phoneNumber: item.phoneNumber,
        email: item.email || '',
        validTo: item.validTo?.split('T')[0] || '',
        examCategoryIds: Array.isArray(item.examCategoryIds) ? [...item.examCategoryIds] : [],
    };
    manualMode.value = !item.candidateId;
    validToDateModel.value = toDateObject(editedItem.value.validTo);
    dialog.value = true;
};

const closeDialog = () => {
    dialog.value = false;
    validToMenu.value = false;
    validToDateModel.value = null;
    manualMode.value = false;
    setTimeout(() => {
        editedItem.value = { ...defaultItem };
        editedIndex.value = -1;
    }, 300);
};

const saveItem = async () => {
    if (!formValid.value) return;
    
    try {
        if (editedIndex.value > -1) {
            await candidateAccountStore.updateCandidateAccount(editedItem.value);
            showSnackbar('Llogaria u përditësua me sukses', 'success');
        } else {
            await candidateAccountStore.createCandidateAccount(editedItem.value);
            showSnackbar('Llogaria u krijua me sukses', 'success');
        }
        closeDialog();
        loadAccounts();
        loadUnlinkedCandidates();
    } catch (error) {
        console.error('Error saving account:');
        const message = error.response?.data?.responseMsg || error.response?.data?.ResponseMsg || 'Gabim në ruajtjen e llogarisë';
        showSnackbar(message, 'error');
    }
};

const toggleActiveItem = async (item) => {
    try {
        await candidateAccountStore.toggleActive(item.candidateAccountId);
        showSnackbar(`Llogaria u ${item.isActive ? 'çaktivizua' : 'aktivizua'} me sukses`, 'success');
        loadAccounts();
    } catch (error) {
        console.error('Error toggling active:');
        showSnackbar('Gabim në ndryshimin e statusit', 'error');
    }
};

const resetPassword = (item) => {
    passwordCandidateAccountId.value = item.candidateAccountId;
    newPassword.value = '';
    passwordDialog.value = true;
};

const closePasswordDialog = () => {
    passwordDialog.value = false;
    newPassword.value = '';
    passwordCandidateAccountId.value = 0;
};

const savePassword = async () => {
    if (!passwordFormValid.value) return;
    
    try {
        await candidateAccountStore.setPassword({
            candidateAccountId: passwordCandidateAccountId.value,
            newPassword: newPassword.value,
        });
        showSnackbar('Fjalëkalimi u ndryshua me sukses', 'success');
        closePasswordDialog();
    } catch (error) {
        console.error('Error setting password:');
        showSnackbar('Gabim në ndryshimin e fjalëkalimit', 'error');
    }
};

const showSnackbar = (text, color = 'success') => {
    snackbarText.value = text;
    snackbarColor.value = color;
    snackbar.value = true;
};

onMounted(() => {
    loadAccounts();
    loadUnlinkedCandidates();
    loadExamCategories();
});

watch(dialog, (isOpen) => {
    if (isOpen && editedIndex.value === -1) {
        validToDateModel.value = toDateObject(editedItem.value.validTo);
        loadUnlinkedCandidates();
    }

    if (isOpen) {
        loadExamCategories();
    }
});

watch(() => editedItem.value.candidateId, (candidateId) => {
    if (editedIndex.value > -1 || manualMode.value || !candidateId) return;
    const selected = unlinkedCandidates.value.find(c => c.candidateId === candidateId);
    if (!selected) return;
    editedItem.value.phoneNumber = selected.phoneNumber || '';
    editedItem.value.firstName = selected.firstName || '';
    editedItem.value.lastName = selected.lastName || '';
});

watch(manualMode, (isManual) => {
    if (editedIndex.value > -1) return;
    if (isManual) {
        editedItem.value.candidateId = null;
    } else {
        editedItem.value.firstName = '';
        editedItem.value.lastName = '';
    }
});
</script>

<style scoped>
.candidate-accounts-container {
    padding: 24px;
}
</style>
