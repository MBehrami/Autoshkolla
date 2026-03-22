<template>
  <header
    class="sticky top-0 w-full bg-white border-b border-gray-200 z-99999 dark:border-gray-800 dark:bg-gray-900"
  >
    <!-- Top bar: Logo + Nav (desktop) + Search + Actions -->
    <div class="flex items-center justify-between px-4 py-3 lg:px-6">
      <!-- Left: Logo + Hamburger (mobile) -->
      <div class="flex items-center gap-4">
        <!-- Mobile hamburger -->
        <button
          @click="toggleMobileNav"
          class="flex items-center justify-center w-10 h-10 text-gray-500 rounded-lg lg:hidden dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800"
        >
          <svg
            v-if="mobileNavOpen"
            class="fill-current"
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              fill-rule="evenodd"
              clip-rule="evenodd"
              d="M6.21967 7.28131C5.92678 6.98841 5.92678 6.51354 6.21967 6.22065C6.51256 5.92775 6.98744 5.92775 7.28033 6.22065L11.999 10.9393L16.7176 6.22078C17.0105 5.92789 17.4854 5.92788 17.7782 6.22078C18.0711 6.51367 18.0711 6.98855 17.7782 7.28144L13.0597 12L17.7782 16.7186C18.0711 17.0115 18.0711 17.4863 17.7782 17.7792C17.4854 18.0721 17.0105 18.0721 16.7176 17.7792L11.999 13.0607L7.28033 17.7794C6.98744 18.0722 6.51256 18.0722 6.21967 17.7794C5.92678 17.4865 5.92678 17.0116 6.21967 16.7187L10.9384 12L6.21967 7.28131Z"
              fill=""
            />
          </svg>
          <svg
            v-else
            width="16"
            height="12"
            viewBox="0 0 16 12"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              fill-rule="evenodd"
              clip-rule="evenodd"
              d="M0.583252 1C0.583252 0.585788 0.919038 0.25 1.33325 0.25H14.6666C15.0808 0.25 15.4166 0.585786 15.4166 1C15.4166 1.41421 15.0808 1.75 14.6666 1.75L1.33325 1.75C0.919038 1.75 0.583252 1.41422 0.583252 1ZM0.583252 11C0.583252 10.5858 0.919038 10.25 1.33325 10.25L14.6666 10.25C15.0808 10.25 15.4166 10.5858 15.4166 11C15.4166 11.4142 15.0808 11.75 14.6666 11.75L1.33325 11.75C0.919038 11.75 0.583252 11.4142 0.583252 11ZM1.33325 5.25C0.919038 5.25 0.583252 5.58579 0.583252 6C0.583252 6.41421 0.919038 6.75 1.33325 6.75L7.99992 6.75C8.41413 6.75 8.74992 6.41421 8.74992 6C8.74992 5.58579 8.41413 5.25 7.99992 5.25L1.33325 5.25Z"
              fill="currentColor"
            />
          </svg>
        </button>

        <!-- Logo -->
        <router-link to="/dashboard" class="flex items-center">
          <img
            class="dark:hidden"
            src="/images/logo/linda_logo.png"
            alt="Logo"
            width="150"
            height="40"
          />
          <img
            class="hidden dark:block"
            src="/images/logo/linda_logo.png"
            alt="Logo"
            width="150"
            height="40"
          />
        </router-link>

        <!-- Desktop navigation -->
        <nav class="hidden lg:flex items-center gap-1 ml-6">
          <div
            v-for="item in flatMenuItems"
            :key="item.name"
            class="relative"
            @mouseenter="item.subItems && openDesktopDropdown(item.name)"
            @mouseleave="item.subItems && closeDesktopDropdown()"
          >
            <!-- Nav item with subitems (dropdown trigger) -->
            <button
              v-if="item.subItems"
              :class="[
                'topnav-item group',
                hasActiveChild(item)
                  ? 'topnav-item-active'
                  : 'topnav-item-inactive',
              ]"
            >
              <span
                :class="[
                  hasActiveChild(item)
                    ? 'menu-item-icon-active'
                    : 'menu-item-icon-inactive',
                ]"
              >
                <component :is="item.icon" />
              </span>
              <span>{{ item.name }}</span>
              <ChevronDownIcon
                :class="[
                  'w-4 h-4 transition-transform duration-200',
                  {
                    'rotate-180 text-brand-500': desktopDropdown === item.name,
                  },
                ]"
              />
            </button>

            <!-- Nav item without subitems (direct link) -->
            <router-link
              v-else-if="item.path"
              :to="item.path"
              :class="[
                'topnav-item group',
                isActive(item.path)
                  ? 'topnav-item-active'
                  : 'topnav-item-inactive',
              ]"
            >
              <span
                :class="[
                  isActive(item.path)
                    ? 'menu-item-icon-active'
                    : 'menu-item-icon-inactive',
                ]"
              >
                <component :is="item.icon" />
              </span>
              <span>{{ item.name }}</span>
            </router-link>

            <!-- Dropdown panel -->
            <transition
              enter-active-class="transition duration-150 ease-out"
              enter-from-class="opacity-0 -translate-y-1"
              enter-to-class="opacity-100 translate-y-0"
              leave-active-class="transition duration-100 ease-in"
              leave-from-class="opacity-100 translate-y-0"
              leave-to-class="opacity-0 -translate-y-1"
            >
              <div
                v-if="item.subItems && desktopDropdown === item.name"
                class="absolute left-0 top-full mt-1 w-56 rounded-xl border border-gray-200 bg-white p-2 shadow-theme-lg dark:border-gray-800 dark:bg-gray-900"
              >
                <router-link
                  v-for="subItem in item.subItems"
                  :key="subItem.name"
                  :to="subItem.path"
                  :class="[
                    'menu-dropdown-item',
                    isActive(subItem.path)
                      ? 'menu-dropdown-item-active'
                      : 'menu-dropdown-item-inactive',
                  ]"
                  @click="closeDesktopDropdown()"
                >
                  {{ subItem.name }}
                  <span class="flex items-center gap-1 ml-auto">
                    <span
                      v-if="subItem.new"
                      :class="[
                        'menu-dropdown-badge',
                        isActive(subItem.path)
                          ? 'menu-dropdown-badge-active'
                          : 'menu-dropdown-badge-inactive',
                      ]"
                    >
                      new
                    </span>
                    <span
                      v-if="subItem.pro"
                      :class="[
                        'menu-dropdown-badge',
                        isActive(subItem.path)
                          ? 'menu-dropdown-badge-active'
                          : 'menu-dropdown-badge-inactive',
                      ]"
                    >
                      pro
                    </span>
                  </span>
                </router-link>
              </div>
            </transition>
          </div>
        </nav>
      </div>

      <!-- Right: Search + Actions -->
      <div class="flex items-center gap-3">
        <ThemeToggler />
        <UserMenu />
      </div>
    </div>

    <!-- Mobile navigation drawer -->
    <transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0 max-h-0"
      enter-to-class="opacity-100 max-h-[80vh]"
      leave-active-class="transition duration-150 ease-in"
      leave-from-class="opacity-100 max-h-[80vh]"
      leave-to-class="opacity-0 max-h-0"
    >
      <div
        v-if="mobileNavOpen"
        class="lg:hidden overflow-y-auto border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900"
      >
        <nav class="p-4 space-y-1">
          <div v-for="(menuGroup, groupIndex) in menuGroups" :key="groupIndex">
            <h2
              class="mb-2 mt-4 text-xs uppercase leading-[20px] text-gray-400 px-3"
            >
              {{ menuGroup.title }}
            </h2>
            <ul class="flex flex-col gap-1">
              <li v-for="(item, index) in menuGroup.items" :key="item.name">
                <!-- Item with subitems -->
                <button
                  v-if="item.subItems"
                  @click="toggleMobileSubmenu(groupIndex, index)"
                  :class="[
                    'menu-item group w-full',
                    isMobileSubmenuOpen(groupIndex, index)
                      ? 'menu-item-active'
                      : 'menu-item-inactive',
                  ]"
                >
                  <span
                    :class="[
                      isMobileSubmenuOpen(groupIndex, index)
                        ? 'menu-item-icon-active'
                        : 'menu-item-icon-inactive',
                    ]"
                  >
                    <component :is="item.icon" />
                  </span>
                  <span class="menu-item-text">{{ item.name }}</span>
                  <ChevronDownIcon
                    :class="[
                      'ml-auto w-5 h-5 transition-transform duration-200',
                      {
                        'rotate-180 text-brand-500': isMobileSubmenuOpen(
                          groupIndex,
                          index
                        ),
                      },
                    ]"
                  />
                </button>

                <!-- Direct link item -->
                <router-link
                  v-else-if="item.path"
                  :to="item.path"
                  :class="[
                    'menu-item group',
                    isActive(item.path)
                      ? 'menu-item-active'
                      : 'menu-item-inactive',
                  ]"
                  @click="closeMobileNav"
                >
                  <span
                    :class="[
                      isActive(item.path)
                        ? 'menu-item-icon-active'
                        : 'menu-item-icon-inactive',
                    ]"
                  >
                    <component :is="item.icon" />
                  </span>
                  <span class="menu-item-text">{{ item.name }}</span>
                </router-link>

                <!-- Mobile submenu -->
                <transition
                  @enter="startTransition"
                  @after-enter="endTransition"
                  @before-leave="startTransition"
                  @after-leave="endTransition"
                >
                  <div
                    v-show="isMobileSubmenuOpen(groupIndex, index)"
                  >
                    <ul class="mt-2 space-y-1 ml-9">
                      <li v-for="subItem in item.subItems" :key="subItem.name">
                        <router-link
                          :to="subItem.path"
                          :class="[
                            'menu-dropdown-item',
                            isActive(subItem.path)
                              ? 'menu-dropdown-item-active'
                              : 'menu-dropdown-item-inactive',
                          ]"
                          @click="closeMobileNav"
                        >
                          {{ subItem.name }}
                          <span class="flex items-center gap-1 ml-auto">
                            <span
                              v-if="subItem.new"
                              :class="[
                                'menu-dropdown-badge',
                                isActive(subItem.path)
                                  ? 'menu-dropdown-badge-active'
                                  : 'menu-dropdown-badge-inactive',
                              ]"
                            >
                              new
                            </span>
                            <span
                              v-if="subItem.pro"
                              :class="[
                                'menu-dropdown-badge',
                                isActive(subItem.path)
                                  ? 'menu-dropdown-badge-active'
                                  : 'menu-dropdown-badge-inactive',
                              ]"
                            >
                              pro
                            </span>
                          </span>
                        </router-link>
                      </li>
                    </ul>
                  </div>
                </transition>
              </li>
            </ul>
          </div>
        </nav>
      </div>
    </transition>
  </header>

  <!-- Mobile backdrop -->
  <div
    v-if="mobileNavOpen"
    class="fixed inset-0 bg-gray-900/50 z-9999 lg:hidden"
    @click="closeMobileNav"
  ></div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import {
  GridIcon,
  ListIcon,
  ChevronDownIcon,
} from "@/icons";
import ThemeToggler from "../common/ThemeToggler.vue";
import UserMenu from "./header/UserMenu.vue";

