<template>
    <!-- ═══ Sidebar ═══ -->
    <v-navigation-drawer v-model="drawer" class="sidebar" :width="smAndDown ? 260 : 260" elevation="0">
        <!-- Brand -->
        <div class="sidebar-brand">
            <v-img src="/linda_logo.png" max-width="110" max-height="48" contain class="sidebar-brand-logo"></v-img>
        </div>

        <!-- Profile -->
        <div class="sidebar-profile">
            <v-avatar size="40" class="sidebar-profile-avatar">
                <v-img :src="profileImage"></v-img>
            </v-avatar>
            <div class="sidebar-profile-info">
                <div class="sidebar-profile-name">{{ profileInfo.obj.fullName }}</div>
                <div class="sidebar-profile-role">{{ profileInfo.obj.roleName }}</div>
            </div>
        </div>

        <div class="sidebar-divider"></div>

        <!-- Navigation -->
        <div class="sidebar-nav">
            <div class="sidebar-nav-label">Menu</div>
            <v-list density="compact" class="sidebar-menu" nav>
                <template v-for="item in menus" :key="item.id">
                    <!-- Single item -->
                    <v-list-item
                        v-if="!(item.childItems && item.childItems.length)"
                        :to="item.route"
                        @click="item.route && smAndDown && (drawer = false)"
                        active-class="sidebar-item--active"
                        class="sidebar-item"
                        rounded="lg"
                    >
                        <template v-slot:prepend>
                            <v-icon size="20" class="sidebar-item-icon">{{ item.icon }}</v-icon>
                        </template>
                        <v-list-item-title class="sidebar-item-text">{{ item.title }}</v-list-item-title>
                    </v-list-item>

                    <!-- Group item -->
                    <v-list-group v-else :value="item.title" class="sidebar-group">
                        <template v-slot:activator="{ props }">
                            <v-list-item v-bind="props" active-class="sidebar-item--active" class="sidebar-item" rounded="lg">
                                <template v-slot:prepend>
                                    <v-icon size="20" class="sidebar-item-icon">{{ item.icon }}</v-icon>
                                </template>
                                <v-list-item-title class="sidebar-item-text">{{ item.title }}</v-list-item-title>
                            </v-list-item>
                        </template>
                        <v-list-item
                            v-for="val in item.childItems"
                            :key="val.id"
                            :to="val.route"
                            @click="val.route && smAndDown && (drawer = false)"
                            active-class="sidebar-item--active"
                            class="sidebar-item sidebar-item--child"
                            rounded="lg"
                        >
                            <template v-slot:prepend>
                                <div class="sidebar-child-dot"></div>
                            </template>
                            <v-list-item-title class="sidebar-item-text">{{ val.title }}</v-list-item-title>
                        </v-list-item>
                    </v-list-group>
                </template>
            </v-list>
        </div>

        <!-- Bottom sign-out (mobile) -->
        <template v-if="smAndDown" v-slot:append>
            <div class="sidebar-bottom">
                <v-btn block variant="text" prepend-icon="mdi-logout" class="sidebar-signout-btn text-none"
                    @click.stop="dialogSignout = true">
                    Dalje
                </v-btn>
            </div>
        </template>
    </v-navigation-drawer>

    <!-- ═══ Top Bar ═══ -->
    <v-app-bar elevation="0" class="topbar" :height="64">
        <template v-slot:prepend>
            <v-btn icon variant="text" class="topbar-toggle" @click.stop="drawer = !drawer">
                <v-icon size="22">mdi-menu</v-icon>
            </v-btn>
        </template>

        <v-spacer></v-spacer>

        <div class="topbar-actions">
            <v-btn icon variant="text" size="small" class="topbar-action-btn" @click.stop="dialogLock = true">
                <v-icon size="20">mdi-lock-outline</v-icon>
                <v-tooltip activator="parent" location="bottom">Kyçe</v-tooltip>
            </v-btn>
            <v-btn icon variant="text" size="small" class="topbar-action-btn" @click.stop="toggleFullScreen">
                <v-icon size="20">{{ props.isFullscreen ? 'mdi-fullscreen-exit' : 'mdi-fullscreen' }}</v-icon>
                <v-tooltip activator="parent" location="bottom">{{ props.isFullscreen ? 'Dil nga ekrani i plotë' : 'Ekrani i plotë' }}</v-tooltip>
            </v-btn>

            <div class="topbar-separator"></div>

            <v-menu offset-y transition="slide-y-transition">
                <template v-slot:activator="{ props: menuProps }">
                    <v-btn variant="text" class="topbar-profile-btn text-none" v-bind="menuProps">
                        <v-avatar size="32" class="mr-2">
                            <v-img :src="profileImage"></v-img>
                        </v-avatar>
                        <span class="topbar-profile-name d-none d-sm-inline">{{ profileInfo.obj.fullName }}</span>
                        <v-icon size="18" class="ml-1">mdi-chevron-down</v-icon>
                    </v-btn>
                </template>
                <v-list density="compact" min-width="180" rounded="lg" class="topbar-dropdown">
                    <v-list-item v-for="(item, i) in personalizeSelect" :key="i" :value="item" :to="item.route" rounded="lg">
                        <template v-slot:prepend>
                            <v-icon size="18" class="mr-2">{{ item.icon || 'mdi-account-outline' }}</v-icon>
                        </template>
                        <v-list-item-title class="text-body-2">{{ item.text }}</v-list-item-title>
                    </v-list-item>
                    <v-divider class="my-1"></v-divider>
                    <v-list-item v-if="mdAndUp" rounded="lg" @click.stop="dialogSignout = true">
                        <template v-slot:prepend>
                            <v-icon size="18" color="error" class="mr-2">mdi-logout</v-icon>
                        </template>
                        <v-list-item-title class="text-body-2 text-error">Dalje</v-list-item-title>
                    </v-list-item>
                </v-list>
            </v-menu>
        </div>
    </v-app-bar>

    <!-- Sign-out Dialog -->
    <v-dialog v-model="dialogSignout" max-width="380" persistent>
        <v-card rounded="xl" class="pa-2">
            <v-card-text class="text-center pt-6 pb-2">
                <v-avatar color="error" variant="tonal" size="56" class="mb-4">
                    <v-icon icon="mdi-logout" size="28"></v-icon>
                </v-avatar>
                <div class="text-h6 font-weight-bold mb-1">Dëshironi të largoheni?</div>
                <div class="text-body-2 text-medium-emphasis">Shtypni Dalje për të larguar</div>
            </v-card-text>
            <v-card-actions class="px-6 pb-4">
                <v-btn block variant="text" class="text-none" @click.stop="dialogSignout = false">Qendro këtu</v-btn>
                <v-btn block variant="flat" color="error" class="text-none" @click.stop="signOut">Dalje</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>

    <!-- Lock Dialog -->
    <v-dialog v-model="dialogLock" max-width="380" persistent>
        <v-card rounded="xl" class="pa-2">
            <v-card-text class="text-center pt-6">
                <v-avatar size="64" class="mb-4">
                    <v-img :src="profileImage"></v-img>
                </v-avatar>
                <div class="text-h6 font-weight-bold mb-4">{{ profileInfo.obj.fullName }}</div>
                <v-form v-model="valid" @submit.prevent="lockOpen">
                    <v-text-field
                        v-model="signInForm.password"
                        variant="outlined"
                        type="password"
                        label="Fjalëkalimi"
                        :rules="passwordRules"
                        required
                        class="mb-2"
                    ></v-text-field>
                    <v-btn :disabled="!valid" block color="primary" variant="flat" type="submit" class="text-none">
                        <v-icon start>mdi-lock-open-outline</v-icon>
                        Hap
                    </v-btn>
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
    { text: 'Profili', route: '/profile', icon: 'mdi-account-outline' },
    { text: 'Fjalëkalimi', route: '/password-change', icon: 'mdi-key-outline' }
])
const linksOthers = ref([
    { text: 'Profili', route: '/profile', icon: 'mdi-account-outline' },
    { text: 'Fjalëkalimi', route: '/password-change', icon: 'mdi-key-outline' }
])
const profileInfo = JSON.parse(localStorage.getItem('profile') || '{}')

