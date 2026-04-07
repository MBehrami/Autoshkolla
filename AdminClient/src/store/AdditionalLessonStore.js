import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useAdditionalLessonStore = defineStore("additionalLessonStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getList(search, categoryId, year) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/AdditionalLessons/GetList?`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
        if (categoryId) url += `categoryId=${categoryId}&`;
        if (year) url += `year=${year}&`;
        API.get(url)
          .then((response) => resolve(response))
          .catch((error) => reject(error))
          .finally(() => { this.loading = false; });
      });
    },
    getAvailableYears() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + `/api/AdditionalLessons/GetAvailableYears`)
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },
    getById(id) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/AdditionalLessons/GetById/${id}`)
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
    create(obj) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/AdditionalLessons/Create",
          camelToPascal(obj)
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
    update(id, obj) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/AdditionalLessons/Update/${id}`,
          camelToPascal(obj)
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
    searchCandidatesForLink(search) {
      return new Promise((resolve, reject) => {
        let url = import.meta.env.VITE_API_URL + `/api/AdditionalLessons/SearchCandidatesForLink`;
        if (search) url += `?search=${encodeURIComponent(search)}`;
        API.get(url)
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },
    deleteItem(id) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.delete(
          import.meta.env.VITE_API_URL + `/api/AdditionalLessons/Delete/${id}`
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
