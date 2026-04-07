<template>
    <div class="page-container">
        <div class="page-header mb-6">
            <div class="page-title">Vozitjet</div>
        </div>
        <!-- ─── Stats Cards — horizontally scrollable on mobile ─── -->
        <div class="stats-scroll-wrap mb-4">
            <div class="stats-scroll">
                <v-menu v-model="calendarMenu" :close-on-content-click="false" location="bottom" min-width="auto">
                    <template v-slot:activator="{ props: menuProps }">
                        <v-card class="stat-card stat-card--date" elevation="2" rounded="lg" v-bind="menuProps">
                            <v-card-text class="d-flex align-center ga-3">
                                <v-avatar color="primary" size="44">
                                    <v-icon icon="mdi-calendar-today" color="white" size="22"></v-icon>
                                </v-avatar>
                                <div>
                                    <div class="text-h6 font-weight-bold">{{ selectedDateDisplay }}</div>
                                    <div class="text-caption text-medium-emphasis">Kliko për të ndryshuar</div>
                                </div>
                                <v-icon icon="mdi-chevron-down" size="18" class="ml-auto text-medium-emphasis"></v-icon>
                            </v-card-text>
                        </v-card>
                    </template>
                    <v-date-picker v-model="calendarDate" color="primary" @update:model-value="handleDateChange"></v-date-picker>
                </v-menu>

                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar color="info" size="44">
                            <v-icon icon="mdi-account-group" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ stats.totalSessions }}</div>
                            <div class="text-caption text-medium-emphasis">Vozitjet</div>
                        </div>
                    </v-card-text>
                </v-card>

                <v-card class="stat-card" elevation="2" rounded="lg">
                    <v-card-text class="d-flex align-center ga-3">
                        <v-avatar color="success" size="44">
                            <v-icon icon="mdi-cash" color="white" size="22"></v-icon>
                        </v-avatar>
                        <div>
                            <div class="text-h6 font-weight-bold">{{ formatCurrency(stats.totalPayments) }}</div>
                            <div class="text-caption text-medium-emphasis">Pagesat</div>
                        </div>
                    </v-card-text>
                </v-card>
            </div>
        </div>

        <!-- ─── Sessions table ─── -->
        <v-row>
            <v-col cols="12">
                <v-card class="table-card">
                    <div class="filter-bar">
                        <div class="export-group">
                            <v-btn variant="tonal" color="success" size="small" class="text-none" prepend-icon="mdi-file-excel">
                                <download-excel :data="sessions" :fields="headersExcel" type="xlsx"
                                    worksheet="all-data" name="driving-sessions.xlsx">Excel</download-excel>
                            </v-btn>
                            <v-btn variant="tonal" color="error" size="small" class="text-none" prepend-icon="mdi-file-pdf-box"
                                @click.stop="exportPdf">PDF</v-btn>
                        </div>
                        <v-spacer></v-spacer>
                        <div class="filter-inputs">
                            <v-select
                                v-model="filterStatus"
                                :items="statusFilterOptions"
                                item-title="label"
                                item-value="value"
                                label="Statusi"
                                clearable
                                class="filter-field filter-field--status"
                                @update:model-value="loadSessions"
                            ></v-select>
                            <v-menu v-model="fromDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                <template v-slot:activator="{ props: menuProps }">
                                    <v-text-field
                                        :model-value="fromDateDisplay"
                                        label="Nga data"
                                        prepend-inner-icon="mdi-calendar"
                                        readonly
                                        clearable
                                        class="filter-field filter-field--date"
                                        v-bind="menuProps"
                                        @click:clear="clearFromDate"
                                    ></v-text-field>
                                </template>
                                <v-date-picker v-model="fromDateModel" color="primary" @update:model-value="handleFromDate"></v-date-picker>
                            </v-menu>
                            <v-menu v-model="toDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                <template v-slot:activator="{ props: menuProps }">
                                    <v-text-field
                                        :model-value="toDateDisplay"
                                        label="Deri në datë"
                                        prepend-inner-icon="mdi-calendar"
                                        readonly
                                        clearable
                                        class="filter-field filter-field--date"
                                        v-bind="menuProps"
                                        @click:clear="clearToDate"
                                    ></v-text-field>
                                </template>
                                <v-date-picker v-model="toDateModel" color="primary" @update:model-value="handleToDate"></v-date-picker>
                            </v-menu>
                            <div class="action-btn-group">
                                <v-btn
                                    color="primary"
                                    variant="flat"
                                    class="text-none action-btn-half"
                                    prepend-icon="mdi-plus"
                                    size="small"
                                    @click="openCreateDialog"
                                >
                                    Regjistro vozitjen
                                </v-btn>
                                <v-btn
                                    color="warning"
                                    variant="flat"
                                    class="text-none action-btn-half"
                                    prepend-icon="mdi-cash-plus"
                                    size="small"
                                    @click="openPaymentDialog"
                                >
                                    Regjistro pagesën
                                </v-btn>
                            </div>
                        </div>
                    </div>

                    <v-data-table
                        :headers="headers"
                        :items="sessions"
                        :loading="loading"
                        no-data-text="Nuk ka vozitje për këtë datë"
                    >
                        <template v-slot:[`item.drivingTime`]="{ item }">
                            <v-chip size="small" color="primary" variant="tonal">
                                <v-icon start icon="mdi-clock-outline" size="14"></v-icon>
                                {{ item.drivingTime }}
                            </v-chip>
                        </template>

                        <template v-slot:[`item.paymentAmount`]="{ item }">
                            {{ formatCurrency(item.paymentAmount) }}
                        </template>

                        <template v-slot:[`item.vehicleDisplay`]="{ item }">
                            <v-chip size="small" variant="outlined" color="grey-darken-1">
                                <v-icon start icon="mdi-car" size="14"></v-icon>
                                {{ item.vehicleDisplay }}
                            </v-chip>
                        </template>

                        <template v-slot:[`item.status`]="{ item }">
                            <v-chip v-if="item.status" size="small" :color="statusColor(item.status)" variant="tonal">
                                {{ item.status }}
                            </v-chip>
                            <span v-else class="text-medium-emphasis text-body-2">—</span>
                        </template>

                        <template v-slot:[`item.examiner`]="{ item }">
                            <span v-if="item.examiner">{{ item.examiner }}</span>
                            <span v-else class="text-medium-emphasis text-body-2">—</span>
                        </template>

                        <template v-slot:[`item.actions`]="{ item }">
                            <div class="d-flex align-center ga-1">
                                <v-btn icon variant="text" color="primary" class="action-btn" @click="openEditDialog(item)">
                                    <v-icon size="18">mdi-pencil-outline</v-icon>
                                    <v-tooltip activator="parent" location="top">Ndrysho</v-tooltip>
                                </v-btn>
                                <v-btn v-if="isAdmin" icon variant="text" color="error" class="action-btn" @click="confirmDelete(item)">
                                    <v-icon size="18">mdi-delete-outline</v-icon>
                                    <v-tooltip activator="parent" location="top">Fshi</v-tooltip>
                                </v-btn>
                            </div>
                        </template>
                    </v-data-table>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Waiting List ─── -->
        <v-row class="mt-4">
            <v-col cols="12">
                <v-card elevation="2" rounded="lg">
                    <v-card-title class="d-flex flex-wrap align-center ga-2 pa-4">
                        <v-icon icon="mdi-clipboard-clock-outline" color="warning"></v-icon>
                        <span>Lista e pritjes</span>
                        <v-chip class="ml-1" size="small" color="warning" variant="tonal">{{ filteredWaitingList.length }}</v-chip>
                        <v-spacer></v-spacer>
                        <v-text-field
                            v-model="waitingSearch"
                            placeholder="Kërko (Emri, Mbiemri)..."
                            prepend-inner-icon="mdi-magnify"
                            variant="outlined"
                            density="compact"
                            hide-details
                            clearable
                            class="waiting-search"
                        ></v-text-field>
                    </v-card-title>
                    <v-divider></v-divider>
                    <v-data-table
                        :headers="waitingHeaders"
                        :items="filteredWaitingList"
                        :loading="waitingLoading"
                        class="waiting-table"
                        no-data-text="Nuk ka kandidatë në listën e pritjes"
                        density="comfortable"
                    >
                        <template v-slot:[`item.paymentAmount`]="{ item }">
                            <span v-if="item.paymentAmount > 0" class="font-weight-medium" style="color:#10b981">{{ formatCurrency(item.paymentAmount) }} &euro;</span>
                            <span v-else class="text-medium-emphasis">–</span>
                        </template>
                        <template v-slot:[`item.actions`]="{ item }">
                            <div class="d-flex align-center ga-1">
                                <v-btn
                                    variant="tonal"
                                    color="primary"
                                    size="small"
                                    prepend-icon="mdi-calendar-plus"
                                    class="text-none"
                                    @click="assignFromWaitingList(item)"
                                >
                                    Cakto datën
                                </v-btn>
                                <v-btn
                                    v-if="isAdmin"
                                    icon
                                    variant="text"
                                    color="error"
                                    size="small"
                                    @click="confirmWaitingDelete(item)"
                                >
                                    <v-icon size="18">mdi-delete-outline</v-icon>
                                    <v-tooltip activator="parent" location="top">Fshi nga lista</v-tooltip>
                                </v-btn>
                            </div>
                        </template>
                    </v-data-table>
                </v-card>
            </v-col>
        </v-row>

        <!-- ─── Create Driving Session Dialog ─── -->
        <v-dialog v-model="dialog" max-width="700" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-car-clock" color="primary"></v-icon>
                    <span>Regjistro vozitjen</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <v-form ref="formRef" @submit.prevent="saveSession">
                        <v-row>
                            <v-col cols="12">
                                <v-switch
                                    v-model="manualEntry"
                                    label="Kandidati nuk gjendet? Shkruaj manualisht"
                                    color="primary"
                                    density="compact"
                                    hide-details
                                    class="mb-2"
                                ></v-switch>
                            </v-col>
                            <v-col v-if="!manualEntry" cols="12" md="6">
                                <v-autocomplete
                                    v-model="form.candidateId"
                                    :items="candidateOptions"
                                    item-title="fullName"
                                    item-value="candidateId"
                                    label="Kandidati *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Zgjidhni kandidatin']"
                                    prepend-inner-icon="mdi-account-search"
                                    no-data-text="Nuk u gjet – aktivizo shkruaj manualisht"
                                ></v-autocomplete>
                            </v-col>
                            <v-col v-if="manualEntry" cols="12" md="6">
                                <v-text-field
                                    v-model="form.manualCandidateName"
                                    label="Emri dhe Mbiemri i kandidatit *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Shkruani emrin']"
                                    prepend-inner-icon="mdi-account-edit"
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="form.vehicleId"
                                    :items="vehicleOptions"
                                    item-title="label"
                                    item-value="vehicleId"
                                    label="Automjeti *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Required']"
                                    prepend-inner-icon="mdi-car"
                                ></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-menu v-model="drivingDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="form.drivingDate"
                                            label="Data e vozitjes"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            clearable
                                            hint="Pa datë → lista e pritjes"
                                            persistent-hint
                                            v-bind="menuProps"
                                            @click:clear="form.drivingDate = ''"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="drivingDateModel" color="primary" @update:model-value="handleDrivingDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="form.drivingTime"
                                    :items="timeSlots"
                                    label="Ora e vozitjes"
                                    variant="outlined"
                                    density="compact"
                                    clearable
                                    hint="Pa orë → lista e pritjes"
                                    persistent-hint
                                    prepend-inner-icon="mdi-clock-outline"
                                ></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-text-field
                                    v-model.number="form.paymentAmount"
                                    label="Shuma e pagesës"
                                    variant="outlined"
                                    density="compact"
                                    type="number"
                                    min="0"
                                    prepend-inner-icon="mdi-cash"
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-menu v-model="paymentDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="form.paymentDate"
                                            label="Data e pagesës"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            clearable
                                            v-bind="menuProps"
                                            @click:clear="form.paymentDate = ''"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="paymentDateModel" color="primary" @update:model-value="handlePaymentDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                        </v-row>
                        <v-alert v-if="!form.drivingDate && !form.drivingTime" type="info" variant="tonal" density="compact" class="mt-3 mb-0">
                            <strong>Listë pritjeje:</strong> Pa datë/orë kandidati shtohet automatikisht në listën e pritjes.
                        </v-alert>
                        <v-alert v-if="formError" type="error" density="compact" class="mt-2 mb-0" closable @click:close="formError = ''">
                            {{ formError }}
                        </v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="dialog = false">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="saving" @click="saveSession">
                        Ruaj
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- ─── Edit Driving Session Dialog ─── -->
        <v-dialog v-model="editDialog" max-width="600" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-pencil" color="primary"></v-icon>
                    <span>Ndrysho vozitjen</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <!-- Read-only info -->
                    <v-row class="mb-2">
                        <v-col cols="6">
                            <div class="text-body-2 text-medium-emphasis">Kandidati</div>
                            <div class="font-weight-medium">{{ editTarget?.candidateName }}</div>
                        </v-col>
                        <v-col cols="6">
                            <div class="text-body-2 text-medium-emphasis">Automjeti</div>
                            <div class="font-weight-medium">{{ editTarget?.vehicleDisplay }}</div>
                        </v-col>
                    </v-row>
                    <v-divider class="mb-4"></v-divider>
                    <v-form ref="editFormRef" @submit.prevent="saveEdit">
                        <v-row>
                            <v-col cols="12" md="6">
                                <v-menu v-model="editDrivingDateMenu" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="editForm.drivingDate"
                                            label="Data e vozitjes"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            clearable
                                            v-bind="menuProps"
                                            @click:clear="editForm.drivingDate = ''"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="editDrivingDateModel" color="primary" @update:model-value="handleEditDrivingDate"></v-date-picker>
                                </v-menu>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="editForm.drivingTime"
                                    :items="timeSlots"
                                    label="Ora e vozitjes"
                                    variant="outlined"
                                    density="compact"
                                    clearable
                                    prepend-inner-icon="mdi-clock-outline"
                                ></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-select
                                    v-model="editForm.status"
                                    :items="statusOptions"
                                    item-title="label"
                                    item-value="value"
                                    label="Statusi"
                                    variant="outlined"
                                    density="compact"
                                    clearable
                                    prepend-inner-icon="mdi-flag-outline"
                                ></v-select>
                            </v-col>
                            <v-col cols="12" md="6">
                                <v-text-field
                                    v-model="editForm.examiner"
                                    label="Egzamineri"
                                    variant="outlined"
                                    density="compact"
                                    prepend-inner-icon="mdi-account-tie"
                                ></v-text-field>
                            </v-col>
                        </v-row>
                        <v-alert v-if="editError" type="error" density="compact" class="mt-2 mb-0" closable @click:close="editError = ''">
                            {{ editError }}
                        </v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="editDialog = false">Anulo</v-btn>
                    <v-btn color="primary" variant="elevated" :loading="editSaving" @click="saveEdit">
                        Ndrysho
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Delete confirm -->
        <v-dialog v-model="deleteDialog" max-width="400">
            <v-card rounded="lg">
                <v-card-title>Konfirmo fshirjen</v-card-title>
                <v-card-text>Jeni të sigurt që dëshironi të fshini këtë vozitje?</v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
                    <v-btn color="error" variant="elevated" :loading="deleting" @click="doDelete">Fshi</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- Waiting list delete confirm -->
        <v-dialog v-model="waitingDeleteDialog" max-width="440">
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-delete-alert" color="error"></v-icon>
                    <span>Fshi nga lista e pritjes</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-5">
                    <div v-if="waitingDeleteTarget" class="mb-2">
                        <span class="font-weight-medium">{{ waitingDeleteTarget.candidateName }}</span>
                        <span v-if="waitingDeleteTarget.paymentAmount > 0" class="text-medium-emphasis ml-1">
                            ({{ formatCurrency(waitingDeleteTarget.paymentAmount) }} &euro;)
                        </span>
                    </div>
                    Jeni të sigurt që dëshironi të fshini këtë regjistrim nga lista e pritjes?
                </v-card-text>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="waitingDeleteDialog = false">Anulo</v-btn>
                    <v-btn color="error" variant="elevated" :loading="waitingDeleting" @click="doWaitingDelete">Fshi</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- ─── Register Payment Dialog (advance payment → waiting list) ─── -->
        <v-dialog v-model="paymentDialog" max-width="560" scrollable>
            <v-card rounded="lg">
                <v-card-title class="d-flex align-center ga-2 pa-4">
                    <v-icon icon="mdi-cash-plus" color="warning"></v-icon>
                    <span>Regjistro pagesën e vozitjes</span>
                </v-card-title>
                <v-divider></v-divider>
                <v-card-text class="pa-6">
                    <v-alert type="info" variant="tonal" density="compact" class="mb-4">
                        Kandidati do të shtohet automatikisht në <strong>Listën e pritjes</strong> pas regjistrimit të pagesës.
                    </v-alert>
                    <v-form ref="paymentFormRef" @submit.prevent="savePayment">
                        <v-row>
                            <v-col cols="12">
                                <v-switch
                                    v-model="paymentManual"
                                    label="Kandidati nuk gjendet? Shkruaj manualisht"
                                    color="primary"
                                    density="compact"
                                    hide-details
                                    class="mb-2"
                                ></v-switch>
                            </v-col>
                            <v-col v-if="!paymentManual" cols="12">
                                <v-autocomplete
                                    v-model="paymentForm.candidateId"
                                    :items="candidateOptions"
                                    item-title="fullName"
                                    item-value="candidateId"
                                    label="Kandidati *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Zgjidhni kandidatin']"
                                    prepend-inner-icon="mdi-account-search"
                                    no-data-text="Nuk u gjet – aktivizo shkruaj manualisht"
                                ></v-autocomplete>
                            </v-col>
                            <v-col v-if="paymentManual" cols="12">
                                <v-text-field
                                    v-model="paymentForm.manualCandidateName"
                                    label="Emri dhe Mbiemri i kandidatit *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Shkruani emrin']"
                                    prepend-inner-icon="mdi-account-edit"
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12" sm="6">
                                <v-select
                                    v-model="paymentForm.vehicleId"
                                    :items="vehicleOptions"
                                    item-title="label"
                                    item-value="vehicleId"
                                    label="Automjeti *"
                                    variant="outlined"
                                    density="compact"
                                    :rules="[v => !!v || 'Zgjidhni automjetin']"
                                    prepend-inner-icon="mdi-car"
                                ></v-select>
                            </v-col>
                            <v-col cols="12" sm="6">
                                <v-text-field
                                    v-model.number="paymentForm.paymentAmount"
                                    label="Shuma e pagesës *"
                                    variant="outlined"
                                    density="compact"
                                    type="number"
                                    min="0.01"
                                    step="0.01"
                                    :rules="[v => v > 0 || 'Vendosni shumën']"
                                    prepend-inner-icon="mdi-cash"
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12" sm="6">
                                <v-menu v-model="paymentDateMenuAdv" :close-on-content-click="false" location="bottom" transition="scale-transition" min-width="auto">
                                    <template v-slot:activator="{ props: menuProps }">
                                        <v-text-field
                                            :model-value="paymentForm.paymentDate"
                                            label="Data e pagesës"
                                            variant="outlined"
                                            density="compact"
                                            prepend-inner-icon="mdi-calendar"
                                            readonly
                                            clearable
                                            v-bind="menuProps"
                                            @click:clear="paymentForm.paymentDate = ''"
                                        ></v-text-field>
                                    </template>
                                    <v-date-picker v-model="paymentDateModelAdv" color="primary" @update:model-value="handlePaymentDateAdv"></v-date-picker>
                                </v-menu>
                            </v-col>
                        </v-row>
                        <v-alert v-if="paymentError" type="error" density="compact" class="mt-2 mb-0" closable @click:close="paymentError = ''">
                            {{ paymentError }}
                        </v-alert>
                    </v-form>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions class="pa-4">
                    <v-spacer></v-spacer>
                    <v-btn variant="text" @click="paymentDialog = false">Anulo</v-btn>
                    <v-btn color="warning" variant="elevated" :loading="paymentSaving" @click="savePayment">
                        Regjistro pagesën
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup>
import { useDrivingSessionStore } from '@/store/DrivingSessionStore';
import { useSettingStore } from '@/store/SettingStore';
import { storeToRefs } from 'pinia';
import { ref, computed, onMounted, nextTick } from 'vue';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import DownloadExcel from 'vue-json-excel3';

