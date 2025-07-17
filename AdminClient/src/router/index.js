import { createRouter, createWebHistory } from "vue-router";
import Landing from "@/pages/signIn/Landing.vue";
import SignIn from "@/pages/signIn/SignIn.vue";
import PasswordChange from "@/pages/user/PasswordChange.vue";
import Profile from "@/pages/user/Profile.vue";
import Dashboard from "@/pages/dashboard/Dashboard.vue";
import AppSettings from "@/pages/settings/AppSettings.vue";
import NotFound from "@/pages/error/NotFound.vue";
import OtherError from "@/pages/error/OtherError.vue";
import ErrorLog from "@/pages/logs/ErrorLog.vue";
import BrowseLog from "@/pages/logs/BrowseLog.vue";
import Contact from "@/pages/user/Contact.vue";
import Faq from "@/pages/others/Faq.vue";
import Users from "@/pages/user/Users.vue";
import UserRole from "@/pages/user/UserRole.vue";
import Menus from "@/pages/menu/Menus.vue";
import MenuGroup from "@/pages/menu/MenuGroup.vue";
import ResetPassword from "@/pages/user/ResetPassword.vue";

const routes = [
  { path: "/", name: "Landing", component: Landing },
  {
    path: "/signIn",
    name: "SignIn",
    component: SignIn,
    meta: { title: "Sign In" },
  },
  { path: "/dashboard", name: "Dashboard", component: Dashboard },
  {
    path: "/password-change",
    name: "PasswordChange",
    component: PasswordChange,
  },
  { path: "/profile", name: "Profile", component: Profile },
  { path: "/settings", name: "AppSettings", component: AppSettings },
  { path: "/errors", name: "OtherError", component: OtherError },
  { path: "/error-log", name: "ErrorLog", component: ErrorLog },
  { path: "/browse-log", name: "BrowseLog", component: BrowseLog },
  { path: "/contact", name: "Contact", component: Contact },
  { path: "/faq", name: "Faq", component: Faq },
  { path: "/menu", name: "Menus", component: Menus },
  { path: "/menu-group", name: "MenuGroup", component: MenuGroup },
  { path: "/users", name: "Users", component: Users },
  { path: "/user-role", name: "UserRole", component: UserRole },
  {
    path: "/password-reset/:ref",
    name: "ResetPassword",
    component: ResetPassword,
  },
  { path: "/:pathMatch(.*)*", name: "NotFound", component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

const appInitialData = JSON.parse(localStorage.getItem("allSettings"));

router.beforeEach((to, from, next) => {
  next();
});

router.afterEach((to, from, failure) => {
  document.title = to.name + " | " + appInitialData.siteTitle;
});

export default router;
