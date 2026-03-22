import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useExamStore = defineStore("examStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    buildExamPayload(objExam) {
      return {
        examCategoryId: objExam.examCategoryId,
        title: objExam.title,
        description: objExam.description ?? "",
        durationMinutes: objExam.durationMinutes,
        passPercent: objExam.passPercent ?? objExam.passPercentage ?? 85,
        sortOrder: objExam.sortOrder ?? 0,
      };
    },

    buildQuestionPayload(objQuestion) {
      return {
        examId: objQuestion.examId,
        text: objQuestion.text,
        imageGuid: objQuestion.imageGuid ?? "",
        sortOrder: objQuestion.sortOrder,
        options: (objQuestion.options || []).map((option, index) => ({
          optionText: option.text ?? "",
          isCorrect: !!option.isCorrect,
          sortOrder: option.sortOrder ?? index,
        })),
      };
    },

    uploadQuestionImage(file) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const formData = new FormData();
        formData.append("file", file);
        
        API.post(
          "/api/Exams/UploadQuestionImage",
          formData,
          {
            headers: {
              "Content-Type": "multipart/form-data",
            },
          }
        )
          .then((response) => {
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },

    // ===== CATEGORIES =====
    getCategoriesAdmin() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(`/api/Exams/GetCategoriesAdmin`)
          .then((response) => {
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    createCategory(objCategory) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          "/api/Exams/CreateCategory",
          camelToPascal(objCategory)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    updateCategory(objCategory) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.put(
          "/api/Exams/UpdateCategory",
          camelToPascal(objCategory)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    toggleCategory(categoryId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          `/api/Exams/ToggleCategory/${categoryId}`
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },

    // ===== EXAMS =====
    getExamsAdmin(categoryCode) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          `/api/Exams/GetExamsAdmin?categoryCode=${categoryCode || ''}`
        )
          .then((response) => {
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    createExam(objExam) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = this.buildExamPayload(objExam);
        API.post(
          "/api/Exams/CreateExam",
          camelToPascal(payload)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    updateExam(objExam) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = this.buildExamPayload(objExam);
        API.patch(
          `/api/Exams/UpdateExam/${objExam.examId}`,
          camelToPascal(payload)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    toggleExam(examId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          `/api/Exams/ToggleExam/${examId}`
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },

    // ===== QUESTIONS =====
    getQuestionsAdmin(examId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          `/api/Exams/GetQuestionsAdmin/${examId || ''}`
        )
          .then((response) => {
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    createQuestion(objQuestion) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = this.buildQuestionPayload(objQuestion);
        API.post(
          "/api/Exams/CreateQuestion",
          camelToPascal(payload)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    updateQuestion(objQuestion) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = this.buildQuestionPayload(objQuestion);
        API.patch(
          `/api/Exams/UpdateQuestion/${objQuestion.examQuestionId}`,
          camelToPascal(payload)
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    toggleQuestion(questionId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          `/api/Exams/ToggleQuestion/${questionId}`
        )
          .then((response) => {
            this.loading = false;
            // Check if the response indicates an error
            if (response.data?.status === 'error') {
              reject(new Error(response.data?.responseMsg || 'Unknown error'));
            } else {
              resolve(response);
            }
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
  },
});