const store = useDrivingSessionStore();
const settingStore = useSettingStore();
const { loading } = storeToRefs(store);

// ─── Role helpers ───
const isAdmin = computed(() => {
    try {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        const role = profile?.obj?.roleName || profile?.obj?.RoleName || '';
        return role === 'Admin' || role === 'SuperAdmin';
    } catch { return false; }
});

// ─── Date picker ───
const calendarDate = ref(new Date());
const calendarMenu = ref(false);
const selectedDateStr = ref(formatDate(new Date()));
const selectedDateDisplay = computed(() => selectedDateStr.value);

function formatDate(value) {
    if (!value) return '';
    if (value instanceof Date) {
        const dd = String(value.getDate()).padStart(2, '0');
        const mm = String(value.getMonth() + 1).padStart(2, '0');
        const yyyy = value.getFullYear();
        return `${dd}.${mm}.${yyyy}`;
    }
    const str = String(value).split('T')[0];
    const parts = str.split('-');
    if (parts.length === 3) return `${parts[2]}.${parts[1]}.${parts[0]}`;
    return str;
}

function handleDateChange(v) {
    selectedDateStr.value = formatDate(v);
    calendarMenu.value = false;
    fromDateDisplay.value = '';
    fromDateModel.value = null;
    toDateDisplay.value = '';
    toDateModel.value = null;
    useRangeMode.value = false;
    loadSessions();
    loadStats();
}

