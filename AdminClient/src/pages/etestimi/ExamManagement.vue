<template>
    <div class="exams-container">
        <div class="mb-6">
            <div class="text-h5 font-weight-bold text-grey-darken-3">Menaxhimi i Testimeve</div>
        </div>
        
        <v-tabs v-model="activeTab" color="primary" class="mb-4">
            <v-tab value="categories">Kategoritë</v-tab>
            <v-tab value="exams">Testet</v-tab>
            <v-tab value="questions">Pyetjet</v-tab>
        </v-tabs>

        <!-- CATEGORIES TAB -->
        <v-window v-model="activeTab">
            <v-window-item value="categories">
                <v-card class="elevation-2">
                    <v-card-text>
                        <div class="d-flex justify-end mb-4">
                            <v-btn color="primary" prepend-icon="mdi-plus" @click="openCategoryDialog()">
                                Shto Kategori
                            </v-btn>
                        </div>
                        <v-data-table
                            :headers="categoryHeaders"
                            :items="categories"
                            :loading="loading"
                        >
                            <template v-slot:item.isActive="{ item }">
                                <v-chip :color="item.isActive ? 'success' : 'error'" size="small">
                                    {{ item.isActive ? 'Aktiv' : 'Jo Aktiv' }}
                                </v-chip>
                            </template>
                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-1">
                                    <v-btn icon variant="tonal" color="secondary" size="36" @click="editCategory(item)">
                                        <v-icon size="20">mdi-pencil</v-icon>
                                    </v-btn>
                                    <v-btn icon variant="tonal" :color="item.isActive ? 'error' : 'success'" size="36" @click="toggleCategoryStatus(item)">
                                        <v-icon size="20">{{ item.isActive ? 'mdi-close-circle' : 'mdi-check-circle' }}</v-icon>
                                    </v-btn>
                                </div>
                            </template>
                        </v-data-table>
                    </v-card-text>
                </v-card>
            </v-window-item>

            <!-- EXAMS TAB -->
            <v-window-item value="exams">
                <v-card class="elevation-2">
                    <v-card-text>
                        <div class="d-flex align-center ga-3 mb-4">
                            <v-select
                                v-model="selectedCategoryForExams"
                                :items="categories"
                                item-title="title"
                                item-value="code"
                                label="Zgjedh Kategorinë"
                                variant="outlined"
                                density="compact"
                                style="max-width: 300px;"
                                @update:model-value="loadExams"
                            ></v-select>
                            <v-spacer></v-spacer>
                            <v-btn color="primary" prepend-icon="mdi-plus" @click="openExamDialog()" :disabled="!selectedCategoryForExams">
                                Shto Test
                            </v-btn>
                        </div>
                        <v-data-table
                            :headers="examHeaders"
                            :items="exams"
                            :loading="loading"
                        >
                            <template v-slot:item.isActive="{ item }">
                                <v-chip :color="item.isActive ? 'success' : 'error'" size="small">
                                    {{ item.isActive ? 'Aktiv' : 'Jo Aktiv' }}
                                </v-chip>
                            </template>
                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-1">
                                    <v-btn icon variant="tonal" color="secondary" size="36" @click="editExam(item)">
                                        <v-icon size="20">mdi-pencil</v-icon>
                                    </v-btn>
                                    <v-btn icon variant="tonal" :color="item.isActive ? 'error' : 'success'" size="36" @click="toggleExamStatus(item)">
                                        <v-icon size="20">{{ item.isActive ? 'mdi-close-circle' : 'mdi-check-circle' }}</v-icon>
                                    </v-btn>
                                </div>
                            </template>
                        </v-data-table>
                    </v-card-text>
                </v-card>
            </v-window-item>

            <!-- QUESTIONS TAB -->
            <v-window-item value="questions">
                <v-card class="elevation-2">
                    <v-card-text>
                        <div class="d-flex align-center ga-3 mb-4">
                            <v-select
                                v-model="selectedCategoryForQuestions"
                                :items="categories"
                                item-title="title"
                                item-value="code"
                                label="Zgjedh Kategorinë"
                                variant="outlined"
                                density="compact"
                                style="max-width: 250px;"
                                @update:model-value="loadExamsForQuestions"
                            ></v-select>
                            <v-select
                                v-model="selectedExamForQuestions"
                                :items="examsForQuestions"
                                item-title="title"
                                item-value="examId"
                                label="Zgjedh Testin"
                                variant="outlined"
                                density="compact"
                                style="max-width: 250px;"
                                :disabled="!selectedCategoryForQuestions"
                                @update:model-value="loadQuestions"
                            ></v-select>
                            <v-spacer></v-spacer>
                            <v-btn color="primary" prepend-icon="mdi-plus" @click="openQuestionDialog()" :disabled="!selectedExamForQuestions">
                                Shto Pyetje
                            </v-btn>
                        </div>
                        <v-data-table
                            :headers="questionHeaders"
                            :items="questions"
                            :loading="loading"
                        >
                            <template v-slot:item.isActive="{ item }">
                                <v-chip :color="item.isActive ? 'success' : 'error'" size="small">
                                    {{ item.isActive ? 'Aktiv' : 'Jo Aktiv' }}
                                </v-chip>
                            </template>
                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-1">
                                    <v-btn icon variant="tonal" color="secondary" size="36" @click="editQuestion(item)">
                                        <v-icon size="20">mdi-pencil</v-icon>
                                    </v-btn>
                                    <v-btn icon variant="tonal" :color="item.isActive ? 'error' : 'success'" size="36" @click="toggleQuestionStatus(item)">
                                        <v-icon size="20">{{ item.isActive ? 'mdi-close-circle' : 'mdi-check-circle' }}</v-icon>
                                    </v-btn>
                                </div>
                            </template>
                        </v-data-table>
                    </v-card-text>
                </v-card>
            </v-window-item>
        </v-window>

        <!-- Category Dialog -->
        <v-dialog v-model="categoryDialog" max-width="600" persistent>
            <v-card>
                <v-card-title>{{ editedCategoryIndex === -1 ? 'Shto Kategori' : 'Ndrysho Kategorinë' }}</v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="categoryForm" v-model="categoryFormValid">
                        <v-text-field
                            v-model="editedCategory.code"
                            label="Kodi (A, B, C, D) *"
                            variant="outlined"
                            density="compact"
                            :rules="[v => !!v || 'Vendos kodin']"
                            :disabled="editedCategoryIndex > -1"
                        ></v-text-field>
                        <v-text-field
                            v-model="editedCategory.title"
                            label="Titulli *"
                            variant="outlined"
                            density="compact"
                            :rules="[v => !!v || 'Vendos titullin']"
                        ></v-text-field>
                        <v-textarea
                            v-model="editedCategory.description"
                            label="Përshkrimi"
                            variant="outlined"
                            density="compact"
                            rows="3"
                        ></v-textarea>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey" variant="text" @click="closeCategoryDialog">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" @click="saveCategory" :loading="loading">Ruaj</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Exam Dialog -->
        <v-dialog v-model="examDialog" max-width="600" persistent>
            <v-card>
                <v-card-title>{{ editedExamIndex === -1 ? 'Shto Test' : 'Ndrysho Testin' }}</v-card-title>
                <v-divider></v-divider>
                <v-card-text>
                    <v-form ref="examForm" v-model="examFormValid">
                        <v-text-field
                            v-model="editedExam.title"
                            label="Titulli *"
                            variant="outlined"
                            density="compact"
                            :rules="[v => !!v || 'Vendos titullin']"
                        ></v-text-field>
                        <v-text-field
                            v-model.number="editedExam.durationMinutes"
                            label="Kohëzgjatja (minuta) *"
                            variant="outlined"
                            density="compact"
                            type="number"
                            :rules="[v => v > 0 || 'Vendos kohëzgjatjen']"
                        ></v-text-field>
                        <v-text-field
                            v-model.number="editedExam.passPercentage"
                            label="Përqindja e Kalueshmërisë *"
                            variant="outlined"
                            density="compact"
                            type="number"
                            :rules="[v => v > 0 && v <= 100 || 'Vendos përqindjen (0-100)']"
                        ></v-text-field>
                        <v-text-field
                            v-model.number="editedExam.sortOrder"
                            label="Renditja"
                            variant="outlined"
                            density="compact"
                            type="number"
                        ></v-text-field>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey" variant="text" @click="closeExamDialog">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" @click="saveExam" :loading="loading">Ruaj</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Question Dialog -->
        <v-dialog v-model="questionDialog" max-width="800" persistent>
            <v-card>
                <v-card-title>{{ editedQuestionIndex === -1 ? 'Shto Pyetje' : 'Ndrysho Pyetjen' }}</v-card-title>
                <v-divider></v-divider>
                <v-card-text style="max-height: 600px; overflow-y: auto;">
                    <v-form ref="questionForm" v-model="questionFormValid">
                        <v-textarea
                            v-model="editedQuestion.text"
                            label="Teksti i Pyetjes *"
                            variant="outlined"
                            density="compact"
                            rows="3"
                            :rules="[v => !!v || 'Vendos tekstin e pyetjes']"
                        ></v-textarea>
                        
                        <div class="mb-4">
                            <label class="text-subtitle-2 mb-2 d-block">Fotoja e Pyetjes (opsionale)</label>
                            <div class="d-flex ga-2 align-center">
                                <input 
                                    ref="imageFileInput" 
                                    type="file" 
                                    accept="image/*" 
                                    style="display: none"
                                    @change="handleImageUpload"
                                />
                                <v-btn 
                                    color="primary" 
                                    variant="outlined" 
                                    prepend-icon="mdi-upload"
                                    @click="$refs.imageFileInput?.click()"
                                    :loading="imageUploadLoading"
                                >
                                    Ngarko Fotën
                                </v-btn>
                                <v-btn 
                                    v-if="editedQuestion.imageGuid"
                                    color="error" 
                                    variant="text" 
                                    icon="mdi-delete"
                                    size="small"
                                    @click="clearQuestionImage"
                                >
                                </v-btn>
                            </div>
                            <div v-if="editedQuestion.imageGuid" class="mt-2">
                                <img 
                                    :src="`${apiUrl}/files/${editedQuestion.imageGuid}.jpg`" 
                                    alt="Question Image"
                                    style="max-width: 150px; max-height: 150px; border-radius: 4px;"
                                    @error="handleImageError"
                                />
                            </div>
                        </div>
                        
                        <v-text-field
                            v-model.number="editedQuestion.sortOrder"
                            label="Renditja"
                            variant="outlined"
                            density="compact"
                            type="number"
                        ></v-text-field>
                        
                        <v-divider class="my-4"></v-divider>
                        <div class="text-subtitle-2 mb-3">Opsionet e Përgjigjeve (duhet të ketë saktësisht 1 përgjigje të saktë)</div>
                        
                        <div v-for="(option, index) in editedQuestion.options" :key="index" class="mb-3 pa-3 border rounded">
                            <div class="d-flex align-center ga-2">
                                <v-text-field
                                    v-model="option.text"
                                    :label="`Opsioni ${index + 1} *`"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Vendos tekstin']"
                                ></v-text-field>
                                <v-checkbox
                                    v-model="option.isCorrect"
                                    label="E Saktë"
                                    density="compact"
                                    hide-details
                                    @update:model-value="() => handleCorrectChange(index)"
                                ></v-checkbox>
                                <v-btn icon="mdi-delete" color="error" variant="text" size="small" @click="removeOption(index)" :disabled="editedQuestion.options.length <= 2"></v-btn>
                            </div>
                            <v-text-field
                                v-model.number="option.sortOrder"
                                label="Renditja"
                                variant="outlined"
                                density="compact"
                                type="number"
                                class="mt-2"
                            ></v-text-field>
                        </div>
                        
                        <v-btn color="primary" variant="outlined" prepend-icon="mdi-plus" @click="addOption" block>
                            Shto Opsion
                        </v-btn>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey" variant="text" @click="closeQuestionDialog">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" @click="saveQuestion" :loading="loading">Ruaj</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3000">
            {{ snackbarText }}
        </v-snackbar>
    </div>
