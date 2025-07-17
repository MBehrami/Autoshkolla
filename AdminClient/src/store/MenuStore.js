import { defineStore } from "pinia";
import { camelToPascal } from "@/helper/CamelToPascel";
import API from "./API";

export const useMenuStore = defineStore("menuStore", {
  state: () => ({
    loading: false,
  }),
  getters: {},
  actions: {
    getSidebar(roleId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Menu/GetSidebarMenu/${roleId}`
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
    getAllMenuByMenuGroup(menuGroupId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Menu/GetAllMenu/${menuGroupId}`
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
    assignNewMenu(objMenu) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Menu/MenuAssign`,
          camelToPascal(objMenu)
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
    getAllMenu() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Menu/GetMenuList`)
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
    getParentMenus() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Menu/GetParentMenuList`)
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
    getSingleMenu(menuId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL + `/api/Menu/GetSingleMenu/${menuId}`
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
    deleteSingleMenu(menuId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.delete(
          import.meta.env.VITE_API_URL + `/api/Menu/DeleteSingleMenu/${menuId}`
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
    createSingleMenu(objMenu) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Menu/CreateMenu`,
          camelToPascal(objMenu)
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
    updateSingleMenu(objMenu) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Menu/UpdateMenu`,
          camelToPascal(objMenu)
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
    getAllMenuGroup() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(import.meta.env.VITE_API_URL + `/api/Menu/GetMenuGroupList`)
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
    getSingleMenuGroup(menuGroupId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Menu/GetSingleMenuGroup/${menuGroupId}`
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
    deleteSingleMenuGroup(menuGroupId) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.delete(
          import.meta.env.VITE_API_URL +
            `/api/Menu/DeleteSingleMenuGroup/${menuGroupId}`
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
    createSingleMenuGroup(objMenuGroup) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.post(
          import.meta.env.VITE_API_URL + `/api/Menu/CreateMenuGroup`,
          camelToPascal(objMenuGroup)
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
    updateSingleMenuGroup(objMenuGroup) {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.patch(
          import.meta.env.VITE_API_URL + `/api/Menu/UpdateMenuGroup`,
          camelToPascal(objMenuGroup)
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
    getAllMenuMapping() {
      return new Promise((resolve, reject) => {
        this.loading = true;
        API.get(
          import.meta.env.VITE_API_URL +
            `/api/Menu/GetMenuGroupWiseMenuMappingList`
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