// ─── Stats ───
const stats = ref({ totalSessions: 0, totalPayments: 0 });

function loadStats() {
    store.getSessionStats(selectedDateStr.value)
        .then((res) => {
            const d = res?.data;
            stats.value = {
                totalSessions: d?.totalSessions ?? 0,
                totalPayments: d?.totalPayments ?? 0,
            };
        })
        .catch(() => { stats.value = { totalSessions: 0, totalPayments: 0 }; });
}

function formatCurrency(v) {
    if (v == null || isNaN(v)) return '0.00';
    return Number(v).toFixed(2);
}

// ─── Status helpers ───
const statusOptions = [
    { label: 'Kaloi', value: 'Kaloi' },
    { label: 'Deshtoi', value: 'Deshtoi' },
    { label: 'Anuloi', value: 'Anuloi' },
];

const statusFilterOptions = [
    { label: 'All', value: '' },
    { label: 'Kaloi', value: 'Kaloi' },
    { label: 'Deshtoi', value: 'Deshtoi' },
    { label: 'Anuloi', value: 'Anuloi' },
];

function statusColor(s) {
    if (!s) return 'grey';
    switch (s) {
        case 'Kaloi': return 'success';
        case 'Deshtoi': return 'error';
        case 'Anuloi': return 'warning';
        default: return 'grey';
    }
}