</template>

<script setup>
import { useExamStore } from '@/store/ExamStore';
import { storeToRefs } from 'pinia';
import { ref, onMounted } from 'vue';

const examStore = useExamStore();
const { loading } = storeToRefs(examStore);

// API URL for file access
const apiUrl = (import.meta.env.VITE_API_URL || 'http://localhost:5002').replace(/\/$/, '');

const activeTab = ref('categories');

// Categories
const categoryHeaders = [
    { title: 'Kodi', key: 'code' },
    { title: 'Titulli', key: 'title' },
    { title: 'Përshkrimi', key: 'description' },
    { title: 'Statusi', key: 'isActive' },
    { title: 'Veprimet', key: 'actions', sortable: false }
];
const categories = ref([]);
const categoryDialog = ref(false);
const categoryFormValid = ref(false);
const editedCategoryIndex = ref(-1);
const editedCategory = ref({ examCategoryId: 0, code: '', title: '', description: '' });
const defaultCategory = { examCategoryId: 0, code: '', title: '', description: '' };

// Exams
const examHeaders = [
    { title: 'Titulli', key: 'title' },
    { title: 'Kohëzgjatja (min)', key: 'durationMinutes' },
    { title: 'Kalueshmëria (%)', key: 'passPercentage' },
    { title: 'Renditja', key: 'sortOrder' },
    { title: 'Statusi', key: 'isActive' },
    { title: 'Veprimet', key: 'actions', sortable: false }
];
const exams = ref([]);
const examsForQuestions = ref([]);
const selectedCategoryForExams = ref('');
const selectedCategoryForQuestions = ref('');
const selectedExamForQuestions = ref(null);
const examDialog = ref(false);
const examFormValid = ref(false);
const editedExamIndex = ref(-1);
const editedExam = ref({ examId: 0, examCategoryId: 0, title: '', durationMinutes: 45, passPercentage: 85, sortOrder: 0 });
const defaultExam = { examId: 0, examCategoryId: 0, title: '', durationMinutes: 45, passPercentage: 85, sortOrder: 0 };

