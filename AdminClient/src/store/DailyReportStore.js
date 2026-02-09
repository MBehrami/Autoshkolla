import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useDailyReportStore = defineStore("dailyReportStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getEntries(date, month, year, entryType, search) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/DailyReports/GetEntries?`;
        if (date) url += `date=${encodeURIComponent(date)}&`;
        if (month) url += `month=${month}&`;
        if (year) url += `year=${year}&`;
        if (entryType && entryType !== 'All') url += `entryType=${encodeURIComponent(entryType)}&`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
        API.get(url)
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    getDayStatus(date) {
      return new Promise((resolve, reject) => {
        let url = import.meta.env.VITE_API_URL + `/api/DailyReports/GetDayStatus`;
        if (date) url += `?date=${encodeURIComponent(date)}`;
        API.get(url)
          .then((r) => resolve(r))
          .catch((e) => reject(e));
      });
    },

    createEntry(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/DailyReports/CreateEntry",
          camelToPascal(payload)
        )
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    reverseEntry(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/DailyReports/ReverseEntry",
          camelToPascal(payload)
        )
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },

    toggleDayStatus(date) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/DailyReports/ToggleDayStatus?date=${encodeURIComponent(date)}`
        )
          .then((r) => { this.loading = false; resolve(r); })
          .catch((e) => { this.loading = false; reject(e); });
      });
    },
  },
});