// ─── Filters ───
const filterStatus = ref('');

// Date range filters
const fromDateMenu = ref(false);
const fromDateModel = ref(null);
const fromDateDisplay = ref('');
const toDateMenu = ref(false);
const toDateModel = ref(null);
const toDateDisplay = ref('');
// Track whether we are in range mode (from/to) vs calendar single-date mode
const useRangeMode = ref(false);

function handleFromDate(v) {
    fromDateDisplay.value = formatDate(v);
    nextTick(() => { fromDateMenu.value = false; });
    useRangeMode.value = true;
    loadSessions();
}
function clearFromDate() {
    fromDateDisplay.value = '';
    fromDateModel.value = null;
    if (!toDateDisplay.value) useRangeMode.value = false;
    loadSessions();
}
function handleToDate(v) {
    toDateDisplay.value = formatDate(v);
    nextTick(() => { toDateMenu.value = false; });
    useRangeMode.value = true;
    loadSessions();
}
function clearToDate() {
    toDateDisplay.value = '';
    toDateModel.value = null;
    if (!fromDateDisplay.value) useRangeMode.value = false;
    loadSessions();
}

// ─── Sessions table ───
const headers = [
    { title: '#', key: 'index', width: '50px', sortable: false },
    { title: 'Data', key: 'drivingDate', width: '110px' },
    { title: 'Ora', key: 'drivingTime', width: '100px' },
    { title: 'Kandidati', key: 'candidateName' },
    { title: 'Automjeti', key: 'vehicleDisplay' },
    { title: 'Pagesa', key: 'paymentAmount', width: '100px' },
    { title: 'Statusi', key: 'status', width: '110px' },
    { title: 'Egzamineri', key: 'examiner', width: '130px' },
    { title: 'Veprimet', key: 'actions', sortable: false, width: '100px' },
];

