# Autoshkolla — System Testing & Stability Checklist

This document tracks functional testing across every module, plus the
error-handling and validation rules the system must enforce. Use the checkboxes
during a full QA pass. The first section summarises the issues already fixed in
this round and how to verify them.

---

## 0. Fixes applied in this round (verify first)

### 0.1 No more unexpected logouts (global API error handling)
File: `AdminClient/src/App.vue` (response interceptor) + `AdminClient/src/helper/ApiError.js`

The interceptor previously **logged the user out on network errors** and
**redirected to an error page on 500s**. It now:

| Condition | Behaviour |
|-----------|-----------|
| `401 Unauthorized` / expired token | Clear session, redirect to Sign-In (the **only** logout case) |
| `403 Forbidden` | Show "no permission" message, stay logged in |
| `400 Bad Request` | Show validation/business message from the server body |
| `404 Not Found` | Show "record not found" message |
| `409 Conflict` | Show duplicate/conflict message |
| `500 Server Error` | Log technical details to ErrorLog, show friendly message, **stay on page** |
| Network error (status 0) | Show "Server connection failed", **do NOT log out** |

Any request may opt out of the global snackbar with `{ suppressGlobalError: true }`
in its axios config (used by forms that show the error inline).

- [ ] Stop the API, trigger a request → snackbar "Lidhja me serverin dështoi…", user stays logged in.
- [ ] Force a 500 → friendly message, an entry appears in Error Log, no redirect/logout.
- [ ] Let the JWT expire → next request logs out cleanly to Sign-In.

### 0.2 Candidate save no longer shows false success
Files: `AdminClient/src/pages/candidate/CandidateEdit.vue`,
`AdminClient/src/components/candidate/CandidateForm.vue`

The backend returns business errors as HTTP **202** with `{ Status: "error", ResponseMsg }`.
Both candidate forms previously awaited the call and **always** showed
"Successfully Saved", even on failure. They now check `Status` (via
`isBusinessError`) and only show success after real confirmation; otherwise they
display the server's message inline and in a snackbar.

- [ ] Edit a candidate so installments exceed the total → clear error, NOT a success message, stays on form.
- [ ] Valid edit → success message + navigates to the candidate view.

### 0.3 Candidate price/installment editing
File: `CandidateEdit.vue` / `CandidateForm.vue` — `validateInstallments()`

- Installment fields are **no longer permanently disabled** after registration.
- A later installment is disabled **only** when earlier ones already cover the
  full total **and** it is empty; a field holding a value always stays editable.
- Amounts are no longer force-zeroed (no data loss).
- Added a live "Paid / Remaining" summary on the edit form.

- [ ] Open a candidate who paid in full at registration → can still raise total and add a 2nd installment.
- [ ] Reduce installment 1 → installments 2/3 re-enable; remaining recalculates.
- [ ] Sum of installments > total → inline error, save blocked.

### 0.4 Backend candidate save hardening
File: `AdminApi/Controllers/CandidatesController.cs`

