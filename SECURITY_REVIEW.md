# Autoshkolla — Security Review & Hardening

Scope: full-stack review of the AdminApi (.NET 8) backend and AdminClient
(Vue 3) frontend covering authentication, authorization, input/output safety,
data protection, session handling, file access, and abuse prevention.

Legend: ✅ already secure · 🔧 fixed in this round · ⚠️ action required (with
remediation below).

---

## 1. Executive summary

The application has a **solid security baseline**: JWT validation is correct,
passwords are hashed (PBKDF2), all data access goes through EF Core/LINQ (no raw
SQL concatenation), CORS is an allow-list, and most endpoints are role-gated.

The **most urgent issue** is that real secrets (production DB password and the
JWT signing key) are committed to source control. Because the JWT signing key is
public in git history, **tokens can be forged** — this must be rotated
immediately. There is also a **privilege-escalation path** in the user-management
endpoints. Details and fixes below.

---

## 2. What is already secure ✅

- **JWT validation** (`Program.cs`): validates issuer, audience, lifetime, and
  signing key; `ClockSkew = TimeSpan.Zero`; role claim = `role`, name = `sub`.
  Tokens expire after 180 min and are **never placed in URLs**.
- **Password storage**: PBKDF2 (`Rfc2898DeriveBytes`, SHA-256, 10k iterations,
  16-byte random salt). No plain-text passwords. Login responses never return
  the password/salt.
- **SQL injection**: all queries use EF Core LINQ and `SqlRepository<T>` (EF
  parameterization). No string-concatenated SQL in any controller.
- **CORS**: explicit origin allow-list (no wildcard with credentials).
- **Authorization coverage**: nearly all endpoints carry `[Authorize(Roles=…)]`.
  Admin-only modules (vehicles, vehicle services, driving sessions, additional
  lessons, exam management, candidate create/update, daily report) exclude the
  Instructor role at the API level — not just the UI.
- **IDOR on candidate details handled**: `GetCandidateDetails` enforces
  `candidate.InstructorId == currentUserId` for instructors and returns only a
  limited field set to them.
- **File download**: `DownloadApplication` is `SuperAdmin,Admin` only;
  instructors/candidates cannot pull another candidate's document.
- **XSS**: Vue auto-escapes interpolation; the PDF/HTML generators use an
  `esc()` helper before `innerHTML`. No `v-html` on user data.
- **Open redirect**: there is **no** `returnUrl`/`redirectUrl` handling; all
  navigation uses internal named routes, so redirect-based attacks are not
  possible.
- **Swagger** is protected with Basic Auth in production.

---

## 3. Fixes applied in this round 🔧

### 3.1 Clear "Access denied" instead of silent bounce / logout
- `AdminClient/src/pages/error/AccessDenied.vue` (new) — friendly page, user
  stays logged in.
- `router/index.js` — role-gated routes now redirect to `AccessDenied` instead
  of silently bouncing to the dashboard.
- Global API interceptor (`App.vue`, from the previous round) already:
  logs out **only** on `401`; shows **"Access denied"** on `403`; keeps the user
  on the page for `400/404/409/500`/network. Wording:
  - `401` → "Sesioni juaj ka skaduar. Ju lutemi kyçuni përsëri."
  - `403` → "Nuk keni leje për të kryer këtë veprim."

### 3.2 Constant-time password comparison
`AdminApi/Models/Helper/Encryption.cs` — `VerifyPassword` now uses
`CryptographicOperations.FixedTimeEquals` (was `==`), removing a timing
side-channel, and safely rejects empty/garbled stored hashes.

### 3.3 Stop committing secrets
- `.gitignore` now ignores `appsettings.json` (keeping `appsettings.Example.json`).
- `AdminApi/appsettings.Example.json` (new) is a safe template.
- **Note:** the file is still *tracked* in git history; see action 4.1.

---

## 4. Action required ⚠️

### 4.1 CRITICAL — Rotate and remove committed secrets
`AdminApi/appsettings.json` (and copies under `AdminApi/publish/…`) are tracked
in git and contain:
- the production SQL connection string **with username and password**, and
- the **JWT signing key**.

Anyone with repo access can read the DB and **forge valid JWTs for any role**.

