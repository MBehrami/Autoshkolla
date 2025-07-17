import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import axios from "axios";
import API from "./API";

export const useUserStore = defineStore("userStore", {
  state: () => ({
    loading: false,
    visible: localStorage.getItem("visible") || false,
    registrationOpen: false,
    componentKey: 0,
  }),
  getters: {},
  actions: {
    userSignIn(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Users/GetLoginInfo",
          camelToPascal(objUser)
        )
          .then((response) => {
            if (response.status == 200) {
              localStorage.setItem("userId", response.data.obj.userId);
              localStorage.setItem("userRoleId", response.data.obj.userRoleId);
              localStorage.setItem(
                "profile",
                JSON.stringify({ ...response.data })
              );
            }
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            this.loading = false;
            reject(error);
          });
      });
    },
    checkPassword(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Users/GetLoginInfo",
          camelToPascal(objUser)
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
    getUserIp() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        axios
          .get("https://api.ipify.org?format=json")
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
    userInfoForForgetPassword(email) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetUserInfoForForgetPassword/${email}`
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
    createRegistration(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + "/api/Users/StudentRegistration",
          camelToPascal(objUser)
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
    createHistory(objHistory) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Users/CreateLoginHistory`,
          camelToPascal(objHistory)
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
    updateHistory(logCode) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL +
            `/api/Users/UpdateLoginHistory/${logCode}`
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
    getDateChart(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetLogInSummaryByDate/${userId}`
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
    getMonthChart(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetLogInSummaryByMonth/${userId}`
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
    getYearChart(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetLogInSummaryByYear/${userId}`
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
    getBrowserChart(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Users/GetBrowserCount/${userId}`
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
    getRoleChart() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Users/GetRoleWiseUser`)
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
    getUserRoles() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + "/api/Users/GetUserRoleList")
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
    getSingleRole(roleId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Users/GetSingleRole/${roleId}`
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
    deleteSingleRole(roleId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.delete(
          import.meta.env.VITE_API_URL + `/api/Users/DeleteSingleRole/${roleId}`
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
    createSingleRole(objRole) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Users/CreateUserRole`,
          camelToPascal(objRole)
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
    updateSingleRole(objRole) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Users/UpdateUserRole`,
          camelToPascal(objRole)
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
    getUsers() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Users/GetUserList`)
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
    getSingleUser(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Users/GetSingleUser/${userId}`
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
    getSingleUserByRef(passwordRef) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetSingleUserByHash/${passwordRef}`
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
    deleteSingleUser(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.delete(
          import.meta.env.VITE_API_URL + `/api/Users/DeleteSingleUser/${userId}`
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
    createSingleUser(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Users/CreateUser`,
          camelToPascal(objUser)
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
    updateSingleUser(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Users/UpdateUser`,
          camelToPascal(objUser)
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
    updateProfile(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Users/UpdateUserProfile`,
          camelToPascal(objUser)
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
    passwordChange(objUser) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Users/ChangeUserPassword`,
          camelToPascal(objUser)
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
    getUserStatus() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + "/api/Users/UserStatus")
          .then((response) => {
            this.loading = false;
            resolve(response);
          })
          .catch((error) => {
            console.log("error thrown");
            this.loading = false;
            reject(error);
          });
      });
    },
    getBrowseList(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Users/GetBrowseList/${userId}`
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
    getNotifications(userId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Users/GetNotificationList/${userId}`
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
    imageUpload(objFormData) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Users/Upload`,
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
    signOut() {
      this.componentKey += 1;
      this.registrationOpen = false;
      this.visible = false;
      localStorage.clear();
      delete API.defaults.headers.common["Authorization"];
    },
    setRegistration(status) {
      this.registrationOpen = !this.registrationOpen;
    },
    setVisibility(val) {
      this.componentKey += 1;
      this.visible = val;
      localStorage.setItem("visible", val);
    },
  },
});