// Questions
const questionHeaders = [
    { title: 'Teksti', key: 'text' },
    { title: 'Opsionet', key: 'optionsCount' },
    { title: 'Renditja', key: 'sortOrder' },
    { title: 'Statusi', key: 'isActive' },
    { title: 'Veprimet', key: 'actions', sortable: false }
];
const questions = ref([]);
const questionDialog = ref(false);
const questionFormValid = ref(false);
const editedQuestionIndex = ref(-1);
const editedQuestion = ref({
    examQuestionId: 0,
    examId: 0,
    text: '',
    imageGuid: '',
    sortOrder: 0,
    options: [
        { examQuestionOptionId: 0, text: '', isCorrect: true, sortOrder: 0 },
        { examQuestionOptionId: 0, text: '', isCorrect: false, sortOrder: 1 }
    ]
});
const defaultQuestion = {
    examQuestionId: 0,
    examId: 0,
    text: '',
    imageGuid: '',
    sortOrder: 0,
    options: [
        { examQuestionOptionId: 0, text: '', isCorrect: true, sortOrder: 0 },
        { examQuestionOptionId: 0, text: '', isCorrect: false, sortOrder: 1 }
    ]
};

const snackbar = ref(false);
const snackbarText = ref('');
const snackbarColor = ref('success');
const imageUploadLoading = ref(false);
const imageFileInput = ref(null);

