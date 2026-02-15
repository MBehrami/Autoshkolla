<template>
    <v-navigation-drawer v-model="drawer" class="sidebar-drawer" elevation="1" :width="smAndDown ? 280 : 300">
        <template v-if="smAndDown" v-slot:append>
            <div class="d-flex justify-center py-2">
                <v-btn text="Sign Out" variant="text" prepend-icon="mdi-logout" class="text-capitalize"
                    style="color: #546E7A;" @click.stop="dialogSignout = true">
                </v-btn>
            </div>
        </template>
        <v-list class="sidebar-profile-list">
            <v-list-item>
                <template v-slot:prepend>
                    <v-avatar><v-img :src="profileImage"></v-img></v-avatar>
                </template>
                <template v-slot:title>
                    <div class="sidebar-profile-name">{{ profileInfo.obj.fullName }}</div>
                </template>
                <template v-slot:subtitle>
                    <div class="sidebar-profile-role">{{ profileInfo.obj.roleName }}</div>
                </template>
            </v-list-item>
        </v-list>
        <v-divider></v-divider>
        <v-list density="default" class="sidebar-menu-list">
            <template v-for="item in menus" :key="item.id">
                <v-list-item v-if="!(item.childItems && item.childItems.length)" :to="item.route"
                    @click="item.route && smAndDown && (drawer = false)"
                    active-class="sidebar-active-item">
                    <v-list-item-title class="sidebar-item-title">{{ item.title }}</v-list-item-title>
                    <template v-slot:prepend>
                        <v-icon class="sidebar-item-icon">{{ item.icon }}</v-icon>
                    </template>
                </v-list-item>

                <v-list-group v-else :value="item.title">
                    <template v-slot:activator="{ props }">
                        <v-list-item v-bind="props" active-class="sidebar-active-item">
                            <template v-slot:prepend>
                                <v-icon class="sidebar-item-icon">{{ item.icon }}</v-icon>
                            </template>
                            <v-list-item-title class="sidebar-item-title">{{ item.title }}</v-list-item-title>
                        </v-list-item>
                    </template>
                    <v-list-item v-for="val in item.childItems" :key="val.id" :to="val.route"
                        @click="val.route && smAndDown && (drawer = false)"
                        active-class="sidebar-active-item">
                        <v-list-item-title class="sidebar-item-title">{{ val.title }}</v-list-item-title>
                    </v-list-item>
                </v-list-group>
            </template>
        </v-list>
    </v-navigation-drawer>
    <v-app-bar elevation="1" class="app-top-bar">
        <template v-slot:prepend>
            <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        </template>
        <template v-slot:append>
            <v-btn icon="mdi-lock" size="small" @click.stop="dialogLock = true"></v-btn>
            <v-btn @click.stop="toggleFullScreen" size="small"
                :icon="props.isFullscreen ? 'mdi-fullscreen-exit' : 'mdi-fullscreen'"></v-btn>
                        <v-menu>
                <template v-slot:activator="{ props }">
                    <v-btn v-bind="props" size="small">
                        <v-badge :content="notifications.recordsTotal">
                            <v-icon icon="mdi-bell" size="large"></v-icon>
                        </v-badge>
                    </v-btn>
                </template>
                <v-list style="max-height: 300px" class="overflow-y-auto">
                    <v-list-item v-for="(item, i) in notifications.data" :key="i" :value="item">
                        <template v-slot:prepend>
                            <v-icon icon="mdi-arrow-right-box"></v-icon>
                        </template>
                        <v-list-item-title>Login Time:{{ item.logInTime }}, IP:{{ item.ip }}, Browser:{{ item.browser }},
                            OS:{{ item.platform }}</v-list-item-title>
                    </v-list-item>
                </v-list>
            </v-menu><v-menu>
                <template v-slot:activator="{ props }">
                    <v-btn text="Personalize" class="text-capitalize" v-bind="props" size="small">
                        <template v-slot:prepend>
                            <v-icon icon="mdi-menu-down" size="large"></v-icon>
                        </template>
                    </v-btn>
                </template>
                <v-list>
                    <v-list-item v-for="(item, i) in personalizeSelect" :key="i" :value="item" :to="item.route">
                        <v-list-item-title v-text="item.text"></v-list-item-title>
                    </v-list-item>
                </v-list>
            </v-menu>
            <v-btn v-if="mdAndUp" icon="mdi-logout" size="small" @click.stop="dialogSignout = true"></v-btn>
        </template>
    </v-app-bar>
        <v-dialog v-model="dialogSignout" max-width="320" persistent>
        <v-card rounded="lg">
            <v-card-title class="text-h6 pt-4">Want to leave?</v-card-title>
            <v-card-text class="py-2 text-body-2 text-medium-emphasis">Press Sign Out to leave</v-card-text>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn variant="text" class="text-capitalize" @click.stop="dialogSignout = false">Stay Here</v-btn>
                <v-btn variant="flat" color="error" class="text-capitalize" @click.stop="signOut">Sign Out</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
    <v-dialog v-model="dialogLock" max-width="320" persistent>
        <v-card rounded="lg">
            <v-card-title class="d-flex justify-center pt-4"><v-avatar><v-img
                        :src="profileImage"></v-img></v-avatar></v-card-title>
            <v-card-text>
                <v-form v-model="valid" @submit.prevent="lockOpen">
                    <v-text-field v-model="signInForm.password" variant="outlined" type="password"
                        :rules="passwordRules" required></v-text-field>
                    <div class="d-flex justify-center"><v-btn :disabled="!valid" size="x-large" type="submit"
                            icon="mdi-key" variant="text"></v-btn></div>
                </v-form>
            </v-card-text>
        </v-card>
    </v-dialog>
