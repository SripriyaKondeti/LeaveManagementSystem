# Leave Management System (LMS) – Project Guide

**Read this first.** It explains what the project is, who uses it, what each role can do, how the system is built, and the business rules behind it.
To actually run it, go to **[RUN_AND_MOBILE_GUIDE.md](RUN_AND_MOBILE_GUIDE.md)**, which covers setup, start commands and how to develop the mobile app.

---

## Contents

1. [What is this project?](#1-what-is-this-project)
2. [The two roles and what each can do](#2-the-two-roles-and-what-each-can-do)
3. [Repository layout](#3-repository-layout)
4. [Architecture](#4-architecture)
5. [The data model](#5-the-data-model)
6. [Demo data created on first start](#6-demo-data-created-on-first-start)
7. [Business rules (the important part)](#7-business-rules-the-important-part)
8. [API reference](#8-api-reference)
9. [How the API talks (envelope, errors, auth)](#9-how-the-api-talks-envelope-errors-auth)
10. [Known limitations and things to be aware of](#10-known-limitations-and-things-to-be-aware-of)
11. [Glossary](#11-glossary)

---

## 1. What is this project?

A **leave (time-off) management system** for a company. Employees request leave (annual, sick, casual…); managers approve or reject it and manage the team and the leave policies.

It has three parts in one repository:

| Part | Folder | Technology | Purpose |
|---|---|---|---|
| **Backend API** | `LeaveManagementSystem/` | ASP.NET Core Web API, **.NET 8**, EF Core, SQL Server, JWT | Owns all data and **all business rules**. |
| **Mobile app** | `LmsMobile/` | **.NET MAUI** (.NET 10), MVVM | The user interface for both roles. Contains **no business rules**, it only displays data and sends requests to the API. |
| **Unit tests** | `LeaveManagementSystem.Tests/` | xUnit | Tests for the backend services (8 tests, all passing). |

```
 Phone / emulator / Windows app             Your PC or server
┌────────────────────┐   HTTP + JSON    ┌───────────────────┐        ┌──────────────┐
│  LmsMobile (MAUI)  │ ───────────────► │ LeaveManagement   │ ─────► │  SQL Server  │
│  Employee / Manager│ ◄─────────────── │ System  (Web API) │ ◄───── │ LeaveManagementDB │
└────────────────────┘  JWT in header   └───────────────────┘        └──────────────┘
```

**Design principle:** the mobile app is a thin client. If you want to change a rule (e.g. "leave cannot start in the past"), change it in the API. The app only mirrors a few checks to give faster feedback.

---

## 2. The two roles and what each can do

The role comes from the user's account (`User.Role`: `Employee` or `Manager`) and is baked into the login token.

### Employee

Sees only **their own** data. Four tabs in the app:

| Capability | Where in the app |
|---|---|
| See remaining leave balance per leave type (e.g. "Annual Leave: 15 of 15 days") | Home |
| See how many of their requests are waiting for approval | Home |
| Request leave: choose type, from/to dates, reason (max 500 characters) | Request Leave |
| See live "Days requested" count while choosing dates | Request Leave |
| See all their requests and filter by All / Pending / Approved / Rejected | My Requests |
| See the manager's decision and comment on each request | My Requests |
| Edit their own name, email and department | Profile |
| See their employee code, joining date, status (read-only) | Profile |
| Sign out | Profile |

An employee **cannot**: see anyone else's data, approve anything, change their own active status, or reach any manager screen (the manager tabs are never even built for them, and the API would answer 403 anyway).

### Manager

Sees **everyone's** data. Four tabs plus three detail/edit pages:

| Capability | Where in the app |
|---|---|
| Company dashboard: active/total employees, pending, approved, rejected requests, total leave days taken | Home |
| Jump from dashboard to Approvals or Team | Home |
| Search the employee list by name, code or department | Team |
| Open an employee: profile, leave balances, full leave history | Team → Employee Detail |
| **Add** an employee, which also creates their login account (username, password, role) and their leave balances | Team → Add |
| **Edit** an employee, including re-activating them | Employee Detail → Edit |
| **Deactivate** an employee (soft delete, they can no longer sign in) | Employee Edit → Deactivate |
| See all **pending** requests and **Approve** or **Reject** each, with an optional comment | Approvals |
| List, **add**, **edit**, **deactivate** leave types and set each type's maximum days | Leave Types |
| Sign out | Home |

A manager account created by the seeder has **no employee record**, so the manager cannot request leave for themselves in the app.

---

## 3. Repository layout

```
LMS_Mobile/
├─ LeaveManagementSystem.slnx          Solution (API + mobile app; tests are separate)
├─ APPLICATION_GUIDE.md                This file
├─ RUN_AND_MOBILE_GUIDE.md             How to run + how to develop the mobile app
│
├─ LeaveManagementSystem/              BACKEND API  (net8.0)
│   ├─ Program.cs                      Startup: DI, JWT, Swagger, middleware, DB seeding
│   ├─ appsettings.json                Connection string + JWT settings
│   ├─ appsettings.Development.json    Development override (connection string)
│   ├─ Properties/launchSettings.json  Ports (http://localhost:5245)
│   ├─ Controllers/                    HTTP endpoints + role/ownership checks
│   │    Auth, Employee, LeaveRequest, LeaveBalance, LeaveType, Dashboard
│   ├─ Services/                       Business logic + database access
│   │    AuthService, EmployeeService, LeaveRequestService,
│   │    LeaveBalanceService, LeaveTypeService, DashboardService
│   ├─ Models/                         Database entities (Employee, User, LeaveType, LeaveBalance, LeaveRequest)
│   ├─ DTOs/                           Request/response shapes + validation attributes
│   ├─ Data/                           ApplicationDbContext (EF Core) + DbSeeder
│   ├─ Migrations/                     EF Core migrations (database schema history)
│   ├─ Middleware/                     ExceptionHandlingMiddleware, ConflictException
│   └─ Swagger/, wwwroot/              Swagger UI customisation (development only)
│
├─ LeaveManagementSystem.Tests/        xUnit tests (net8.0)
│
└─ LmsMobile/                          MOBILE APP  (net10.0, .NET MAUI)
    ├─ MauiProgram.cs                  App startup + dependency injection
    ├─ App.xaml(.cs)                   Application object, creates the window with AppShell
    ├─ AppShell.xaml(.cs)              Navigation: login route + role-based tab bars
    ├─ Models/ApiModels.cs             C# classes matching the API's JSON
    ├─ Services/                       ApiClient, ApiConfig (base URL), SessionService (token)
    ├─ ViewModels/                     One per screen: logic + data (BaseViewModel, Login, Staff/, Manager/)
    ├─ Views/                          One per screen: XAML layout (LoginPage, Staff/, Manager/)
    ├─ Converters/                     StatusToColorConverter
    ├─ Resources/                      Styles, colours, fonts, icon, splash
    └─ Platforms/                      Android / iOS / MacCatalyst / Windows native files
```

---

## 4. Architecture

### Backend (layered)

```
HTTP request
   │
   ▼
Middleware: ExceptionHandlingMiddleware ── turns exceptions into JSON + HTTP status
   │
   ▼
JWT authentication ── validates the Bearer token (issuer, audience, signature, expiry)
   │
   ▼
Controller ── checks role / "is this your own record?" then calls a service
   │
   ▼
Service ── business rules + EF Core queries, throws exceptions on rule violations
   │
   ▼
SQL Server (database "LeaveManagementDB")
```

* **Controllers decide who may call** (manager-only, own-record-only). **Services decide whether the operation is valid** (balance, overlap, dates).
* On startup the API runs `Database.MigrateAsync()` and seeds users, so **a fresh database is created automatically**.
* Delete is always a **soft delete**: employees and leave types get `IsActive = false`, nothing is physically removed.

### Mobile app (MVVM)

| Layer | Class types | Role |
|---|---|---|
| **View** | `*.xaml` + tiny `*.xaml.cs` | What the screen looks like. Binds to a ViewModel. |
| **ViewModel** | `*ViewModel.cs` | The screen's data and actions. Calls the API. |
| **Services** | `ApiClient`, `SessionService` | Networking and "who is signed in". |
| **Models** | `ApiModels.cs` | Plain classes that match the API's JSON. |

The full explanation, with step-by-step instructions for adding a screen, is in [RUN_AND_MOBILE_GUIDE.md](RUN_AND_MOBILE_GUIDE.md).

---

## 5. The data model

Database `LeaveManagementDB` on SQL Server.

| Table | Key columns | Notes |
|---|---|---|
| **Employees** | EmployeeId, EmployeeCode, Name, Email, Department, JoiningDate, IsActive | A person in the company. Code and email must be unique (checked in `EmployeeService`). |
| **Users** | UserId, Username, PasswordHash, Role, EmployeeId (nullable) | A login account. Passwords are hashed with ASP.NET `PasswordHasher`, never stored in clear text. `EmployeeId` is null for the seeded manager. |
| **LeaveTypes** | LeaveTypeId, LeaveTypeName, MaximumDays, IsActive | e.g. Annual Leave, 15 days. |
| **LeaveBalances** | LeaveBalanceId, EmployeeId, LeaveTypeId, TotalDays, UsedDays | One row per employee per leave type. **Remaining = TotalDays − UsedDays.** |
| **LeaveRequests** | LeaveRequestId, EmployeeId, LeaveTypeId, FromDate, ToDate, Reason, Status, AppliedDate, ApprovedBy, Comments | `Status` is `Pending`, `Approved` or `Rejected`. |

Relationships: Employee 1—* LeaveBalance, Employee 1—* LeaveRequest, LeaveType 1—* LeaveBalance, LeaveType 1—* LeaveRequest, User *—0..1 Employee.

---

## 6. Demo data created on first start

### Login accounts (created by `DbSeeder` only if the Users table is empty)

| Username | Password | Role | Linked employee |
|---|---|---|---|
| `priya` | `Priya@123` | Employee | Priya, EMP001, IT |
| `poojitha` | `Poojitha@123` | Employee | Poojitha, EMP002, HR |
| `arjun` | `Arjun@123` | Employee | Arjun, EMP003, Finance |
| `sai` | `Sai@123` | **Manager** | none |

### Employees (from migrations)

EMP001 Priya (IT), EMP002 Poojitha (HR), EMP003 Arjun (Finance), EMP004 Saritha (Administration). **Saritha has no login account**; she only appears in the manager's Team list. Create a login for her by adding a new employee, or use the other three accounts.

### Leave types (from migrations)

| Id | Name | Maximum days |
|---|---|---|
| 1 | Annual Leave | 15 |
| 2 | Sick Leave | 10 |
| 3 | Casual Leave | 5 |

> Quirk in the seed data: the seeded **Casual Leave balances are 12 days** per employee even though the type's maximum is 5. Saving the leave type in the manager's Leave Types screen re-syncs every balance to the type's maximum. New employees get balances equal to the type's maximum.

---

## 7. Business rules (the important part)

All enforced in the API (`Services/`). The mobile app shows the API's message when a rule is broken.

### Login
* Username and password required. The password is verified against the stored hash. A wrong username and a wrong password give the **same** message ("Invalid username or password.") so attackers cannot probe which usernames exist.
* A user whose employee is **inactive** cannot sign in ("Your account is inactive. Please contact your manager.").
* Success returns a **JWT valid for 2 hours** and the role. The token carries: user id, username, role and (if linked) an `EmployeeId` claim.

### Requesting leave (`POST api/leaves`)
In order, the request is refused when:
1. The reason is empty (max 500 characters).
2. The employee does not exist (404) or is inactive (409).
3. The leave type does not exist (404) or is inactive (400).
4. From date is after To date.
5. The leave starts or ends **before today**.
6. The remaining balance is smaller than the days requested.
7. The dates **overlap** another request of the same employee that is Pending or Approved (409).

Days are counted as **calendar days, inclusive, weekends included** (1 Nov to 3 Nov = 3 days). New requests are always created as `Pending`; the server sets the status and applied date, the client cannot.
An employee may only create requests **for themselves**; a manager may create for anyone through the API.

### Approving / rejecting (`PUT api/leaves/{id}/approve|reject`)
* Manager only. The approver's username is recorded in `ApprovedBy`; an optional comment is stored in `Comments`.
* Only **Pending** requests can be decided; deciding an already decided request gives 409.
* Cannot decide for an inactive employee. Approving also needs the leave type to be active.
* **Approve** re-checks the balance, then adds the days to `UsedDays`.
* **Reject** does **not** change the balance.
* The balance is only reduced at approval, **not** when the request is submitted.

### Employees
* Create (manager): the employee record, the login account and the balances for every active leave type are saved **together in one transaction**. Code and email must be unique; the username must be unique.
* Username: 3–50 characters, letters/digits/`.`/`_`/`-`. Password: 8–100 characters with upper case, lower case, digit and special character. Role: `Employee` or `Manager`.
* Update: managers can edit anyone and change `IsActive`; employees can edit **only themselves** and **cannot change `IsActive`**.
* Deactivate: sets `IsActive = false`. Re-activating restores the old balances and adds balances only for leave types created in the meantime.

### Leave types
* Name required and unique (case-insensitive); maximum days 1–366.
* Creating a type adds a balance to every active employee.
* Changing the maximum days **rewrites every employee's balance** for that type (and caps `UsedDays` at the new maximum).
* Deactivating a type sets every balance of that type to **0 / 0**. Re-activating restores totals to the maximum and adds missing balances.

### Leave balance
* Returns only **active** leave types. Returns 409 for an inactive employee.

---

## 8. API reference

Base URL (local): `http://localhost:5245`. Interactive docs: `http://localhost:5245/swagger` (Development only).
All endpoints except login need `Authorization: Bearer <token>`.

| Method & route | Who may call | What it does |
|---|---|---|
| `POST /api/auth/login` | anyone | Body `{username, password}` → `{token, role}` |
| `GET /api/employees` | Manager | List all employees |
| `GET /api/employees/{id}` | Manager, or the employee themselves | One employee |
| `POST /api/employees` | Manager | Create employee + login + balances |
| `PUT /api/employees/{id}` | Manager, or the employee themselves (cannot change IsActive) | Update |
| `DELETE /api/employees/{id}` | Manager | Deactivate |
| `GET /api/employees/{id}/leave-balance` | Manager, or the employee themselves | Balances per active leave type |
| `GET /api/employees/{id}/leaves` | Manager, or the employee themselves | Leave history |
| `GET /api/leaves?employeeId&leaveTypeId&status&fromDate&toDate` | Everyone (employees are forced to their own records) | Search leave requests |
| `GET /api/leaves/{id}` | Manager, or owner | One request |
| `POST /api/leaves` | Employee (self) / Manager | Request leave |
| `PUT /api/leaves/{id}/approve` | Manager | Approve, body `{comments}` |
| `PUT /api/leaves/{id}/reject` | Manager | Reject, body `{comments}` |
| `GET /api/leavetypes` | Everyone | List leave types |
| `GET /api/leavetypes/{id}` | Manager | One leave type |
| `POST /api/leavetypes` | Manager | Create |
| `PUT /api/leavetypes/{id}` | Manager | Update |
| `DELETE /api/leavetypes/{id}` | Manager | Deactivate |
| `GET /api/dashboard/leave-summary` | Manager | Counts for the dashboard |

---

## 9. How the API talks (envelope, errors, auth)

### Every response uses the same envelope

```json
{
  "statusCode": 200,
  "message": "Leave request approved successfully.",
  "data": { ... },
  "traceId": "...",          // only on errors
  "errors": { "Field": ["message"] }   // only on validation errors
}
```

The mobile app's `ApiClient` relies on this: it reads `data` on success and `errors`/`message` on failure.

### Status codes

| Code | Meaning in this API |
|---|---|
| 200 / 201 | Success / created |
| 400 | Validation or business-rule failure (bad dates, insufficient balance, DTO validation) |
| 401 | No/invalid/expired token, or wrong credentials |
| 403 | Authenticated but the role/ownership does not allow it |
| 404 | Record not found |
| 409 | Conflict with current state (already decided, overlap, duplicate, inactive) |
| 500 | Unexpected error (generic message, details only in server log) |

---

## 10. Known limitations and things to be aware of

**Security / configuration**
1. The **JWT signing key is committed in `appsettings.json`**. Move it to user-secrets or an environment variable before any real deployment.
2. The Android manifest allows **cleartext HTTP** and the app's base URL is hard-coded. Production needs HTTPS and a configurable URL.
3. No refresh tokens: after 2 hours the user is sent back to Sign In.

**Behaviour**
4. **Pending requests do not reserve balance.** Two pending requests can together exceed the balance; the second approval then fails with "Insufficient leave balance".
5. The seeded manager has **no employee record**, so managers cannot request leave in the app.
6. Seeded Casual Leave balances (12) differ from its maximum (5), see section 6.
7. Employee **Saritha (EMP004) has no login**.

**Project**
8. The mobile app still has template identity: ID `com.companyname.lmsmobile`, title "LmsMobile". Change before publishing to a store.
9. iOS and Mac Catalyst targets exist in the project but need a Mac to build and are unverified here. Windows is verified (builds with 0 errors).
10. Tests cover only the backend services (`LeaveRequestService`, `LeaveTypeService`, employee lifecycle). The mobile app has no tests.
11. The test project is not part of `LeaveManagementSystem.slnx`; run it separately (see the run guide).
12. The backend targets **.NET 8** and the mobile app **.NET 10**: both SDKs must be installed.

---

## 11. Glossary

| Term | Meaning |
|---|---|
| **API** | The backend web service the app talks to. |
| **JWT** | JSON Web Token: a signed string proving who you are and your role. Sent in the `Authorization` header. |
| **DTO** | Data Transfer Object: the shape of data going in/out of the API. |
| **EF Core / migration** | The database library and its versioned schema scripts. |
| **Soft delete** | Marking a record inactive instead of deleting it. |
| **Seed data** | Demo records created automatically on first start. |
| **MAUI** | .NET Multi-platform App UI: write one C# app for Android, iOS, Windows, macOS. |
| **MVVM** | Model-View-ViewModel: UI (View) bound to logic (ViewModel). |
| **Shell** | MAUI's navigation container (tabs, routes). |