// ===== CATEGORIES =====
const loadCategories = async () => {
    try {
        const response = await examStore.getCategoriesAdmin();
        categories.value = response.data?.data || response.data?.obj || [];
    } catch (error) {
        console.error('Error loading categories:', error);
        showSnackbar('Gabim në ngarkimin e kategorive', 'error');
    }
};

const openCategoryDialog = (item = null) => {
    if (item) {
        editedCategoryIndex.value = categories.value.indexOf(item);
        editedCategory.value = { ...item };
    } else {
        editedCategoryIndex.value = -1;
        editedCategory.value = { ...defaultCategory };
    }
    categoryDialog.value = true;
};

const editCategory = (item) => {
    openCategoryDialog(item);
};

const closeCategoryDialog = () => {
    categoryDialog.value = false;
    setTimeout(() => {
        editedCategory.value = { ...defaultCategory };
        editedCategoryIndex.value = -1;
    }, 300);
};

const saveCategory = async () => {
    if (!categoryFormValid.value) return;
    
    try {
        if (editedCategoryIndex.value > -1) {
            await examStore.updateCategory(editedCategory.value);
            showSnackbar('Kategoria u përditësua me sukses', 'success');
        } else {
            await examStore.createCategory(editedCategory.value);
            showSnackbar('Kategoria u krijua me sukses', 'success');
        }
        closeCategoryDialog();
        loadCategories();
    } catch (error) {
        console.error('Error saving category:', error);
        const message = error.response?.data?.responseMsg || error.response?.data?.ResponseMsg || 'Gabim në ruajtjen e kategorisë';
        showSnackbar(message, 'error');
    }
};

