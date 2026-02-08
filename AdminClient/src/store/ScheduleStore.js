import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useScheduleStore = defineStore("scheduleStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getEvents(dateFrom, dateTo, instructorId, vehicleId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Schedules/GetEvents?`;
        if (dateFrom) url += `dateFrom=${encodeURIComponent(dateFrom)}&`;
        if (dateTo) url += `dateTo=${encodeURIComponent(dateTo)}&`;
        if (instructorId) url += `instructorId=${instructorId}&`;
        if (vehicleId) url += `vehicleId=${vehicleId}&`;
        API.get(url)
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    getInstructorsDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Schedules/GetInstructorsDropdown")
          .then((r) => resolve(r))
          .catch((e) => reject(e));
      });
    },

    getCandidatesDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Schedules/GetCandidatesDropdown")
          .then((r) => resolve(r))
          .catch((e) => reject(e));
      });
    },

    getVehiclesDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Schedules/GetVehiclesDropdown")
          .then((r) => resolve(r))
          .catch((e) => reject(e));
      });
    },

    createEvent(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Schedules/CreateEvent",
          camelToPascal(payload)
        )
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    updateEvent(id, payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.put(
          import.meta.env.VITE_API_URL + `/api/Schedules/UpdateEvent/${id}`,
          camelToPascal(payload)
        )
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    deleteEvent(id) {
      return new Promise((resolve, reject) => {
        API.delete(import.meta.env.VITE_API_URL + `/api/Schedules/DeleteEvent?id=${id}`)
          .then((r) => resolve(r))
          .catch((e) => reject(e));
      });
    },
  },
});