const route = useRoute();

// --- Menu data (identical to the old sidebar) ---
const menuGroups = [
  {
    title: "Menu",
    items: [
      {
        icon: GridIcon,
        name: "Paneli",
        path: "/dashboard",
      },
      {
        icon: ListIcon,
        name: "Testet",
        path: "/exams",
      },
    ],
  },
];

// Flatten all menu items for the desktop horizontal nav
const flatMenuItems = computed(() =>
  menuGroups.flatMap((group) => group.items)
);

// --- Active states ---
const isActive = (path) => {
  if (path === "/dashboard") {
    return route.path === path;
  }

  return route.path === path || route.path.startsWith(`${path}/`);
};

const hasActiveChild = (item) => {
  if (item.path) return isActive(item.path);
  return item.subItems?.some((sub) => isActive(sub.path));
};

// --- Desktop dropdown ---
const desktopDropdown = ref(null);
let closeTimer = null;

const openDesktopDropdown = (name) => {
  clearTimeout(closeTimer);
  desktopDropdown.value = name;
};

const closeDesktopDropdown = () => {
  closeTimer = setTimeout(() => {
    desktopDropdown.value = null;
  }, 150);
};

// --- Mobile nav ---
const mobileNavOpen = ref(false);
const mobileOpenSubmenu = ref(null);