Remediation:
1. **Rotate now**: change the DB password and generate a new `Jwt:SecretKey`
   (≥ 256-bit random, e.g. `openssl rand -base64 48`). All existing tokens become
   invalid — expected.
2. Stop tracking the file (local copy is preserved):
   ```bash
   git rm --cached AdminApi/appsettings.json \
     AdminApi/publish/appsettings.json \
     AdminApi/publish/publish/appsettings.json \
     AdminApi/publish/publish/publish/appsettings.json
   git commit -m "chore: stop tracking secret-bearing appsettings.json"
   ```
3. Provide real values via **environment variables** (ASP.NET Core overrides
   appsettings automatically — no code change needed):
   ```
   ConnectionStrings__ApiConnStringMssql="Server=…;Password=…;Encrypt=True;…"
   Jwt__SecretKey="<new base64 key>"
   Swagger__Password="<strong password>"
   ```
4. Consider purging the secrets from git history (`git filter-repo`) after
   rotation. Also set `Encrypt=True` on the connection string (currently `False`).

### 4.2 HIGH — Privilege escalation in user/role management
In `UsersController`, these endpoints allow the **`User`** role and the
`UpdateUser` action accepts a client-supplied `UserRoleId`:
```
CreateUser, UpdateUser, DeleteSingleUser,
CreateUserRole, UpdateUserRole, DeleteSingleRole   →  [Authorize(Roles="SuperAdmin,Admin,User")]
UpdateUser:  objUser.UserRoleId = model.UserRoleId;  // caller-controlled
```
If the `User` role is a regular (non-admin) account, a user can call
`UpdateUser` and set their own `UserRoleId` to SuperAdmin → full takeover.

Remediation (pick one):
- **Preferred:** remove `User` from these `[Authorize(Roles=…)]` lists so only
  `SuperAdmin,Admin` can manage users and roles.
- **Defensive (also recommended):** in `UpdateUser`/`CreateUser`, reject role
  changes unless the caller is SuperAdmin/Admin:
  ```csharp
  var callerRole = User.FindFirst("role")?.Value ?? "";
  bool callerCanSetRole = callerRole == "SuperAdmin" || callerRole == "Admin";
  if (!callerCanSetRole) objUser.UserRoleId = objUser.UserRoleId; // keep existing
  ```
Verify what the `User` role is actually used for before changing.

### 4.3 MEDIUM — Brute-force / abuse: add rate limiting
There is no rate limiting on `GetLoginInfo` (admin login),
`CandidateAuthController.Login`, `StudentRegistration`, or
`GetUserInfoForForgetPassword`. Use the built-in .NET 8 limiter:
```csharp
// Program.cs (services)
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"status\":\"error\",\"responseMsg\":\"Shumë kërkesa. Provoni përsëri pas pak.\"}", ct);
    };
    o.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;                  // 5 attempts
        opt.Window = TimeSpan.FromMinutes(1); // per minute per partition
        opt.QueueLimit = 0;
    });
});

// Program.cs (pipeline) — after UseRouting(), before MapControllers()
app.UseRateLimiter();
```
Then annotate the sensitive actions:
```csharp
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
public async Task<ActionResult> GetLoginInfo(UserInfo credential) { … }
```
For per-IP limiting use a `PartitionedRateLimiter` keyed on
`httpContext.Connection.RemoteIpAddress`.

### 4.4 MEDIUM — Forget-password flow leaks data
`GetUserInfoForForgetPassword(email)`:
- returns the reset reference (`forgetPasswordRef`) **in the HTTP response** —
  it should be emailed to the user, not returned to the caller.
- returns different responses for existing vs missing emails → **user
  enumeration**.
Remediation: always return a generic "If the email exists, a reset link was
sent." and deliver the reset token only by email.

### 4.5 MEDIUM — Anonymous `StudentRegistration`
`[AllowAnonymous] StudentRegistration` lets anyone create `User`-role accounts
(role is correctly hard-coded to 2, so no escalation, but it enables spam/abuse).
If the admin app does not need public self-registration, remove the endpoint or
gate it behind admin auth + rate limiting + CAPTCHA.

