import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useCandidateAccountStore = defineStore("candidateAccountStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getCandidateAccountsList(search) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/CandidateAccounts/GetList?`;
        if (search) url += `search=${encodeURIComponent(search)}`;
        API.get(url)
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
    getCandidateAccountDetails(candidateAccountId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/CandidateAccounts/GetDetails/${candidateAccountId}`
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
    getUnlinkedCandidates() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/CandidateAccounts/GetUnlinkedCandidates`
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
    getExamCategories() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Exams/GetCategoriesAdmin`)
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
    createCandidateAccount(objAccount) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = {
          candidateId: objAccount.candidateId,
          firstName: objAccount.firstName,
          lastName: objAccount.lastName,
          phoneNumber: objAccount.phoneNumber,
          email: objAccount.email,
          initialPassword: objAccount.password,
          validTo: objAccount.validTo,
          examCategoryIds: objAccount.examCategoryIds || [],
        };
        API.post(
          import.meta.env.VITE_API_URL + "/api/CandidateAccounts/Create",
          camelToPascal(payload)
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
    updateCandidateAccount(objAccount) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = {
          phoneNumber: objAccount.phoneNumber,
          email: objAccount.email,
          validTo: objAccount.validTo,
          examCategoryIds: objAccount.examCategoryIds || [],
        };
        API.patch(
          import.meta.env.VITE_API_URL + `/api/CandidateAccounts/Update/${objAccount.candidateAccountId}`,
          camelToPascal(payload)
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
    toggleActive(candidateAccountId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/CandidateAccounts/ToggleActive/${candidateAccountId}`
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
    setPassword(objPassword) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        const payload = {
          newPassword: objPassword.newPassword,
        };
        API.patch(
          import.meta.env.VITE_API_URL + `/api/CandidateAccounts/SetPassword/${objPassword.candidateAccountId}`,
          camelToPascal(payload)
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
  },
});