</template>

<script setup>
import defaultImg from '@/assets/pp2.png'
import { useRouter } from 'vue-router';
import { useUserStore } from '@/store/UserStore';
import { useMenuStore } from '@/store/MenuStore';
import { useSettingStore } from '@/store/SettingStore';
import { useDisplay } from 'vuetify';
import { computed, ref } from 'vue';

const props = defineProps(['isFullscreen'])
const emits = defineEmits(['toggle-screen'])
const { smAndDown, mdAndUp } = useDisplay()
const dialogSignout = ref(false)
const dialogLock = ref(false)
const drawer = ref(true)
const menus = ref([])
const notifications = ref({})
const router = useRouter()
const userStore = useUserStore()
const menuStore = useMenuStore()
const settingStore = useSettingStore()
const valid = ref(false)
const passwordRules = [
    (v) => !!v || 'Password is required',
    (v) => (v && v.length >= 6) || 'Password must be more than 6 characters',
]
const signInForm = ref({
    email: '',
    password: ''
})
const linksAdmin = ref([
    { text: 'Profile', route: '/profile' },
    { text: 'Password', route: '/password-change' }
])
const linksOthers = ref([
    { text: 'Profile', route: '/profile' },
    { text: 'Password', route: '/password-change' },
    { text: 'Faqs', route: '/faq' }
])
const profileInfo = JSON.parse(localStorage.getItem('profile') || '{}')

// ─── Role helpers ───
const roleId = localStorage.getItem('userRoleId')
const roleName = (typeof profileInfo?.obj?.roleName === 'string') ? profileInfo.obj.roleName
    : (typeof profileInfo?.obj?.RoleName === 'string') ? profileInfo.obj.RoleName : ''
const isSuperAdmin = roleName === 'SuperAdmin'
const isAdmin = roleId === '1' || roleName === 'Admin'
const isInstructor = roleId === '3' || roleName === 'Instructor'
const isAdminLevel = isSuperAdmin || isAdmin  // either Admin or SuperAdmin