// ─── Role helpers ───
const roleId = localStorage.getItem('userRoleId')
const roleName = (typeof profileInfo?.obj?.roleName === 'string') ? profileInfo.obj.roleName
    : (typeof profileInfo?.obj?.RoleName === 'string') ? profileInfo.obj.RoleName : ''
const isSuperAdmin = roleName === 'SuperAdmin'
const isAdmin = roleId === '1' || roleName === 'Admin'
const isInstructor = roleId === '3' || roleName === 'Instructor'
const isAdminLevel = isSuperAdmin || isAdmin

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

const personalizeSelect = computed(() => {
    return isAdminLevel ? linksAdmin.value : linksOthers.value
})

const toggleFullScreen = () => {
    emits('toggle-screen', props.isFullscreen)
}

const profileImage = computed(() => {
    return profileInfo.obj.imagePath == null ? defaultImg : import.meta.env.VITE_API_URL + profileInfo.obj.imagePath
})

// ─── Build sidebar menu ───
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
        const has = (route) => items.some(m =>
            (m.route || '').toLowerCase() === route ||
            (m.childItems && m.childItems.some(c => (c.route || '').toLowerCase() === route)))
        const hasTitle = (title) => items.some(m => m.title === title)

        const systemRoutes = ['/users', '/user-role', '/menu', '/menu-group', '/settings', '/error-log', '/browse-log']
        items = items.filter(m => !systemRoutes.includes((m.route || '').toLowerCase()))
        items.forEach(m => {
            if (m.childItems) {
                m.childItems = m.childItems.filter(c => !systemRoutes.includes((c.route || '').toLowerCase()))
            }
        })
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
        items = items.filter(m => (m.route || '').toLowerCase() !== '/driving-sessions')
        items.forEach(m => {
            if (m.childItems) m.childItems = m.childItems.filter(c => (c.route || '').toLowerCase() !== '/driving-sessions')
        })
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
        return [
            { id: 14, title: 'Kandidatet e Mi', icon: 'mdi-account-group', route: '/candidates', order: 1, childItems: [] },
            { id: 16, title: 'Oraret', icon: 'mdi-calendar-clock', route: '/schedules', order: 2, childItems: [] },
            { id: 22, title: 'Derivatet e Automjeteve', icon: 'mdi-gas-station', route: '/vehicle-fuel', order: 10, childItems: [] }
        ]
    }
}

