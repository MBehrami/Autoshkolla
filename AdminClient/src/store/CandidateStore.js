import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useCandidateStore = defineStore("candidateStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getCategories() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Candidates/GetCategories`)
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
    getActiveInstructors() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Candidates/GetActiveInstructors`)
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
    getCandidatesList(search, categoryId, year) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Candidates/GetCandidatesList?`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
        if (categoryId) url += `categoryId=${categoryId}&`;
        if (year) url += `year=${year}&`;
        API.get(url)
          .then((response) => {
            if (typeof console !== 'undefined' && console.log) {
              console.log('candidates raw response (store)', response?.data);
            }
            resolve(response);
          })
          .catch((error) => {
            reject(error);
          })
          .finally(() => {
            this.loading = false;
          });
      });
    },
    getCandidateDetails(candidateId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Candidates/GetCandidateDetails/${candidateId}`)
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
    createCandidate(objCandidate) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Candidates/CreateCandidate",
          camelToPascal(objCandidate)
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
    updateCandidate(candidateId, objCandidate) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Candidates/UpdateCandidate/${candidateId}`,
          camelToPascal(objCandidate)
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
    getAvailableYears() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Candidates/GetAvailableYears`)
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
    getPracticalLessons(candidateId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Candidates/GetPracticalLessons/${candidateId}`)
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
    addPracticalLesson(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Candidates/AddPracticalLesson",
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
