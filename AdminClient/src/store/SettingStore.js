import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useSettingStore = defineStore("settingStore", {
  state: () => ({
    loading: false,
    isOverlay: false,
    snackbar: false,
    snackbarMsg: "",
    allSettings: [],
  }),
  getters: {},
  actions: {
    fetchSiteSettings() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + "/api/Settings/GetSiteSettings")
          .then((response) => {
            this.loading = false;
            this.allSettings = response.data;
            localStorage.setItem("allSettings", JSON.stringify(response.data));
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    welcomeEmailSent(objRequest) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Settings/SendWelcomeMail",
          camelToPascal(objRequest)
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
    passwordEmailSent(objRequest) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Settings/SendPasswordMail",
          camelToPascal(objRequest)
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
    generalSettingsUpdate(objSettings) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + "/api/Settings/UpdateGeneralSetting",
          camelToPascal(objSettings)
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
    emailSettingsUpdate(objSettings) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + "/api/Settings/UpdateEmailSetting",
          camelToPascal(objSettings)
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
    colorSettingsUpdate(objSettings) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + "/api/Settings/UpdateColorSetting",
          camelToPascal(objSettings)
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
    landingSettingsUpdate(objSettings) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + "/api/Settings/UpdateLandingSetting",
          camelToPascal(objSettings)
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
    emailTextSettingsUpdate(objSettings) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + "/api/Settings/UpdateEmailTextSetting",
          camelToPascal(objSettings)
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
    logoUpload(objFormData) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Settings/UploadLogo",
          objFormData
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
    faviconUpload(objFormData) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Settings/UploadFavicon",
          objFormData
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
    getAllErrors(page = 1, pageSize = 10) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + "/api/Settings/GetErrorLogList", {
          params: { page, pageSize },
        })
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
    createErrorLog(objError) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Settings/CreateErrorLog",
          camelToPascal(objError)
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
    overlayToggle(val) {
      this.isOverlay = val;
    },
    toggleSnackbar(obj) {
      this.snackbar = obj.status;
      this.snackbarMsg = obj.msg;
    },
  },
});