menuStore.getSidebar(roleId)
    .then((res) => {
        menus.value = buildMenu(res.data)
    })
    .catch((error) => {
        console.log(error)
        menus.value = getFallbackMenu()
    })

const signOut = () => {
    userStore.updateHistory(localStorage.getItem('logCode'))
    userStore.signOut()
    router.push({ name: 'SignIn' })
}
</script>

<style>
/* ═══════════════════════════════════════════════════════
   Sidebar — Dark Navy Blue
   ═══════════════════════════════════════════════════════ */
.sidebar.v-navigation-drawer {
    background: linear-gradient(180deg, #1e293b 0%, #0f172a 100%) !important;
    border-right: none !important;
    color: #e2e8f0 !important;
}

.sidebar-brand {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px 20px 16px;
}

.sidebar-brand-logo {
    filter: brightness(0) invert(1);
    opacity: 0.95;
}

.sidebar-profile {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 20px;
    margin: 0 12px 4px;
    background: rgba(255, 255, 255, 0.06);
    border-radius: 10px;
}

.sidebar-profile-avatar {
    border: 2px solid rgba(255,255,255,0.15);
}

.sidebar-profile-name {
    font-size: 0.8125rem;
    font-weight: 600;
    color: #f1f5f9;
    line-height: 1.3;
}

.sidebar-profile-role {
    font-size: 0.6875rem;
    color: #94a3b8;
    font-weight: 500;
}

.sidebar-profile-info {
    min-width: 0;
    overflow: hidden;
}

.sidebar-profile-info .sidebar-profile-name,
.sidebar-profile-info .sidebar-profile-role {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.sidebar-divider {
    height: 1px;
    margin: 12px 20px;
    background: rgba(255, 255, 255, 0.08);
}

.sidebar-nav {
    padding: 0 12px;
    flex: 1;
    overflow-y: auto;
}

.sidebar-nav-label {
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: #64748b;
    padding: 8px 12px 6px;
}

/* Menu list reset */
.sidebar-menu.v-list {
    background: transparent !important;
    padding: 0 !important;
    color: #cbd5e1 !important;
}

/* Menu items */
.sidebar-item.v-list-item {
    min-height: 40px !important;
    margin-bottom: 2px;
    padding: 0 12px !important;
    border-radius: 8px !important;
    color: #cbd5e1 !important;
    transition: all 0.15s ease;
}

.sidebar-item.v-list-item:hover {
    background: rgba(255, 255, 255, 0.06) !important;
    color: #fff !important;
}

.sidebar-item--active.v-list-item {
    background: rgba(37, 99, 235, 0.2) !important;
    color: #fff !important;
}

.sidebar-item--active .sidebar-item-icon {
    color: #60a5fa !important;
}

.sidebar-item--active .sidebar-item-text {
    color: #fff !important;
    font-weight: 600 !important;
}

.sidebar-item-icon {
    color: #94a3b8 !important;
    margin-inline-end: 12px !important;
    opacity: 1 !important;
}

.sidebar-item-text {
    font-size: 0.8125rem !important;
    font-weight: 500 !important;
    color: #cbd5e1 !important;
    white-space: normal !important;
    line-height: 1.3 !important;
}

/* Child items */
.sidebar-item--child.v-list-item {
    padding-left: 20px !important;
}

.sidebar-child-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: #475569;
    margin-inline-end: 12px;
    transition: background 0.15s;
}

.sidebar-item--active .sidebar-child-dot {
    background: #60a5fa;
}

/* Group headers */
.sidebar-group :deep(.v-list-group__header) {
    min-height: 40px !important;
    padding: 0 12px !important;
    color: #cbd5e1 !important;
}

.sidebar-group :deep(.v-list-group__header .v-list-item__append .v-icon) {
    color: #64748b !important;
    font-size: 18px !important;
}

.sidebar-group :deep(.v-list-group__items) {
    padding-left: 8px;
}

/* Bottom sign-out */
.sidebar-bottom {
    padding: 12px;
    border-top: 1px solid rgba(255,255,255,0.06);
}

.sidebar-signout-btn {
    color: #94a3b8 !important;
    justify-content: flex-start;
}

.sidebar-signout-btn:hover {
    color: #ef4444 !important;
    background: rgba(239, 68, 68, 0.1) !important;
}

/* ─── Sidebar scrollbar ─── */
.sidebar .v-navigation-drawer__content::-webkit-scrollbar {
    width: 4px;
}

.sidebar .v-navigation-drawer__content::-webkit-scrollbar-thumb {
    background: rgba(255,255,255,0.1);
    border-radius: 2px;
}

/* ═══════════════════════════════════════════════════════
   Top Bar — Clean White
   ═══════════════════════════════════════════════════════ */
.topbar.v-app-bar {
    background: #fff !important;
    border-bottom: 1px solid #e2e8f0 !important;
    box-shadow: 0 1px 3px rgba(0,0,0,0.04) !important;
}

.topbar-toggle {
    color: #475569 !important;
}

.topbar-actions {
    display: flex;
    align-items: center;
    gap: 4px;
    padding-right: 8px;
}

.topbar-action-btn {
    color: #64748b !important;
}

.topbar-action-btn:hover {
    color: #2563eb !important;
    background: rgba(37, 99, 235, 0.06) !important;
}

.topbar-separator {
    width: 1px;
    height: 24px;
    background: #e2e8f0;
    margin: 0 8px;
}

.topbar-profile-btn {
    color: #334155 !important;
    padding: 4px 8px !important;
    height: 44px !important;
}

.topbar-profile-name {
    font-weight: 500;
    font-size: 0.875rem;
    max-width: 140px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.topbar-dropdown.v-list {
    padding: 6px !important;
    border: 1px solid #e2e8f0;
    box-shadow: 0 10px 25px -5px rgba(0,0,0,0.1), 0 4px 10px -6px rgba(0,0,0,0.05) !important;
}

.topbar-dropdown .v-list-item {
    min-height: 38px !important;
    padding: 0 12px !important;
    border-radius: 8px !important;
}
</style>