const toggleCategoryStatus = async (item) => {
    try {
        await examStore.toggleCategory(item.examCategoryId);
        showSnackbar(`Kategoria u ${item.isActive ? 'çaktivizua' : 'aktivizua'} me sukses`, 'success');
        loadCategories();
    } catch (error) {
        console.error('Error toggling category:', error);
        showSnackbar('Gabim në ndryshimin e statusit', 'error');
    }
};

// ===== EXAMS =====
const loadExams = async () => {
    if (!selectedCategoryForExams.value) {
        exams.value = [];
        return;
    }
    
    try {
        const response = await examStore.getExamsAdmin(selectedCategoryForExams.value);
        exams.value = response.data?.data || response.data?.obj || [];
    } catch (error) {
        console.error('Error loading exams:', error);
        showSnackbar('Gabim në ngarkimin e testeve', 'error');
    }
};

const loadExamsForQuestions = async () => {
    if (!selectedCategoryForQuestions.value) {
        examsForQuestions.value = [];
        selectedExamForQuestions.value = null;
        questions.value = [];
        return;
    }
    
    try {
        const response = await examStore.getExamsAdmin(selectedCategoryForQuestions.value);
        examsForQuestions.value = response.data?.data || response.data?.obj || [];
        selectedExamForQuestions.value = null;
        questions.value = [];
    } catch (error) {
        console.error('Error loading exams:', error);
        showSnackbar('Gabim në ngarkimin e testeve', 'error');
    }
};

const openExamDialog = (item = null) => {
    if (item) {
        editedExamIndex.value = exams.value.indexOf(item);
        editedExam.value = { ...item };
    } else {
        editedExamIndex.value = -1;
        const category = categories.value.find(c => c.code === selectedCategoryForExams.value);
        editedExam.value = { ...defaultExam, examCategoryId: category?.examCategoryId || 0 };
    }
    examDialog.value = true;
};

const editExam = (item) => {
    openExamDialog(item);
};

const closeExamDialog = () => {
    examDialog.value = false;
    setTimeout(() => {
        editedExam.value = { ...defaultExam };
        editedExamIndex.value = -1;
    }, 300);
};

const saveExam = async () => {
    if (!examFormValid.value) return;
    
    try {
        if (editedExamIndex.value > -1) {
            await examStore.updateExam(editedExam.value);
            showSnackbar('Testi u përditësua me sukses', 'success');
        } else {
            await examStore.createExam(editedExam.value);
            showSnackbar('Testi u krijua me sukses', 'success');
        }
        closeExamDialog();
        loadExams();
    } catch (error) {
        console.error('Error saving exam:', error);
        const message = error.response?.data?.responseMsg || error.response?.data?.ResponseMsg || 'Gabim në ruajtjen e testit';
        showSnackbar(message, 'error');
    }
};

const toggleExamStatus = async (item) => {
    try {
        await examStore.toggleExam(item.examId);
        showSnackbar(`Testi u ${item.isActive ? 'çaktivizua' : 'aktivizua'} me sukses`, 'success');
        loadExams();
    } catch (error) {
        console.error('Error toggling exam:', error);
        showSnackbar('Gabim në ndryshimin e statusit', 'error');
    }
};

// ===== QUESTIONS =====
const loadQuestions = async () => {
    if (!selectedExamForQuestions.value) {
        questions.value = [];
        return;
    }
    
    try {
        const response = await examStore.getQuestionsAdmin(selectedExamForQuestions.value);
        questions.value = response.data?.data || response.data?.obj || [];
    } catch (error) {
        console.error('Error loading questions:', error);
        showSnackbar('Gabim në ngarkimin e pyetjeve', 'error');
    }
};

const openQuestionDialog = (item = null) => {
    if (item) {
        editedQuestionIndex.value = questions.value.indexOf(item);
        editedQuestion.value = JSON.parse(JSON.stringify(item));
    } else {
        editedQuestionIndex.value = -1;
        editedQuestion.value = JSON.parse(JSON.stringify({
            ...defaultQuestion,
            examId: selectedExamForQuestions.value
        }));
    }
    questionDialog.value = true;
};

const editQuestion = (item) => {
    openQuestionDialog(item);
};

const closeQuestionDialog = () => {
    questionDialog.value = false;
    setTimeout(() => {
        editedQuestion.value = JSON.parse(JSON.stringify(defaultQuestion));
        editedQuestionIndex.value = -1;
    }, 300);
};