const headersExcel = {
    '#': 'index',
    'Date': 'drivingDate',
    'Time': 'drivingTime',
    'Candidate': 'candidateName',
    'Vehicle': 'vehicleDisplay',
    'Payment': 'paymentAmount',
    'Payment Date': 'paymentDate',
    'Status': 'status',
    'Examiner': 'examiner',
};

const sessions = ref([]);

function normalizeSession(row, idx) {
    if (!row || typeof row !== 'object') return null;
    const plate = row.vehiclePlate ?? row.VehiclePlate ?? '';
    const brand = row.vehicleBrand ?? row.VehicleBrand ?? '';
    return {
        index: idx + 1,
        drivingSessionId: row.drivingSessionId ?? row.DrivingSessionId,
        candidateId: row.candidateId ?? row.CandidateId,
        candidateName: row.candidateName ?? row.CandidateName ?? '',
        vehicleId: row.vehicleId ?? row.VehicleId,
        vehicleDisplay: plate + (brand ? ` – ${brand}` : ''),
        drivingDate: row.drivingDate ?? row.DrivingDate ?? '',
        drivingTime: row.drivingTime ?? row.DrivingTime ?? '',
        paymentAmount: row.paymentAmount ?? row.PaymentAmount ?? 0,
        paymentDate: row.paymentDate ?? row.PaymentDate ?? '',
        status: row.status ?? row.Status ?? '',
        examiner: row.examiner ?? row.Examiner ?? '',
    };
}

