import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useVehicleStore = defineStore("vehicleStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    // ── Vehicle List ──
    getVehiclesList(search, status) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Vehicles/GetVehiclesList?`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
        if (status) url += `status=${encodeURIComponent(status)}&`;
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
    getVehicleDetails(id) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Vehicles/GetVehicleDetails/${id}`)
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
    createVehicle(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Vehicles/CreateVehicle",
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
    updateVehicle(id, payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.put(
          import.meta.env.VITE_API_URL + `/api/Vehicles/UpdateVehicle/${id}`,
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

    toggleVehicleStatus(id) {
      return new Promise((resolve, reject) => {
        API.put(import.meta.env.VITE_API_URL + `/api/Vehicles/ToggleVehicleStatus/${id}`)
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },

    // ── Vehicle Fuel ──
    getVehicleFuelList(search, vehicleId, dateFrom, dateTo) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Vehicles/GetVehicleFuelList?`;
        if (search) url += `search=${encodeURIComponent(search)}&`;
        if (vehicleId) url += `vehicleId=${vehicleId}&`;
        if (dateFrom) url += `dateFrom=${encodeURIComponent(dateFrom)}&`;
        if (dateTo) url += `dateTo=${encodeURIComponent(dateTo)}&`;
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
    createVehicleFuel(payload) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Vehicles/CreateVehicleFuel",
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
    getVehiclesDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Vehicles/GetVehiclesDropdown")
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },
    getStaffDropdown() {
      return new Promise((resolve, reject) => {
        API.get(import.meta.env.VITE_API_URL + "/api/Vehicles/GetStaffDropdown")
          .then((response) => resolve(response))
          .catch((error) => reject(error));
      });
    },

    // ── Vehicle Services ──
    getVehicleServiceList(search) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        let url = import.meta.env.VITE_API_URL + `/api/Vehicles/GetVehicleServiceList?`;
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
  },
});
