import { createRouter, createWebHistory } from "vue-router";
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
import Candidates from "@/pages/candidate/Candidates.vue";
import Instructors from "@/pages/instructor/Instructors.vue";
import Vehicles from "@/pages/vehicle/Vehicles.vue";
import VehicleFuel from "@/pages/vehicle/VehicleFuel.vue";
import VehicleServices from "@/pages/vehicle/VehicleServices.vue";
import DrivingSessions from "@/pages/candidate/DrivingSessions.vue";
import Schedules from "@/pages/schedule/Schedules.vue";
import DailyReport from "@/pages/report/DailyReport.vue";

// Helper: get current user's role name from stored profile
function getRoleName() {
  try {
    const profile = JSON.parse(localStorage.getItem("profile") || "{}");
    return profile?.obj?.roleName || profile?.obj?.RoleName || "";
  } catch {
    return "";
  }
}

// Helper: check if a valid session token exists
function isAuthenticated() {
  try {
    const profile = JSON.parse(localStorage.getItem("profile") || "{}");
    return !!profile?.token;
  } catch {
    return false;
  }
}

const routes = [
  { path: "/", redirect: () => (getRoleName() ? "/dashboard" : "/signIn") },
  {
    path: "/signIn",
    name: "SignIn",
    component: SignIn,
    meta: { title: "Sign In", public: true },
  },
  { path: "/dashboard", name: "Dashboard", component: Dashboard },
  {
    path: "/password-change",
    name: "PasswordChange",
    component: PasswordChange,
  },
  { path: "/profile", name: "Profile", component: Profile },

  // ─── System pages (SuperAdmin only) ───
  { path: "/settings", name: "AppSettings", component: AppSettings, meta: { superAdminOnly: true } },
  { path: "/error-log", name: "ErrorLog", component: ErrorLog, meta: { superAdminOnly: true } },
  { path: "/browse-log", name: "BrowseLog", component: BrowseLog, meta: { superAdminOnly: true } },
  { path: "/menu", name: "Menus", component: Menus, meta: { superAdminOnly: true } },
  { path: "/menu-group", name: "MenuGroup", component: MenuGroup, meta: { superAdminOnly: true } },
  { path: "/users", name: "Users", component: Users, meta: { superAdminOnly: true } },
  { path: "/user-role", name: "UserRole", component: UserRole, meta: { superAdminOnly: true } },

  // ─── Admin + SuperAdmin pages ───
  {
    path: "/driving-sessions",
    name: "DrivingSessions",
    component: DrivingSessions,
    meta: { adminOnly: true },
  },
  {
    path: "/daily-report",
    name: "DailyReport",
    component: DailyReport,
    meta: { adminOnly: true },
  },

  // ─── General pages ───
  { path: "/errors", name: "OtherError", component: OtherError, meta: { public: true } },
  { path: "/contact", name: "Contact", component: Contact },
  { path: "/faq", name: "Faq", component: Faq },
  { path: "/candidates", name: "Candidates", component: Candidates },
  { path: "/instructors", name: "Instructors", component: Instructors },
  { path: "/vehicles", name: "Vehicles", component: Vehicles },
  { path: "/vehicle-fuel", name: "VehicleFuel", component: VehicleFuel },
  { path: "/vehicle-services", name: "VehicleServices", component: VehicleServices },
  { path: "/schedules", name: "Schedules", component: Schedules },
  {
    path: "/password-reset/:ref",
    name: "ResetPassword",
    component: ResetPassword,
    meta: { public: true },
  },
  { path: "/:pathMatch(.*)*", name: "NotFound", component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

const appInitialData = JSON.parse(localStorage.getItem("allSettings")) || {};

router.beforeEach((to, from, next) => {
  const role = getRoleName();
  const isSuperAdmin = role === "SuperAdmin";
  const isAdmin = role === "Admin";

  // Redirect signed-in users away from sign-in page
  if (to.name === "SignIn" && isAuthenticated()) {
    return next({ name: "Dashboard" });
  }

  // Redirect unauthenticated users to sign-in for protected routes
  if (!to.meta?.public && !isAuthenticated()) {
    return next({ name: "SignIn" });
  }

  // SuperAdmin-only routes
  if (to.meta?.superAdminOnly) {
    if (!isSuperAdmin) {
      return next({ name: "Dashboard" });
    }
  }

  // Admin-only routes (Admin OR SuperAdmin)
  if (to.meta?.adminOnly) {
    if (!isSuperAdmin && !isAdmin) {
      return next({ name: "Dashboard" });
    }
  }

  next();
});

router.afterEach((to, from, failure) => {
  const settings = appInitialData ?? JSON.parse(localStorage.getItem("allSettings"));

  // Page title: use site title from settings or fall back to default
  const siteTitle = settings?.siteTitle || "Autoshkolla Linda";
  document.title = `${siteTitle} - ${to.name}`;

  // Dynamic favicon: update to API-uploaded favicon when available, else keep static
  const faviconEl = document.getElementById("favicon");
  if (faviconEl) {
    if (settings?.faviconPath) {
      faviconEl.type = "image/x-icon";
      faviconEl.href = import.meta.env.VITE_API_URL + settings.faviconPath;
    } else {
      faviconEl.type = "image/svg+xml";
      faviconEl.href = "/favicon.svg";
    }
  }
});

export default router;