function loadSessions() {
    const statusVal = filterStatus.value || null;

    if (useRangeMode.value) {
        // Use date range endpoint
        store.getSessionsByDateRange(
            fromDateDisplay.value || null,
            toDateDisplay.value || null,
            statusVal
        )
            .then((res) => {
                const body = res?.data;
                const rawData = body?.data ?? body?.Data ?? [];
                sessions.value = rawData.map(normalizeSession).filter(Boolean);
                // Update stats from loaded data
                stats.value = {
                    totalSessions: sessions.value.length,
                    totalPayments: sessions.value.reduce((sum, s) => sum + (s.paymentAmount || 0), 0),
                };
            })
            .catch(() => {
                sessions.value = [];
                settingStore.toggleSnackbar({ status: true, msg: 'Error loading sessions' });
            });
    } else {
        // Use single date endpoint (calendar mode)
        store.getSessionsByDate(selectedDateStr.value, statusVal)
            .then((res) => {
                const body = res?.data;
                const rawData = body?.data ?? body?.Data ?? [];
                sessions.value = rawData.map(normalizeSession).filter(Boolean);
            })
            .catch(() => {
                sessions.value = [];
                settingStore.toggleSnackbar({ status: true, msg: 'Error loading sessions' });
            });
        loadStats();
    }
}

// ─── Create dialog ───
const dialog = ref(false);
const saving = ref(false);
const formRef = ref(null);
const formError = ref('');

const manualEntry = ref(false);

const form = ref({
    candidateId: null,
    manualCandidateName: '',
    vehicleId: null,
    drivingDate: '',
    drivingTime: null,
    paymentAmount: 0,
    paymentDate: '',
});

const drivingDateMenu = ref(false);
const drivingDateModel = ref(null);
const paymentDateMenu = ref(false);
const paymentDateModel = ref(null);

function openCreateDialog(prefill = null) {
    manualEntry.value = false;
    form.value = {
        candidateId: prefill?.candidateId ?? null,
        manualCandidateName: '',
        vehicleId: null,
        drivingDate: '',
        drivingTime: null,
        paymentAmount: 0,
        paymentDate: '',
    };
    drivingDateModel.value = null;
    paymentDateModel.value = null;
    formError.value = '';
    dialog.value = true;
}

function handleDrivingDate(v) {
    form.value.drivingDate = formatDate(v);
    nextTick(() => { drivingDateMenu.value = false; });
}
function handlePaymentDate(v) {
    form.value.paymentDate = formatDate(v);
    nextTick(() => { paymentDateMenu.value = false; });
}

