import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useDrivingSessionStore = defineStore("drivingSessionStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getSessionsByDate(date, status) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/DrivingSessions/GetSessionsByDate?`;
        if (date) url += `date=${encodeURIComponent(date)}&`;
        if (status) url += `status=${encodeURIComponent(status)}&`;
        API.get(url)
          .then((response) => { this.loading = false; resolve(response); })
          .catch((error) => { this.loading = false; reject(error); });
      });
    },

    getSessionsByDateRange(from, to, status) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/DrivingSessions/GetSessionsByDateRange?`;
        if (from) url += `from=${encodeURIComponent(from)}&`;
        if (to) url += `to=${encodeURIComponent(to)}&`;
        if (status) url += `status=${encodeURIComponent(status)}&`;
        API.get(url)
          .then((response) => { this.loading = false; resolve(response); })
          .catch((error) => { this.loading = false; reject(error); });
      });
    },

    getSessionStats(date) {
      return new Promise((resolve, reject) => {
        let url = import.meta.env.VITE_API_URL + `/api/DrivingSessions/GetSessionStats`;
        if (date) url += `?date=${encodeURIComponent(date)}`;
        API.get(url)
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },

    getCandidatesDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/DrivingSessions/GetCandidatesDropdown")
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },

    getVehiclesDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Vehicles/GetVehiclesDropdown")
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },

    createDrivingSession(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/DrivingSessions/CreateDrivingSession",
          camelToPascal(payload)
        )
          .then((response) => { this.loading = false; resolve(response); })
          .catch((error) => { this.loading = false; reject(error); });
      });
    },

    updateDrivingSession(id, payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.put(
          import.meta.env.VITE_API_URL + `/api/DrivingSessions/UpdateDrivingSession/${id}`,
          camelToPascal(payload)
        )
          .then((response) => { this.loading = false; resolve(response); })
          .catch((error) => { this.loading = false; reject(error); });
      });
    },

    deleteDrivingSession(id) {
      return new Promise((resolve, reject) => {
        API.delete(import.meta.env.VITE_API_URL + `/api/DrivingSessions/DeleteDrivingSession?id=${id}`)
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },
  },
});