### 4.6 LOW/INFO — Audit logging
There is login history (`LogHistory`) but no dedicated audit trail for sensitive
actions. Recommended: an `AuditLog` table (`UserId, Role, Action, EntityType,
EntityId, Ip, CreatedAt, Detail`) written from a small helper and called on:
candidate create/edit, payment changes, driving-session create/edit/cancel,
vehicle service, daily-report entries, user login/logout, **failed logins**, and
**unauthorized access attempts** (log `403`s via an `IAuthorizationMiddleware`
or an action filter). Until then, at minimum add
`_logger.LogWarning("Failed login for {Email} from {IP}", …)` in the two login
methods (inject `ILogger<UsersController>` / `ILogger<CandidateAuthController>`).

### 4.7 INFO — Transport & token storage
- `RequireHttpsMetadata = false`: acceptable only behind a TLS-terminating proxy.
  Ensure the site is HTTPS-only in production (it is, per the allowed origins).
- The JWT is stored in `localStorage`, which is readable by any XSS. Given Vue's
  auto-escaping this is acceptable, but a `HttpOnly` + `Secure` + `SameSite`
  cookie would be stronger (and would then require CSRF protection — see §6).

---

## 5. Input validation & output encoding

- Backend: model binding + explicit checks (required fields, installments ≤
  total, duplicate personal number — added previously). Add `[Required]`/
  `[StringLength]`/`[RegularExpression]` data annotations on the request
  view-models (`CandidateCreateRequest`, `AddScheduleEventRequest`, etc.) so
  invalid payloads are rejected uniformly with `400` + field messages.
- Frontend: Vuetify rules + auto-escaping. Free-text fields (notes, descriptions,
  addresses) are escaped on render and via `esc()` in PDF generation.

---

## 6. CSRF

The app authenticates with a **Bearer token in the `Authorization` header**, not
cookies. CSRF is therefore **not exploitable** (an attacker's site cannot read
or set the header). If you migrate the token to a cookie (§4.7), you MUST add
anti-forgery tokens / `SameSite=Strict`.

---

## 7. Security testing checklist

| # | Test | Expected result | Status |
|---|------|-----------------|--------|
| 1 | Open an admin page (`/users`, `/vehicles`) with an Instructor by typing the URL | Redirected to **Access denied** page; not logged out | ✅ guard added |
| 2 | Call an admin API (e.g. `POST /api/Vehicles/CreateVehicle`) with an Instructor token (Postman) | `403`; "Nuk keni leje…" | ✅ role-gated |
| 3 | Create/edit/delete with **no** token | `401`; never reaches the DB | ✅ `[Authorize]` |
| 4 | Same with an **expired** token | `401` → "Session expired"; clean logout | ✅ |
| 5 | Change a record ID in an API URL (e.g. another candidate's id) | Admin allowed; Instructor blocked by ownership check | ✅ candidate details; ⚠️ audit other GET-by-id endpoints |
| 6 | Download another candidate's document by changing the id | Only `SuperAdmin,Admin`; instructors/candidates `403` | ✅ |
| 7 | Submit `<script>`/HTML in name/notes/address | Stored as text, escaped on render & PDF | ✅ |
| 8 | SQL-injection payloads in search/form fields | Treated as literal values (EF parameterization) | ✅ |
| 9 | External redirect URL after login | No redirect parameters exist; always lands on internal route | ✅ |
| 10 | Repeated wrong-password logins | Should be throttled | ⚠️ implement §4.3 |
| 11 | Create activities via raw API calls | Requires valid token + role + model + business rules | ✅ (strengthen with §5 annotations) |
| 12 | Read repo for secrets | Must find none | ⚠️ do §4.1 (rotate + untrack) |
| 13 | Forge a JWT using the committed key | Must be impossible | ⚠️ blocked only after §4.1 key rotation |
| 14 | Escalate role via `UpdateUser` | Must be rejected for non-admins | ⚠️ implement §4.2 |

---

## 8. Priority order
1. **§4.1** rotate + untrack secrets (CRITICAL — enables token forgery & DB access).
2. **§4.2** close the privilege-escalation path.
3. **§4.3** login rate limiting.
4. **§4.4 / §4.5** forget-password leak & anonymous registration.
5. **§4.6** audit logging.
6. **§5** request-model validation annotations.