- `CreateCandidate` / `UpdateCandidate` now reject **duplicate personal numbers**
  (update ignores the candidate's own record).
- The `catch` blocks now **log the real exception** (`_logger.LogError`) and
  return a clear message + HTTP 500 instead of silently swallowing it.

- [ ] Create two candidates with the same personal number → second is rejected with a clear message.
- [ ] Update a candidate to a personal number used by another → rejected.

---

## 1. Error-handling acceptance criteria (apply to every module)

- [ ] No action ever logs the user out except a genuine 401 / expired token.
- [ ] Every failed save/edit/delete shows a **specific** reason, never a blank/generic toast.
- [ ] Success messages appear **only after** the backend confirms (`Status != "error"`).
- [ ] Lists refresh automatically after create/edit/delete (no manual reload).
- [ ] Loading indicators show during save/edit/delete/filter operations.
- [ ] 500 responses are logged on the backend (`_logger.LogError`) AND in the Error Log.

---

## 2. Module-by-module functional checklist

### Candidate registration (`components/candidate/CandidateForm.vue`)
- [ ] Required fields enforced (first name, last name, DOB, personal number, category, total).
- [ ] Duplicate personal number rejected with clear message.
- [ ] Installments validation: sum ≤ total; dates optional.
- [ ] Auto daily-report income entries created for installments / doc / driving payments.
- [ ] Success only on confirmed save; list refreshes.

### Candidate edit (`pages/candidate/CandidateEdit.vue`)
- [ ] Loads all existing values (personal, contact, category, instructor, payments, installments).
- [ ] Prices, total service amount, installment values are all editable.
- [ ] Disabled installment fields behave per rule 0.3.
- [ ] Paid / Remaining summary updates live.
- [ ] Save respects backend status; errors shown inline.

### Candidate payments & installments
- [ ] Increasing doc/driving payment creates a daily-report income diff entry.
- [ ] Editing installments deletes old + recreates correctly (no duplicates).
- [ ] Negative/invalid amounts rejected.

### Driving sessions (`pages/candidate/DrivingSessions.vue`)
- [ ] Create / edit / cancel a session.
- [ ] Cancelled sessions are visually distinct and excluded from active counts.
- [ ] Instructor/vehicle conflict checks fire with clear messages.

### Instructor schedule (`pages/schedule/Schedules.vue`)
- [ ] Create/edit/delete schedule events; day & week views.
- [ ] "This schedule time is already occupied for this instructor." on conflict.
- [ ] Vehicle conflict message on overlap.
- [ ] Instructors see only their own events; cannot see driving-session candidate details.
- [ ] Color-coding per instructor consistent with Dashboard calendars.

### Waiting list / Additional lessons (`pages/additional-lessons/AdditionalLessons.vue`)
- [ ] Add / edit / remove entries.
- [ ] Service payment & installments validated.
- [ ] Reflected in candidate dropdowns and schedules.

### Extra driving hours (practical lessons)
- [ ] Add practical lesson; overlap rejected ("Mbivendosje…").
- [ ] Cancel lesson; cannot cancel an already-cancelled lesson.

### E-testing — candidate accounts (`pages/etestimi/CandidateAccounts.vue`)
- [ ] Create account, reset password, edit candidate account.
- [ ] Errors surface via the local snackbar with correct colour.

### E-testing — exam management (`pages/etestimi/ExamManagement.vue`)
- [ ] Create/edit exam, upload question image, assign questions.
- [ ] Validation messages on missing fields.

### Vehicles (`pages/vehicle/Vehicles.vue`)
- [ ] CRUD; plate uniqueness; active/inactive toggle.

### Vehicle services (`pages/vehicle/VehicleServices.vue`)
- [ ] CRUD service records; cost/date validation; reflected in reports if applicable.

### Vehicle fuel (`pages/vehicle/VehicleFuel.vue`)
- [ ] Add/edit fuel entries; numeric validation.

### Daily report income/expense (`pages/report/DailyReport.vue`)
- [ ] Manual income/expense entries; totals recompute.
- [ ] Auto entries from candidate payments appear with correct source/date.
- [ ] Date filters and export work.

### Dashboard (`pages/dashboard/Dashboard.vue`)
- [ ] Stats cards load (admin only).
- [ ] "Today's Driving Sessions" and "Today's Instructor Schedule" load only today's data.
- [ ] Empty-state messages show when no sessions.
- [ ] Calendars refresh when returning to the tab.

### Instructors (`pages/instructor/Instructors.vue`)
- [ ] CRUD; role assignment; active filter.

### Users / Roles / Menus (SuperAdmin)
- [ ] CRUD with the 200/202 success/error convention respected.

### Filters, search, exports
- [ ] Search debounced; clearing resets list.
- [ ] Category/year/instructor/vehicle filters combine correctly.
- [ ] Excel/PDF exports produce correct data.
- [ ] **Instructors cannot export** restricted data (verify hidden/blocked).

### Role-based permissions
- [ ] Route guards: SuperAdmin-only and Admin-only routes redirect others to Dashboard.
- [ ] API `[Authorize(Roles=…)]` matches frontend visibility.

### Login / session handling
- [ ] Valid login → Dashboard; invalid → clear message, no crash.
- [ ] Expired token → clean logout to Sign-In.
- [ ] Refresh on a deep link keeps the user authenticated.

---

## 3. Cross-module data-flow checks
- [ ] New candidate appears in schedule/driving-session candidate dropdowns.
- [ ] Candidate payment shows in Daily Report income.
- [ ] New schedule/driving session appears on Dashboard "today" calendars.
- [ ] Cancelling a session updates dashboards and reports.
- [ ] Editing instructor/vehicle reflects in schedules and dropdowns.

---

## 4. Backend hardening backlog (recommended, not yet applied)

The candidate controller was hardened as the reference pattern. Apply the same
to the remaining controllers when convenient:

- [ ] Replace empty/generic `catch { return Accepted("error") }` with
      `_logger.LogError(ex, …)` + a clear message (see `CandidatesController`).
- [ ] Return structured `Confirmation { Status, ResponseMsg }` everywhere.
- [ ] Use 400/404/409 status codes for new endpoints; keep the existing
      202-with-Status="error" convention only where the frontend already relies on it.
- [ ] Audit `Schedules`, `DrivingSessions`, `AdditionalLessons`, `Vehicles`,
      `DailyReports`, `Instructors`, `Exams` controllers for silent catches.

---

## 5. Notes for running locally
- Frontend requires **Node 18+** (Vite). The shell default `node` here is v10;
  use `nvm use 20` before `npm run dev` / `npm run build`.
- Backend: `dotnet run` in `AdminApi` (needs NuGet restore with network access).
