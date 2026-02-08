import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useInstructorStore = defineStore("instructorStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getInstructorsList(search) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Instructors/GetInstructorsList?`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
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
    getInstructorDetails(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Instructors/GetInstructorDetails/${userId}`)
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
    createInstructor(obj) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Instructors/CreateInstructor",
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
    updateInstructor(userId, obj) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Instructors/UpdateInstructor/${userId}`,
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
    toggleInstructorActive(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Instructors/ToggleInstructorActive/${userId}`
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