const addOption = () => {
    const newSortOrder = editedQuestion.value.options.length;
    editedQuestion.value.options.push({
        examQuestionOptionId: 0,
        text: '',
        isCorrect: false,
        sortOrder: newSortOrder
    });
};

const removeOption = (index) => {
    if (editedQuestion.value.options.length <= 2) return;
    editedQuestion.value.options.splice(index, 1);
};

const handleCorrectChange = (index) => {
    // Ensure only one option is marked as correct
    editedQuestion.value.options.forEach((opt, i) => {
        if (i !== index) {
            opt.isCorrect = false;
        }
    });
};

const saveQuestion = async () => {
    if (!questionFormValid.value) return;
    
    // Validate exactly one correct answer
    const correctCount = editedQuestion.value.options.filter(o => o.isCorrect).length;
    if (correctCount !== 1) {
        showSnackbar('Duhet të ketë saktësisht 1 përgjigje të saktë', 'error');
        return;
    }
    
    try {
        if (editedQuestionIndex.value > -1) {
            await examStore.updateQuestion(editedQuestion.value);
            showSnackbar('Pyetja u përditësua me sukses', 'success');
        } else {
            await examStore.createQuestion(editedQuestion.value);
            showSnackbar('Pyetja u krijua me sukses', 'success');
        }
        closeQuestionDialog();
        loadQuestions();
    } catch (error) {
        console.error('Error saving question:', error);
        const message = error.message || error.response?.data?.responseMsg || error.response?.data?.ResponseMsg || 'Gabim në ruajtjen e pyetjes';
        showSnackbar(message, 'error');
    }
};

const toggleQuestionStatus = async (item) => {
    try {
        await examStore.toggleQuestion(item.examQuestionId);
        showSnackbar(`Pyetja u ${item.isActive ? 'çaktivizua' : 'aktivizua'} me sukses`, 'success');
        loadQuestions();
    } catch (error) {
        console.error('Error toggling question:', error);
        showSnackbar('Gabim në ndryshimin e statusit', 'error');
    }
};

const handleImageUpload = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file size (5 MB)
    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
        showSnackbar('Fotoja duhet të jetë më e vogël se 5 MB', 'error');
        return;
    }

    // Validate file type
    const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!validTypes.includes(file.type)) {
        showSnackbar('Vetëm fotot (JPEG, PNG, GIF, WebP) janë të lejuara', 'error');
        return;
    }

    try {
        imageUploadLoading.value = true;
        const response = await examStore.uploadQuestionImage(file);
        if (response.data?.status === 'success' && response.data?.imageGuid) {
            editedQuestion.value.imageGuid = response.data.imageGuid;
            showSnackbar('Fotoja u ngarkua me sukses', 'success');
        } else {
            showSnackbar(response.data?.responseMsg || 'Gabim në ngarkimin e fotografisë', 'error');
        }
    } catch (error) {
        console.error('Error uploading image:', error);
        const message = error.response?.data?.responseMsg || 'Gabim në ngarkimin e fotografisë';
        showSnackbar(message, 'error');
    } finally {
        imageUploadLoading.value = false;
        // Clear the input so the same file can be uploaded again
        if (imageFileInput.value) {
            imageFileInput.value.value = '';
        }
    }
};

const clearQuestionImage = () => {
    editedQuestion.value.imageGuid = '';
    showSnackbar('Fotoja u hoq', 'success');
};

const handleImageError = (event) => {
    // Attempt to load with different extensions if primary fails
    const imageGuid = editedQuestion.value.imageGuid;
    const extensions = ['.jpg', '.png', '.gif', '.webp'];
    let currentSrcIndex = 0;
    
    if (event.target.src.includes('.jpg')) {
        currentSrcIndex = 1;
    } else if (event.target.src.includes('.png')) {
        currentSrcIndex = 2;
    } else if (event.target.src.includes('.gif')) {
        currentSrcIndex = 3;
    }
    
    if (currentSrcIndex < extensions.length - 1) {
        event.target.src = `${apiUrl}/files/${imageGuid}${extensions[currentSrcIndex + 1]}`;
    } else {
        console.error('Image load failed for GUID:', imageGuid);
    }
};

const showSnackbar = (text, color = 'success') => {
    snackbarText.value = text;
    snackbarColor.value = color;
    snackbar.value = true;
};

onMounted(() => {
    loadCategories();
});
</script>

<style scoped>
.exams-container {
    padding: 24px;
}
</style>