const toggleMobileNav = () => {
  mobileNavOpen.value = !mobileNavOpen.value;
  if (!mobileNavOpen.value) mobileOpenSubmenu.value = null;
};

const closeMobileNav = () => {
  mobileNavOpen.value = false;
  mobileOpenSubmenu.value = null;
};

const toggleMobileSubmenu = (groupIndex, itemIndex) => {
  const key = `${groupIndex}-${itemIndex}`;
  mobileOpenSubmenu.value = mobileOpenSubmenu.value === key ? null : key;
};

const isAnyMobileSubmenuRouteActive = computed(() => {
  return menuGroups.some((group) =>
    group.items.some(
      (item) =>
        item.subItems && item.subItems.some((subItem) => isActive(subItem.path))
    )
  );
});

const isMobileSubmenuOpen = (groupIndex, itemIndex) => {
  const key = `${groupIndex}-${itemIndex}`;
  return (
    mobileOpenSubmenu.value === key ||
    (isAnyMobileSubmenuRouteActive.value &&
      menuGroups[groupIndex].items[itemIndex].subItems?.some((subItem) =>
        isActive(subItem.path)
      ))
  );
};

// --- Transition helpers (same as original sidebar) ---
const startTransition = (el) => {
  el.style.height = "auto";
  const height = el.scrollHeight;
  el.style.height = "0px";
  el.offsetHeight;
  el.style.height = height + "px";
};

const endTransition = (el) => {
  el.style.height = "";
};
</script>