const timeSlots = (() => {
    const slots = [];
    for (let h = 8; h <= 15; h++) {
        for (let m = 0; m < 60; m += 15) {
            if (h === 15 && m > 0) break;
            slots.push(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`);
        }
    }
    return slots;
})();

// Dropdowns
const candidateOptions = ref([]);
const vehicleOptions = ref([]);

function loadDropdowns() {
    store.getCandidatesDropdown()
        .then((res) => {
            candidateOptions.value = (res?.data?.data ?? []).map(c => ({
                candidateId: c.candidateId ?? c.CandidateId,
                fullName: c.fullName ?? c.FullName ?? '',
            }));
        })
        .catch(() => { candidateOptions.value = []; });

    store.getVehiclesDropdown()
        .then((res) => {
            vehicleOptions.value = (res?.data?.data ?? []).map(v => ({
                vehicleId: v.vehicleId ?? v.VehicleId,
                label: `${v.plateNumber ?? v.PlateNumber}${v.brand ? ' – ' + v.brand : ''}`,
            }));
        })
        .catch(() => { vehicleOptions.value = []; });
}

// ─── Waiting List ───
const waitingList = ref([]);
const waitingLoading = ref(false);
const waitingSearch = ref('');

const filteredWaitingList = computed(() => {
    const q = (waitingSearch.value || '').trim().toLowerCase();
    if (!q) return waitingList.value;
    return waitingList.value.filter(w => {
        const name = (w.candidateName || '').toLowerCase();
        return name.includes(q);
    });
});

const waitingHeaders = [
    { title: 'Kandidati', key: 'candidateName' },
    { title: 'Automjeti', key: 'vehicleDisplay' },
    { title: 'Pagesa', key: 'paymentAmount', width: '110px' },
    { title: 'Data e pagesës', key: 'paymentDate', width: '130px' },
    { title: 'Regjistruar', key: 'dateAddedDisplay', width: '130px' },
    { title: 'Veprimet', key: 'actions', sortable: false, width: '160px' },
];

function loadWaitingList() {
    waitingLoading.value = true;
    store.getWaitingList()
        .then((res) => {
            const raw = res?.data?.data ?? [];
            waitingList.value = raw.map(w => ({
                drivingSessionId: w.drivingSessionId ?? w.DrivingSessionId,
                candidateId: w.candidateId ?? w.CandidateId,
                candidateName: w.candidateName ?? w.CandidateName ?? w.manualCandidateName ?? '',
                manualCandidateName: w.manualCandidateName ?? w.ManualCandidateName ?? '',
                vehicleId: w.vehicleId ?? w.VehicleId,
                vehicleDisplay: `${w.vehiclePlate ?? w.VehiclePlate ?? ''}${w.vehicleBrand ? ' – ' + w.vehicleBrand : ''}`,
                drivingDate: w.drivingDate ?? w.DrivingDate ?? '',
                drivingTime: w.drivingTime ?? w.DrivingTime ?? '',
                paymentAmount: w.paymentAmount ?? w.PaymentAmount ?? 0,
                paymentDate: w.paymentDate ?? w.PaymentDate ?? '',
                status: w.status ?? w.Status ?? '',
                dateAddedDisplay: w.dateAdded ? new Date(w.dateAdded).toLocaleDateString('sq-AL') : '',
            }));
        })
        .catch(() => { waitingList.value = []; })
        .finally(() => { waitingLoading.value = false; });
}

function assignFromWaitingList(item) {
    openEditDialog(item);
}

async function saveSession() {
    formError.value = '';
    const valid = await formRef.value?.validate();
    if (!valid?.valid) return;

    saving.value = true;
    try {
        const payload = {
            vehicleId: form.value.vehicleId,
            drivingDate: form.value.drivingDate,
            drivingTime: form.value.drivingTime,
            paymentAmount: form.value.paymentAmount || 0,
            paymentDate: form.value.paymentDate || null,
        };
        if (manualEntry.value) {
            payload.candidateId = null;
            payload.manualCandidateName = form.value.manualCandidateName;
        } else {
            payload.candidateId = form.value.candidateId;
        }
        const res = await store.createDrivingSession(payload);
        const body = res?.data;
        if (body?.status === 'error') {
            formError.value = body.responseMsg || 'An error occurred';
            saving.value = false;
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Driving session created successfully!' });
        dialog.value = false;
        loadSessions();
        loadStats();
        loadWaitingList();
    } catch (err) {
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Failed to create session';
        formError.value = msg;
    } finally {
        saving.value = false;
    }
}

// ─── Payment-only dialog (advance payment → waiting list) ───
const paymentDialog = ref(false);
const paymentSaving = ref(false);
const paymentFormRef = ref(null);
const paymentError = ref('');
const paymentManual = ref(false);
const paymentDateMenuAdv = ref(false);
const paymentDateModelAdv = ref(null);

const paymentForm = ref({
    candidateId: null,
    manualCandidateName: '',
    vehicleId: null,
    paymentAmount: 0,
    paymentDate: '',
});

function openPaymentDialog() {
    paymentManual.value = false;
    paymentForm.value = {
        candidateId: null,
        manualCandidateName: '',
        vehicleId: null,
        paymentAmount: 0,
        paymentDate: formatDate(new Date()),
    };
    paymentDateModelAdv.value = new Date();
    paymentError.value = '';
    paymentDialog.value = true;
}

function handlePaymentDateAdv(v) {
    paymentForm.value.paymentDate = formatDate(v);
    nextTick(() => { paymentDateMenuAdv.value = false; });
}

async function savePayment() {
    paymentError.value = '';
    const valid = await paymentFormRef.value?.validate();
    if (!valid?.valid) return;

    paymentSaving.value = true;
    try {
        const payload = {
            vehicleId: paymentForm.value.vehicleId,
            drivingDate: null,
            drivingTime: null,
            paymentAmount: paymentForm.value.paymentAmount || 0,
            paymentDate: paymentForm.value.paymentDate || null,
        };
        if (paymentManual.value) {
            payload.candidateId = null;
            payload.manualCandidateName = paymentForm.value.manualCandidateName;
        } else {
            payload.candidateId = paymentForm.value.candidateId;
        }
        const res = await store.createDrivingSession(payload);
        const body = res?.data;
        if (body?.status === 'error') {
            paymentError.value = body.responseMsg || 'Ndodhi një gabim';
            paymentSaving.value = false;
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Pagesa u regjistrua me sukses! Kandidati u shtua në listën e pritjes.' });
        paymentDialog.value = false;
        loadWaitingList();
        loadSessions();
        loadStats();
    } catch (err) {
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Dështoi regjistrimi i pagesës';
        paymentError.value = msg;
    } finally {
        paymentSaving.value = false;
    }
}

// ─── Edit dialog ───
const editDialog = ref(false);
const editSaving = ref(false);
const editFormRef = ref(null);
const editError = ref('');
const editTarget = ref(null);

const editForm = ref({
    drivingDate: '',
    drivingTime: null,
    status: null,
    examiner: '',
});
const editDrivingDateMenu = ref(false);
const editDrivingDateModel = ref(null);

function handleEditDrivingDate(val) {
    if (val) {
        const d = new Date(val);
        editForm.value.drivingDate = `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
    }
    editDrivingDateMenu.value = false;
}

function openEditDialog(item) {
    editTarget.value = item;
    editForm.value = {
        drivingDate: item.drivingDate || '',
        drivingTime: item.drivingTime || null,
        status: item.status || null,
        examiner: item.examiner || '',
    };
    editDrivingDateModel.value = null;
    editError.value = '';
    editDialog.value = true;
}

async function saveEdit() {
    editError.value = '';
    editSaving.value = true;
    try {
        const res = await store.updateDrivingSession(editTarget.value.drivingSessionId, {
            drivingDate: editForm.value.drivingDate || null,
            drivingTime: editForm.value.drivingTime || null,
            status: editForm.value.status || null,
            examiner: editForm.value.examiner || null,
        });
        const body = res?.data;
        if (body?.status === 'error') {
            editError.value = body.responseMsg || 'An error occurred';
            editSaving.value = false;
            return;
        }
        settingStore.toggleSnackbar({ status: true, msg: 'Session updated successfully!' });
        editDialog.value = false;
        loadSessions();
        loadStats();
        loadWaitingList();
    } catch (err) {
        const msg = err?.response?.data?.responseMsg || err?.response?.data?.ResponseMsg || 'Failed to update session';
        editError.value = msg;
    } finally {
        editSaving.value = false;
    }
}

// ─── Delete ───
const deleteDialog = ref(false);
const deleting = ref(false);
const deleteTarget = ref(null);

function confirmDelete(item) {
    deleteTarget.value = item;
    deleteDialog.value = true;
}

async function doDelete() {
    if (!deleteTarget.value) return;
    deleting.value = true;
    try {
        await store.deleteDrivingSession(deleteTarget.value.drivingSessionId);
        settingStore.toggleSnackbar({ status: true, msg: 'Session deleted' });
        deleteDialog.value = false;
        loadSessions();
        loadStats();
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Error deleting session' });
    } finally {
        deleting.value = false;
    }
}

// ─── Waiting List Delete ───
const waitingDeleteDialog = ref(false);
const waitingDeleting = ref(false);
const waitingDeleteTarget = ref(null);

function confirmWaitingDelete(item) {
    waitingDeleteTarget.value = item;
    waitingDeleteDialog.value = true;
}

async function doWaitingDelete() {
    if (!waitingDeleteTarget.value) return;
    waitingDeleting.value = true;
    try {
        await store.deleteDrivingSession(waitingDeleteTarget.value.drivingSessionId);
        settingStore.toggleSnackbar({ status: true, msg: 'U fshi nga lista e pritjes' });
        waitingDeleteDialog.value = false;
        loadWaitingList();
    } catch {
        settingStore.toggleSnackbar({ status: true, msg: 'Gabim gjatë fshirjes' });
    } finally {
        waitingDeleting.value = false;
    }
}

// ─── PDF Export ───
function exportPdf() {
    const doc = new jsPDF({ orientation: 'landscape' });
    const title = useRangeMode.value
        ? `Driving Sessions – ${fromDateDisplay.value || '...'} to ${toDateDisplay.value || '...'}`
        : `Driving Sessions – ${selectedDateStr.value}`;
    doc.text(title, 14, 10);
    const head = ['#', 'Date', 'Time', 'Candidate', 'Vehicle', 'Payment', 'Status', 'Examiner'];
    const body = sessions.value.map(r => [r.index, r.drivingDate, r.drivingTime, r.candidateName, r.vehicleDisplay, formatCurrency(r.paymentAmount), r.status || '', r.examiner || '']);
    autoTable(doc, { head: [head], body });
    doc.save('driving-sessions.pdf');
}

// ─── Init ───
onMounted(() => {
    loadSessions();
    loadStats();
    loadDropdowns();
    loadWaitingList();
});
</script>

<style scoped>
/* ─── Stats horizontal scroll ─── */
.stats-scroll-wrap {
    width: 100%;
}

.stats-scroll {
    display: flex;
    gap: 16px;
}

.stat-card {
    border-radius: 12px;
    transition: transform 0.15s, box-shadow 0.15s;
    flex: 1 1 0;
    min-width: 0;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12) !important;
}

.stat-card--date {
    cursor: pointer;
}

.filter-inputs {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.filter-field--status {
    min-width: 140px;
    max-width: 160px;
}

.filter-field--date {
    min-width: 140px;
    max-width: 160px;
}

.action-btn-group {
    display: flex;
    gap: 8px;
    flex-shrink: 0;
}

.action-btn-half {
    white-space: nowrap;
}

.waiting-search {
    max-width: 260px;
    min-width: 160px;
}

.waiting-table :deep(thead th) {
    font-weight: 600;
    font-size: 13px;
    padding: 14px 16px !important;
    background: #fff8e1;
    color: #424242;
}

.waiting-table :deep(tbody td) {
    padding: 12px 16px !important;
    font-size: 14px;
}

/* ─── Tablet ─── */
@media (max-width: 960px) {
    .filter-bar {
        flex-direction: column !important;
        align-items: stretch !important;
    }

    .filter-inputs {
        width: 100%;
    }

    .filter-field--status,
    .filter-field--date {
        min-width: 0;
        max-width: 100%;
        flex: 1;
    }
}

/* ─── Mobile ─── */
@media (max-width: 600px) {
    .stats-scroll-wrap {
        overflow-x: auto;
        -webkit-overflow-scrolling: touch;
        margin-left: -12px;
        margin-right: -12px;
        padding: 0 12px;
    }

    .stats-scroll {
        min-width: max-content;
        gap: 10px;
        padding-bottom: 4px;
    }

    .stat-card {
        flex: 0 0 auto;
        min-width: 180px;
    }

    .stat-card :deep(.v-card-text) {
        padding: 10px 12px !important;
    }

    .stat-card :deep(.text-h6) {
        font-size: 0.95rem !important;
    }

    .stat-card :deep(.v-avatar) {
        width: 36px !important;
        height: 36px !important;
    }

    .stat-card :deep(.v-avatar .v-icon) {
        font-size: 18px !important;
    }

    .filter-inputs {
        flex-direction: column;
    }

    .filter-field--status,
    .filter-field--date {
        width: 100%;
    }

    .export-group {
        width: 100%;
    }

    .export-group .v-btn {
        flex: 1;
    }

    .action-btn-group {
        width: 100%;
        display: flex;
        gap: 6px;
    }

    .action-btn-half {
        flex: 1 1 0;
        min-width: 0;
        font-size: 0.75rem !important;
        padding: 0 8px !important;
    }

    .action-btn-half :deep(.v-btn__prepend) {
        margin-inline-end: 4px;
    }

    .waiting-search {
        max-width: 100%;
        min-width: 0;
    }
}
</style>