//open lock
const lockOpen = () => {
    const obj = {
        ...signInForm.value,
        email: profileInfo.obj.email
    }
    userStore.checkPassword(obj)
        .then(response => {
            if (response.status == 200) {
                dialogLock.value = false
            } else if (response.status == 202) {
                settingStore.toggleSnackbar({ status: true, msg: response.data.responseMsg })
            }
        })
        .catch(error => {
            console.log('error', error)
        })
}

//personalize menu selection depend on role
const personalizeSelect = computed(() => {
    return isAdminLevel ? linksAdmin.value : linksOthers.value
})

//toggle full screen event emit
const toggleFullScreen = () => {
    emits('toggle-screen', props.isFullscreen)
}

//profile image
const profileImage = computed(() => {
    return profileInfo.obj.imagePath == null ? defaultImg : import.meta.env.VITE_API_URL + profileInfo.obj.imagePath
})


// ─── Build sidebar menu ───
// SuperAdmin: FULL menu (all business + all system pages)
// Admin:      Business modules only (no system pages like Error Log, Users, Menus, etc.)
// Instructor: Candidates (own), Schedules, Vehicle Fuel only
const buildMenu = (apiItems) => {
    const raw = apiItems || []
    let items = raw.map((item) => ({
        id: item.id ?? item.Id,
        title: item.title ?? item.Title,
        icon: (item.icon ?? item.Icon) || 'mdi-circle-small',
        route: (item.route ?? item.Route) || '',
        order: item.order ?? item.Order ?? 99,
        childItems: item.childItems ?? item.ChildItems ?? []
    }))

    if (isSuperAdmin) {
        // ── SuperAdmin: ensure ALL items exist ──
        const has = (route) => items.some(m =>
            (m.route || '').toLowerCase() === route ||
            (m.childItems && m.childItems.some(c => (c.route || '').toLowerCase() === route)))
        const hasTitle = (title) => items.some(m => m.title === title)

        if (!has('/dashboard')) {
            items = [{ id: 1, title: 'Dashboard', icon: 'mdi-view-dashboard', route: '/dashboard', order: 0, childItems: [] }, ...items]
        }
        if (!has('/candidates') || !has('/driving-sessions')) {
            items = items.filter(m => (m.route || '').toLowerCase() !== '/candidates')
            items.push({
                id: 14, title: 'Kandidatet', icon: 'mdi-account-group', route: '', order: 8,
                childItems: [
                    { id: 141, title: 'Lista e Kandidateve', icon: 'mdi-account-multiple', route: '/candidates' },
                    { id: 142, title: 'Vozitjet', icon: 'mdi-car-clock', route: '/driving-sessions' }
                ]
            })
        }
        if (!has('/schedules')) {
            items.push({ id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 7, childItems: [] })
        }
        if (!has('/instructors')) {
            items.push({ id: 15, title: 'Instructors', icon: 'mdi-account-tie', route: '/instructors', order: 9, childItems: [] })
        }
        if (!hasTitle('Automjetet') && !hasTitle('Vehicles')) {
            items.push({
                id: 20, title: 'Automjetet', icon: 'mdi-car', route: '', order: 10,
                childItems: [
                    { id: 21, title: 'Lista e Automjeteve', icon: 'mdi-car-side', route: '/vehicles' },
                    { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel' },
                    { id: 23, title: 'Serviset e Automjeteve', icon: 'mdi-wrench', route: '/vehicle-services' }
                ]
            })
        }
        if (!has('/daily-report')) {
            items.push({
                id: 30, title: 'Raportet', icon: 'mdi-chart-box', route: '', order: 11,
                childItems: [
                    { id: 31, title: 'Raporti Ditor', icon: 'mdi-file-document-outline', route: '/daily-report' }
                ]
            })
        }
        // System pages (SuperAdmin only)
        if (!has('/users')) {
            items.push({
                id: 50, title: 'System', icon: 'mdi-cog', route: '', order: 20,
                childItems: [
                    { id: 51, title: 'Users', icon: 'mdi-account-multiple', route: '/users' },
                    { id: 52, title: 'User Roles', icon: 'mdi-shield-account', route: '/user-role' },
                    { id: 53, title: 'Menus', icon: 'mdi-menu', route: '/menu' },
                    { id: 54, title: 'Menu Groups', icon: 'mdi-format-list-group', route: '/menu-group' },
                    { id: 55, title: 'Settings', icon: 'mdi-cog-outline', route: '/settings' },
                    { id: 56, title: 'Error Log', icon: 'mdi-alert-circle-outline', route: '/error-log' },
                    { id: 57, title: 'Browse Log', icon: 'mdi-history', route: '/browse-log' }
                ]
            })
        }
    } else if (isAdmin) {
        // ── Admin: business modules only, NO system pages ──
        const has = (route) => items.some(m =>
            (m.route || '').toLowerCase() === route ||
            (m.childItems && m.childItems.some(c => (c.route || '').toLowerCase() === route)))
        const hasTitle = (title) => items.some(m => m.title === title)

        // Remove any system pages that might come from API
        const systemRoutes = ['/users', '/user-role', '/menu', '/menu-group', '/settings', '/error-log', '/browse-log']
        items = items.filter(m => !systemRoutes.includes((m.route || '').toLowerCase()))
        items.forEach(m => {
            if (m.childItems) {
                m.childItems = m.childItems.filter(c => !systemRoutes.includes((c.route || '').toLowerCase()))
            }
        })
        // Remove empty groups after filtering
        items = items.filter(m => m.route || (m.childItems && m.childItems.length > 0))

        if (!has('/candidates') || !has('/driving-sessions')) {
            items = items.filter(m => (m.route || '').toLowerCase() !== '/candidates')
            items.push({
                id: 14, title: 'Kandidatet', icon: 'mdi-account-group', route: '', order: 8,
                childItems: [
                    { id: 141, title: 'Lista e Kandidateve', icon: 'mdi-account-multiple', route: '/candidates' },
                    { id: 142, title: 'Vozitjet', icon: 'mdi-car-clock', route: '/driving-sessions' }
                ]
            })
        }
        if (!has('/schedules')) {
            items.push({ id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 7, childItems: [] })
        }
        if (!has('/instructors')) {
            items.push({ id: 15, title: 'Instructors', icon: 'mdi-account-tie', route: '/instructors', order: 9, childItems: [] })
        }
        if (!hasTitle('Automjetet') && !hasTitle('Vehicles')) {
            items.push({
                id: 20, title: 'Automjetet', icon: 'mdi-car', route: '', order: 10,
                childItems: [
                    { id: 21, title: 'Lista e Automjeteve', icon: 'mdi-car-side', route: '/vehicles' },
                    { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel' },
                    { id: 23, title: 'Serviset e Automjeteve', icon: 'mdi-wrench', route: '/vehicle-services' }
                ]
            })
        }
        if (!has('/daily-report')) {
            items.push({
                id: 30, title: 'Raportet', icon: 'mdi-chart-box', route: '', order: 11,
                childItems: [
                    { id: 31, title: 'Raporti Ditor', icon: 'mdi-file-document-outline', route: '/daily-report' }
                ]
            })
        }
    } else if (isInstructor) {
        // ── Instructor: limited menu ──
        // Remove Driving Sessions
        items = items.filter(m => (m.route || '').toLowerCase() !== '/driving-sessions')
        items.forEach(m => {
            if (m.childItems) m.childItems = m.childItems.filter(c => (c.route || '').toLowerCase() !== '/driving-sessions')
        })
        // Remove system routes
        const systemRoutes = ['/users', '/user-role', '/menu', '/menu-group', '/settings', '/error-log', '/browse-log']
        items = items.filter(m => !systemRoutes.includes((m.route || '').toLowerCase()))
        items.forEach(m => {
            if (m.childItems) m.childItems = m.childItems.filter(c => !systemRoutes.includes((c.route || '').toLowerCase()))
        })
        items = items.filter(m => m.route || (m.childItems && m.childItems.length > 0))

        const has = (route) => items.some(m => (m.route || '').toLowerCase() === route)
        if (!has('/candidates')) {
            items.push({ id: 14, title: 'Kandidatet e Mi', icon: 'mdi-account-group', route: '/candidates', order: 1, childItems: [] })
        }
        if (!has('/schedules')) {
            items.push({ id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 2, childItems: [] })
        }
        if (!has('/vehicle-fuel')) {
            items.push({ id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel', order: 10, childItems: [] })
        }
    }

    items.sort((a, b) => (a.order ?? 99) - (b.order ?? 99))
    return items
}

// Fallback menu (used when API call fails)
const getFallbackMenu = () => {
    if (isSuperAdmin) {
        return [
            { id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 7, childItems: [] },
            { id: 14, title: 'Kandidatet', icon: 'mdi-account-group', route: '', order: 8, childItems: [
                { id: 141, title: 'Lista e Kandidateve', icon: 'mdi-account-multiple', route: '/candidates' },
                { id: 142, title: 'Vozitjet', icon: 'mdi-car-clock', route: '/driving-sessions' }
            ]},
            { id: 15, title: 'Instructors', icon: 'mdi-account-tie', route: '/instructors', order: 9, childItems: [] },
            { id: 20, title: 'Automjetet', icon: 'mdi-car', route: '', order: 10, childItems: [
                { id: 21, title: 'Lista e Automjeteve', icon: 'mdi-car-side', route: '/vehicles' },
                { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel' },
                { id: 23, title: 'Serviset e Automjeteve', icon: 'mdi-wrench', route: '/vehicle-services' }
            ]},
            { id: 30, title: 'Raportet', icon: 'mdi-chart-box', route: '', order: 11, childItems: [
                { id: 31, title: 'Raporti Ditor', icon: 'mdi-file-document-outline', route: '/daily-report' }
            ]},
            { id: 50, title: 'System', icon: 'mdi-cog', route: '', order: 20, childItems: [
                { id: 51, title: 'Users', icon: 'mdi-account-multiple', route: '/users' },
                { id: 52, title: 'User Roles', icon: 'mdi-shield-account', route: '/user-role' },
                { id: 53, title: 'Menus', icon: 'mdi-menu', route: '/menu' },
                { id: 54, title: 'Menu Groups', icon: 'mdi-format-list-group', route: '/menu-group' },
                { id: 55, title: 'Settings', icon: 'mdi-cog-outline', route: '/settings' },
                { id: 56, title: 'Error Log', icon: 'mdi-alert-circle-outline', route: '/error-log' },
                { id: 57, title: 'Browse Log', icon: 'mdi-history', route: '/browse-log' }
            ]}
        ]
    } else if (isAdmin) {
        return [
            { id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 7, childItems: [] },
            { id: 14, title: 'Kandidatet', icon: 'mdi-account-group', route: '', order: 8, childItems: [
                { id: 141, title: 'Lista e Kandidateve', icon: 'mdi-account-multiple', route: '/candidates' },
                { id: 142, title: 'Vozitjet', icon: 'mdi-car-clock', route: '/driving-sessions' }
            ]},
            { id: 15, title: 'Instructors', icon: 'mdi-account-tie', route: '/instructors', order: 9, childItems: [] },
            { id: 20, title: 'Automjetet', icon: 'mdi-car', route: '', order: 10, childItems: [
                { id: 21, title: 'Lista e Automjeteve', icon: 'mdi-car-side', route: '/vehicles' },
                { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel' },
                { id: 23, title: 'Serviset e Automjeteve', icon: 'mdi-wrench', route: '/vehicle-services' }
            ]},
            { id: 30, title: 'Raportet', icon: 'mdi-chart-box', route: '', order: 11, childItems: [
                { id: 31, title: 'Raporti Ditor', icon: 'mdi-file-document-outline', route: '/daily-report' }
            ]}
        ]
    } else {
        // Instructor fallback
        return [
            { id: 14, title: 'Kandidatet e Mi', icon: 'mdi-account-group', route: '/candidates', order: 1, childItems: [] },
            { id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 2, childItems: [] },
            { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel', order: 10, childItems: [] }
        ]
    }
}

// Fetch sidebar from API then apply role-based logic
menuStore.getSidebar(roleId)
    .then((res) => {
        menus.value = buildMenu(res.data)
    })
    .catch((error) => {
        console.log(error)
        menus.value = getFallbackMenu()
    })

//fetch notifications
userStore.getNotifications(localStorage.getItem('userId'))
    .then(res => {
        if (res.status == 200) {
            notifications.value = res.data
        }
    })

//sign out
const signOut = () => {
    userStore.updateHistory(localStorage.getItem('logCode'))
    userStore.signOut()
    router.push({ name: 'Landing' })
}
</script>

<style>
/* ─── Clean Gray Sidebar Theme ─── */
.sidebar-drawer {
    background: #F5F5F5 !important;
    border-right: 1px solid #E0E0E0 !important;
    color: #37474F !important;
}

.sidebar-drawer :deep(.v-list-item) {
    min-height: 44px;
    align-items: flex-start;
}

.sidebar-drawer :deep(.v-list-item__prepend) {
    padding-inline-end: 10px !important;
}

.sidebar-drawer :deep(.v-list-item__content) {
    overflow: visible !important;
}

.sidebar-drawer .v-list-item-title {
    color: #37474F !important;
    font-weight: 500;
    font-size: 0.875rem;
    white-space: normal !important;
    overflow: visible !important;
    text-overflow: clip !important;
    line-height: 1.25 !important;
    word-break: break-word;
}

.sidebar-drawer .v-list-item-subtitle {
    color: #78909C !important;
}

.sidebar-drawer .v-icon {
    color: #607D8B !important;
}

.sidebar-drawer .v-divider {
    border-color: #E0E0E0 !important;
}

.sidebar-profile-list {
    background: #EEEEEE !important;
    padding: 12px 0 !important;
}

.sidebar-profile-name {
    color: #263238 !important;
    font-weight: 600;
    font-size: 0.9rem;
}

.sidebar-profile-role {
    color: #78909C !important;
    font-size: 0.75rem;
}

.sidebar-menu-list {
    background: transparent !important;
    padding: 8px !important;
}

.sidebar-item-title {
    padding-left: 0 !important;
}

.sidebar-item-icon {
    padding-left: 0 !important;
}

.sidebar-active-item {
    background: #E0E0E0 !important;
    border-radius: 8px;
}

.sidebar-active-item .v-list-item-title {
    color: #263238 !important;
    font-weight: 600;
}

.sidebar-active-item .v-icon {
    color: #455A64 !important;
}

.sidebar-drawer .v-list-group__items .v-list-item {
    padding-left: 20px !important;
}

.sidebar-drawer :deep(.sidebar-menu-list > .v-list-item),
.sidebar-drawer :deep(.v-list-group__header) {
    padding-inline-start: 16px !important;
    padding-inline-end: 12px !important;
}

.sidebar-drawer :deep(.v-list-group__header .v-list-item__prepend) {
    margin-inline-end: 10px !important;
}

.sidebar-drawer .v-list-item:hover {
    background: #EEEEEE !important;
    border-radius: 8px;
}

/* App Bar clean style */
.app-top-bar {
    background: #FAFAFA !important;
    border-bottom: 1px solid #E0E0E0 !important;
}
</style>
