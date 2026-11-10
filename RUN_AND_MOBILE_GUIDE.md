# LMS – Run Guide & Mobile App Developer Guide

This guide takes someone who has just received the **zip file** or the **GitHub link** from nothing to a running system, and then explains how the mobile app is built so they can change it.

> New here? Read **[APPLICATION_GUIDE.md](APPLICATION_GUIDE.md)** first for *what* the project is and what each role can do. This file is the *how*.

## Contents

**Part A: Running it**
1. [What you will run](#1-what-you-will-run)
2. [Prerequisites](#2-prerequisites)
3. [Get the code](#3-get-the-code)
4. [Quick start (copy-paste)](#4-quick-start-copy-paste)
5. [Detailed steps](#5-detailed-steps)
6. [Run on an Android emulator](#6-run-on-an-android-emulator)
7. [Run on a real phone](#7-run-on-a-real-phone)
8. [Smoke test: prove everything works](#8-smoke-test-prove-everything-works)
9. [Troubleshooting](#9-troubleshooting)

**Part B: Understanding and developing the mobile app**
10. [Mobile concepts in plain words](#10-mobile-concepts-in-plain-words)
11. [Project structure](#11-project-structure-of-lmsmobile)
12. [What happens when the app starts](#12-what-happens-when-the-app-starts)
13. [Navigation (AppShell)](#13-navigation-appshell)
14. [The MVVM pattern, using Login as the example](#14-the-mvvm-pattern-using-login-as-the-example)
15. [The shared building blocks](#15-the-shared-building-blocks)
16. [Every screen](#16-every-screen)
17. [Walkthrough: following one feature end to end](#17-walkthrough-following-one-feature-end-to-end)
18. [Tutorial: add a new screen](#18-tutorial-add-a-new-screen)
19. [Common changes](#19-common-changes-cookbook)
20. [Build for release](#20-building-for-release)
21. [Command cheat sheet](#21-command-cheat-sheet)

---

# Part A: Running it

## 1. What you will run

Two programs, started in this order:

| # | Program | Folder | Listens on | Needs |
|---|---|---|---|---|
| 1 | **Backend API** | `LeaveManagementSystem/` | `http://localhost:5245` | SQL Server, .NET 8 |
| 2 | **Mobile app** | `LmsMobile/` | n/a (it is the client) | .NET 10 + MAUI workload, the API running |

The mobile app cannot do anything without the API; the API cannot do anything without SQL Server.

## 2. Prerequisites

| Requirement | Version | Why | Check with |
|---|---|---|---|
| **Windows 10/11** | 64-bit | The Windows and Android targets are built on Windows (iOS/macOS builds need a Mac) | |
| **.NET 8 SDK** | 8.0.x | Backend targets `net8.0` | `dotnet --list-sdks` |
| **.NET 10 SDK** | 10.0.x | Mobile app targets `net10.0-*` | `dotnet --list-sdks` |
| **.NET MAUI workload** | matching .NET 10 | Compiles the mobile app | `dotnet workload list` |
| **SQL Server** | Express, Developer, or LocalDB | Database | see below |
| **Git** | any | Only to clone | `git --version` |
| **IDE (recommended)** | Visual Studio 2022/2026 with the **".NET Multi-platform App UI development"** workload | One-click run, Android emulator manager, hot reload | |
| **Android emulator** *(only for the Android target)* | any API 21+ image | Virtual phone | Visual Studio: Tools → Android → Android Device Manager |

You need **both** the .NET 8 and the .NET 10 SDKs. `dotnet --list-sdks` should show an `8.0.xxx` and a `10.0.xxx` line.

**Install the MAUI workload without Visual Studio** (needs an elevated PowerShell):

```powershell
dotnet workload install maui
```

**SQL Server:** if you have none, install **SQL Server Express** or **LocalDB** (comes with Visual Studio). The default connection string expects an instance at `localhost`; see [step 5.1](#51-database) to change it.

## 3. Get the code

**From GitHub**

```powershell
git clone https://github.com/SripriyaKondeti/LeaveManagementSystem.git
cd LeaveManagementSystem
git checkout feature/mobile-app
```

The mobile app lives on the `feature/mobile-app` branch. If there is **no `LmsMobile` folder**, you are on the wrong branch.

**From a zip file**

Extract it somewhere with a **short path** such as `C:\dev\LMS_Mobile` (very long paths, e.g. deep inside OneDrive, can break .NET builds). If the zip contains `bin` and `obj` folders you may delete them, they are rebuilt automatically.

After either, you should see:

```
LeaveManagementSystem/        <- backend
LeaveManagementSystem.Tests/  <- tests
LmsMobile/                    <- mobile app
LeaveManagementSystem.slnx
```

All commands below are run **from this top folder** unless a `cd` is shown.

## 4. Quick start (copy-paste)

**Terminal 1: backend** (leave it running)

```powershell
cd LeaveManagementSystem
dotnet run --launch-profile http
```

Wait for `Now listening on: http://localhost:5245`.

**Terminal 2: mobile app on Windows** (no emulator needed)

```powershell
cd LmsMobile
dotnet run -f net10.0-windows10.0.19041.0
```

Sign in with **`sai` / `Sai@123`** (manager) or **`priya` / `Priya@123`** (employee).

That is the whole setup. The sections below explain each step and what to do when something fails.

## 5. Detailed steps

### 5.1 Database

The API connects with the string in `LeaveManagementSystem/appsettings.json` (and the identical one in `appsettings.Development.json`):

```
Server=localhost;Database=LeaveManagementDB;Trusted_Connection=True;TrustServerCertificate=True;
```

This means: SQL Server **default instance on this PC**, Windows authentication, database `LeaveManagementDB`.

**You do not create the database or run any SQL script.** On startup the API applies the EF Core migrations (`MigrateAsync`) which creates the tables and the demo data.

If your SQL Server is different, edit **`appsettings.Development.json`** (it overrides `appsettings.json` in Development):

| Your setup | Connection string |
|---|---|
| Default instance (the default) | `Server=localhost;Database=LeaveManagementDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Express | `Server=.\\SQLEXPRESS;Database=LeaveManagementDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| LocalDB | `Server=(localdb)\\MSSQLLocalDB;Database=LeaveManagementDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL login | `Server=localhost;Database=LeaveManagementDB;User Id=sa;Password=<pwd>;TrustServerCertificate=True;` |

(In JSON the backslash must be doubled, as shown.)

Check SQL Server is running: `Get-Service MSSQLSERVER` (default instance) or `sqllocaldb info` (LocalDB).

**Start over with a clean database** (drops all data, the next API start recreates and re-seeds it):

Stop the API first, then:

```powershell
sqlcmd -S localhost -E -C -Q "ALTER DATABASE LeaveManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE LeaveManagementDB"
```

(`sqlcmd` ships with SQL Server tools; alternatively right-click the database in SQL Server Management Studio → Delete.)

### 5.2 Backend API

```powershell
cd LeaveManagementSystem
dotnet run --launch-profile http
```

* The first run restores packages (can take a minute), creates the database and seeds the demo data.
* Listens on **http://localhost:5245**.
* **Swagger UI** (interactive API docs and a way to try the API without the app): http://localhost:5245/swagger
* In Visual Studio: open `LeaveManagementSystem.slnx`, set `LeaveManagementSystem` as the startup project, choose the **http** profile and press F5.

**Check it works** (from another terminal):

```powershell
$body = '{"username":"sai","password":"Sai@123"}'
Invoke-RestMethod -Method Post -Uri http://localhost:5245/api/auth/login -ContentType "application/json" -Body $body
```

Expected: an object with `statusCode 200` and `data.token` / `data.role = Manager`.

**Backend tests** (optional, no API or DB needed, uses an in-memory database):

```powershell
dotnet test LeaveManagementSystem.Tests
```

Expected: `Passed! Failed: 0, Passed: 8`.

### 5.3 Mobile app

The app picks the API address automatically (`LmsMobile/Services/ApiConfig.cs`):

| App runs on | API address used |
|---|---|
| Android **emulator** | `http://10.0.2.2:5245/` (10.0.2.2 is how an emulator reaches your PC's `localhost`) |
| **Windows** desktop | `http://localhost:5245/` |
| Real phone | you must edit it, see [section 7](#7-run-on-a-real-phone) |

**Windows (quickest)**

```powershell
cd LmsMobile
dotnet run -f net10.0-windows10.0.19041.0
```

This builds and launches the app as a desktop window. (Use `dotnet run`, **not** `dotnet build -t:Run`: the latter fails with "'C:\Users\...' is not recognized as an internal or external command" when the project path contains spaces.)

**Visual Studio (any target)**
1. Open `LeaveManagementSystem.slnx`.
2. Right-click `LmsMobile` → *Set as Startup Project*.
3. In the toolbar pick **Windows Machine** or an **Android emulator**.
4. Press **F5**. (Visual Studio can start both projects: *Solution → Configure Startup Projects → Multiple startup projects*.)

**Visual Studio Code:** install the *C# Dev Kit* and *.NET MAUI* extensions, open the folder, and use the status-bar target picker, or run the `dotnet` commands above in the terminal.

## 6. Run on an Android emulator

1. **Create an emulator** (one time): Visual Studio → *Tools → Android → Android Device Manager* → *New* → pick e.g. *Pixel 7*, a recent API level → *Create*. (Requires Windows Hypervisor Platform or Hyper-V enabled.)
2. **Start it** from the Device Manager and wait for the home screen.
3. **Start the API** (Terminal 1, section 4).
4. **Run the app on it**

```powershell
cd LmsMobile
dotnet build -f net10.0-android -t:Run
```

   or pick the emulator in Visual Studio and press F5. The first Android build is slow (several minutes).

The app reaches your PC's API through `10.0.2.2:5245`. Plain HTTP works because the Android manifest sets `usesCleartextTraffic="true"` (a development convenience).

## 7. Run on a real phone

The phone and PC must be on the **same Wi-Fi**.

1. Find the PC's IP address: `ipconfig`, then note the IPv4 address, e.g. `192.168.1.25`.
2. Edit `LmsMobile/Services/ApiConfig.cs`:

```csharp
public static string BaseUrl => "http://192.168.1.25:5245/";
```

3. Make the API listen on all network interfaces (not just localhost):

```powershell
cd LeaveManagementSystem
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --no-launch-profile --urls http://0.0.0.0:5245
```

4. Allow the port through Windows Firewall (elevated PowerShell):

```powershell
New-NetFirewallRule -DisplayName "LMS API 5245" -Direction Inbound -Protocol TCP -LocalPort 5245 -Action Allow
```

5. Enable *Developer options → USB debugging* on the phone, connect via USB, choose the phone as the run target in Visual Studio, press F5.

## 8. Smoke test: prove everything works

Do this once after setup. It exercises both roles and the main rules. (Dates are relative to today.) The numbers below assume a **fresh database** (first API start). If someone already used the system, the counts and balances will differ; to reset see [section 5.1](#51-database).

**As employee `priya` / `Priya@123`**
1. **Home** shows "Hello, Priya" and three balance cards (Annual 15, Sick 10, Casual 12).
2. **Request Leave** → type *Annual Leave*, From = tomorrow, To = the day after, reason "Family event" → *Submit* → "Your leave request is pending approval" → you land on **My Requests** and see a **Pending** (orange) request for 2 days.
3. Try submitting the **same dates again** → error "…overlap with an existing leave request."
4. **Profile** → change the department → *Save changes* → "Your details were updated." → *Sign Out*.

**As manager `sai` / `Sai@123`**
5. **Home** shows employees 4 active of 4 and **Pending requests: 1**. Tap the tile.
6. **Approvals** shows Priya's request → *Approve* → type a comment → confirm. The list is now empty ("You're all caught up").
7. **Team** → search "pri" → open Priya → **Leave balance** shows Annual Leave 13 remaining of 15, **Leave history** shows the request as Approved with your comment.
8. **Team → Add**: code `EMP005`, name, email, department, username `newuser`, password `Passw0rd!`, role Employee → *Save*. Sign out and sign in as `newuser` to confirm the account works.
9. **Leave Types** → *Add* → "Study Leave", 5 days → *Save*. It appears in the list, and every active employee now has a Study Leave balance.

**As employee again:** sign in as `priya`, **My Requests** now shows the request as **Approved** (green) with "Manager: <your comment>", and **Home** shows Annual Leave 13 of 15.

## 9. Troubleshooting

| Symptom | Cause and fix |
|---|---|
| App shows "Cannot reach the server. Check your connection and that the API is running." | API not running, or wrong address for your target (section 5.3). Open `http://localhost:5245/swagger` in a browser on the PC to confirm. Emulator must use `10.0.2.2`. Real phone: firewall + LAN IP (section 7). |
| App shows "The server took too long to respond" | 20-second timeout hit. Is the API stuck starting or the database slow? |
| API crashes on startup with a SQL error | SQL Server not running or wrong connection string (section 5.1). |
| API: `Cannot open database … login failed` | Windows account has no SQL login. Use a SQL login or add your user in SQL Server. |
| API: "JWT key is not configured." | `Jwt:Key` is missing from `appsettings.json`. |
| `dotnet run` says SDK/runtime not found for net8.0 | Install the .NET 8 SDK (section 2). |
| Mobile build: "workload … not installed" / "Android SDK not found" | `dotnet workload install maui` (elevated) or install the VS MAUI workload. |
| Mobile build for Android: `adb`/emulator missing | Create an emulator in Android Device Manager (section 6). |
| Login says "Invalid username or password." | Check the demo password casing (`Sai@123`). If the DB was created before the seeder ran, drop it (section 5.1) and restart the API. Note demo accounts are only created when the Users table is empty. |
| Login says "Your account is inactive." | The employee was deactivated; a manager can re-activate them in Team → Edit. |
| Session expired alert pops up | The token lasts 2 hours, or the API's JWT key changed. Sign in again. |
| Port 5245 already in use | Stop the other API instance, or run with `--urls http://localhost:5300` and update `ApiConfig.cs`. |
| App keeps auto-logging in as a previous user | Press *Sign Out* (clears the saved token). |
| Weird build errors after switching branches | Delete the `bin` and `obj` folders in `LmsMobile` and rebuild. |
| Build fails with "path too long" | Move the project to a short folder such as `C:\dev\LMS_Mobile`. |
| `dotnet build -t:Run` fails with "'C:\Users\…' is not recognized as an internal or external command" | The project path contains spaces. Use `dotnet run -f net10.0-windows10.0.19041.0` instead, or move the project to a path without spaces. |

---

# Part B: Understanding and developing the mobile app

## 10. Mobile concepts in plain words

Mapped to things a web/API developer already knows.

| Term | Meaning | Closest web idea |
|---|---|---|
| **.NET MAUI** | Microsoft's framework to write one C# app for Android, iOS, Windows and macOS | ASP.NET, but for apps |
| **XAML** (`.xaml`) | XML that describes a screen | HTML / Razor markup |
| **Page** (`ContentPage`) | One screen: a `.xaml` file + a small `.xaml.cs` code-behind | A view / page |
| **ViewModel** | A class holding a screen's data and actions | A PageModel / controller for that page |
| **Data binding** | `Text="{Binding Username}"` keeps a control and a ViewModel property in sync | Model binding, but live and two-way |
| **Command** | A method a button runs: `Command="{Binding SignInCommand}"` | A form action / handler |
| **Shell** | The navigation container (tabs, routes, back stack) | A router |
| **Route** | A navigation address such as `//approvals` or `employeedetail?id=5` | A URL |
| **Dependency injection** | Same as ASP.NET; registered in `MauiProgram.cs` | `Program.cs` services |
| **SecureStorage** | OS-protected key/value store (Android Keystore, Windows credential locker) | A secure cookie |
| **Emulator** | A virtual phone on the PC | A browser dev-tools device mode, but a full OS |
| **`Platforms/` folder** | Native per-OS files (manifests, entry points) that you rarely touch | |
| **CommunityToolkit.Mvvm** | NuGet package that generates boilerplate: `[ObservableProperty]` makes a property that notifies the UI; `[RelayCommand]` makes a command from a method | Source generators |

**One project, many targets.** `LmsMobile.csproj` lists `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`. The same C# + XAML compiles for each. On Windows only Android and Windows can be built (iOS/Mac need a Mac).

**Key packages** (`LmsMobile.csproj`): `Microsoft.Maui.Controls`, `CommunityToolkit.Mvvm` (8.4.2), `Microsoft.Extensions.Http` (typed `HttpClient`), `System.IdentityModel.Tokens.Jwt` (read token claims).

## 11. Project structure of LmsMobile

```
LmsMobile/
├─ MauiProgram.cs        App startup + dependency injection (the "Program.cs")
├─ App.xaml / .cs        The Application; CreateWindow() returns a Window with AppShell
├─ AppShell.xaml / .cs   Navigation: the login route + role-based tab bars built in code
│
├─ Models/ApiModels.cs   C# classes matching the API JSON (+ display helpers like DateRange)
│
├─ Services/
│   ├─ ApiConfig.cs      Base URL (per platform) + ApiException
│   ├─ ApiClient.cs      HttpClient wrapper + AuthHeaderHandler (adds the Bearer token)
│   └─ SessionService.cs Holds/saves the JWT, role, employee id, expiry
│
├─ ViewModels/
│   ├─ BaseViewModel.cs  Shared: IsBusy, ErrorMessage, pull-to-refresh, RunAsync()
│   ├─ LoginViewModel.cs
│   ├─ Staff/            EmployeeHome, RequestLeave, MyRequests, Profile
│   └─ Manager/          ManagerHome, Team, EmployeeDetail, EmployeeEdit,
│                        Approvals, LeaveTypes, LeaveTypeEdit
│
├─ Views/                The XAML screens, mirrors ViewModels/ (LoginPage, Staff/, Manager/)
├─ Converters/           StatusToColorConverter (Approved=green, Rejected=red, other=orange)
├─ Resources/            Styles (Colors.xaml, Styles.xaml, AppStyles.xaml), fonts, icon, splash
└─ Platforms/            Android / iOS / MacCatalyst / Windows native bits
```

Rule of thumb: **every screen = one View + one ViewModel**, same name, parallel folders.

## 12. What happens when the app starts

```
OS launches the app
 └─ MauiProgram.CreateMauiApp()
      · registers SessionService (singleton), ApiClient (typed HttpClient, 20 s timeout,
        base address from ApiConfig, AuthHeaderHandler attached)
      · registers every Page and ViewModel as transient
 └─ App.CreateWindow()  →  new Window(new AppShell())
 └─ AppShell shows its only static content: the "login" route → LoginPage
 └─ LoginPage.Loaded  →  LoginViewModel.TryAutoLoginAsync()
        ├─ SessionService.RestoreAsync(): saved token in SecureStorage and not expired?
        │     └─ yes → AppShell.GoHomeAsync()  (skip the login screen)
        └─ no  → user types credentials → SignInCommand
```

Why `Loaded`? It fires after Shell has finished building, so navigating from it cannot collide with startup (see the comment in `LoginPage.xaml.cs`).

## 13. Navigation (AppShell)

`AppShell.xaml` declares only the login screen:

```xml
<ShellContent Route="login" Shell.NavBarIsVisible="False"
              ContentTemplate="{DataTemplate views:LoginPage}" />
```

The tab bars are built **in code**, after sign-in, so a user only ever has the tabs of their role:

* `AppShell.GoHomeAsync(session)` removes old tab bars, adds `BuildManagerTabs()` or `BuildEmployeeTabs()` depending on `session.IsManager`, then navigates to `//manager` or `//employee`.
* `AppShell.SignOutAsync()` navigates to `//login` and removes the tabs.
* `AppShell.SessionExpiredAsync()` signs out and shows the "Session expired" alert; `ApiClient` calls it on the first 401.

| Role | Tab bar route | Tabs (title → route) |
|---|---|---|
| Employee | `//employee` | Home → `employeehome`, Request Leave → `requestleave`, My Requests → `myrequests`, Profile → `profile` |
| Manager | `//manager` | Home → `managerhome`, Team → `team`, Approvals → `approvals`, Leave Types → `leavetypes` |

**Pages that are not tabs** are registered in the `AppShell` constructor and opened with a query string:

```csharp
Routing.RegisterRoute("employeedetail", typeof(EmployeeDetailPage));
Routing.RegisterRoute("employeeedit",   typeof(EmployeeEditPage));
Routing.RegisterRoute("leavetypeedit",  typeof(LeaveTypeEditPage));
```

```csharp
Shell.Current.GoToAsync("employeedetail?id=5");   // push, value arrives via [QueryProperty]
Shell.Current.GoToAsync("//approvals");           // jump to a tab (absolute route)
Shell.Current.GoToAsync("..");                    // go back one page
```

`id=0` means "new" on the edit pages.

## 14. The MVVM pattern, using Login as the example

Four files cooperate for every screen:

**1. The View: `Views/LoginPage.xaml`** is the layout. Controls bind to ViewModel properties:

```xml
<Entry Placeholder="Username" Text="{Binding Username}" />
<Entry Placeholder="Password" IsPassword="True" Text="{Binding Password}" />
<Label Text="{Binding ErrorMessage}" IsVisible="{Binding HasError}" Style="{StaticResource ErrorText}" />
<Button Text="Sign In" Command="{Binding SignInCommand}" />
```

**2. The code-behind: `Views/LoginPage.xaml.cs`** gets the ViewModel injected and makes it the `BindingContext`:

```csharp
public LoginPage(LoginViewModel vm)
{
    InitializeComponent();
    BindingContext = _vm = vm;
}
```

**3. The ViewModel: `ViewModels/LoginViewModel.cs`** holds the data and the action:

```csharp
[ObservableProperty] private string username = "";          // generates property Username
[ObservableProperty] private string password = "";

[RelayCommand]                                              // generates SignInCommand
private Task SignInAsync() => RunAsync(async () =>
{
    // validate → POST api/auth/login → save session → AppShell.GoHomeAsync
});
```

**4. The registration: `MauiProgram.cs`**

```csharp
builder.Services.AddTransient<LoginPage>();
builder.Services.AddTransient<LoginViewModel>();
```

DI builds the page, sees it needs a `LoginViewModel`, builds that (which needs `ApiClient` and `SessionService`), and hands it over. **Every other screen repeats exactly this pattern.**

Data screens add one thing: the code-behind loads data each time the page appears:

```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();
    await _vm.LoadAsync();
}
```

## 15. The shared building blocks

### BaseViewModel
Every screen's ViewModel inherits it. It gives each screen:

| Member | Use |
|---|---|
| `IsBusy` | Bind to an `ActivityIndicator` (spinner) |
| `ErrorMessage` / `HasError` | Bind to a red label under the form |
| `LoadAsync()` | Calls your `LoadCoreAsync()` wrapped in error handling |
| `RefreshCommand` / `IsRefreshing` | Bind to `RefreshView` for pull-to-refresh |
| `RunAsync(action)` | Wrap any button action: sets busy, clears the old error, catches `ApiException` and shows **its message**, catches anything else as "Something went wrong. Please try again." Ignores a second tap while busy. |
| `LoadCoreAsync()` *(override)* | Fetch the data your screen needs |

### ApiClient
The only class that talks to the network.

```csharp
await _api.GetAsync<List<LeaveType>>("api/leavetypes");
await _api.PostAsync<LeaveRequest>("api/leaves", new CreateLeaveRequest { ... });
await _api.PutAsync<Employee>($"api/employees/{id}", employee);
await _api.DeleteAsync($"api/leavetypes/{id}");
```

What it does for you:
* Adds `Authorization: Bearer <token>` to every request (`AuthHeaderHandler`).
* Unwraps the `{statusCode, message, data, errors}` envelope and **returns just `data`**.
* Turns failures into `ApiException(statusCode, message)`: "Cannot reach the server…", "The server took too long…", the server's validation errors (joined by new lines) or its `message`.
* On the **first 401** it clears the session and sends the user to Sign In with "Session expired" (guarded so several parallel 401s only trigger it once).

### SessionService
Singleton that knows who is signed in.
* `SaveAsync(token, role)` reads the `EmployeeId` claim and expiry from the JWT and stores the token and role in **SecureStorage**.
* `RestoreAsync()` runs at launch; returns true when a saved token exists and has not expired.
* `IsManager` (role == "Manager") decides which tabs to build. `EmployeeId` is what employee screens use to call `api/employees/{id}`.
* `Clear()` / `TryEnd()` wipe it (sign-out / 401).

### Models (`ApiModels.cs`)
Plain classes matching the API JSON (`Employee`, `LeaveType`, `LeaveBalance`, `LeaveRequest`, `LeaveSummary`, `LoginRequest`…), JSON names are matched case-insensitively. Properties marked `[JsonIgnore]` are display helpers computed on the phone (e.g. `LeaveRequest.DateRange` = "12 Nov – 14 Nov 2026 (3 days)", `Employee.StatusText`). `LeaveTypeName` and `EmployeeName` on `LeaveRequest` are filled in by the ViewModels after joining with other API calls.

### Styles and colours (`Resources/Styles/`)
| File | Contains |
|---|---|
| `Colors.xaml` | Brand palette (`Primary` = #512BD4 purple…) |
| `Styles.xaml` | Default look of built-in controls (Button, Entry…) from the MAUI template |
| `AppStyles.xaml` | This app's styles: `Card`, `Tag`, `ErrorText`, `MutedText`, `PageBackground`, plus `StatusColor` converter and light/dark colours |

Use them in XAML: `Style="{StaticResource Card}"`.

### StatusToColorConverter
Maps a status string to a colour: Approved/Active → green, Rejected → red, Inactive → gray, anything else (Pending) → orange. Used as `BackgroundColor="{Binding Status, Converter={StaticResource StatusColor}}"`.

## 16. Every screen

13 screens: 1 shared, 4 for employees, 8 for managers.

### Shared

| View | ViewModel | Does | API |
|---|---|---|---|
| **LoginPage** (`login`) | LoginViewModel | Username/password, Sign In, auto-login from saved token | `POST api/auth/login` |

### Employee (tab bar `employee`)

| View (route) | ViewModel | What the user sees / does | API calls |
|---|---|---|---|
| **EmployeeHomePage** (`employeehome`) | EmployeeHomeViewModel | "Hello, <name>", a card per leave type with remaining days ("of N days"), number of requests waiting for approval, *Request Leave* button | `GET employees/{id}`, `GET employees/{id}/leave-balance`, `GET leaves?status=Pending` (in parallel) |
| **RequestLeavePage** (`requestleave`) | RequestLeaveViewModel | Leave-type picker (active types only), From/To date pickers (cannot pick the past; changing From moves To forward), live "Days requested: N", reason box (max 500, with counter), Submit | `GET leavetypes`, `POST leaves` |
| **MyRequestsPage** (`myrequests`) | MyRequestsViewModel | Filter picker (All/Pending/Approved/Rejected), cards with type, colour-coded status tag, date range, reason, manager comment; newest first | `GET leaves[?status=…]`, `GET leavetypes` |
| **ProfilePage** (`profile`) | ProfileViewModel | Editable name/email/department; read-only code, joining date, status; *Save changes*; *Sign Out* | `GET employees/{id}`, `PUT employees/{id}` |

Client-side checks before calling the API: leave type chosen, dates in order, not before today, reason not empty; profile fields required and email contains `@`. The API re-checks everything.

### Manager (tab bar `manager`)

| View (route) | ViewModel | What the user sees / does | API calls |
|---|---|---|---|
| **ManagerHomePage** (`managerhome`) | ManagerHomeViewModel | Overview tiles: active employees "of N", **Pending** (tap → Approvals), Approved, Rejected, leave days taken; *Sign Out* | `GET dashboard/leave-summary` |
| **TeamPage** (`team`) | TeamViewModel | Search box (name, code, department, filtering happens on the phone), alphabetical list with an "Inactive" badge, *Add* button; tap a row → detail | `GET employees` |
| **ApprovalsPage** (`approvals`) | ApprovalsViewModel | Pending requests oldest-start first: employee, type, dates, reason, **Approve / Reject**. Each opens a prompt for an optional comment; Cancel aborts. List reloads afterwards even on error (another manager may have decided it) | `GET leaves?status=Pending`, `GET employees`, `GET leavetypes`; `PUT leaves/{id}/approve` or `/reject` |
| **LeaveTypesPage** (`leavetypes`) | LeaveTypesViewModel | Active types first; max days; *Add*, tap to edit, *Deactivate* (confirmation warns balances reset to 0) | `GET leavetypes`, `DELETE leavetypes/{id}` |
| **EmployeeDetailPage** (`employeedetail?id=N`) | EmployeeDetailViewModel | Name/status/email, balances (a note if the employee is inactive), full history with comments, *Edit* | `GET employees/{id}`, `…/leaves`, `GET leavetypes`, `…/leave-balance` |
| **EmployeeEditPage** (`employeeedit?id=N`, 0 = new) | EmployeeEditViewModel | Code, name, email, department, joining date, Active switch. **New only:** username, password (rules shown), role (Employee/Manager). *Save*; *Deactivate* (existing & active only, with confirmation) | `POST employees`, `PUT employees/{id}`, `DELETE employees/{id}` |
| **LeaveTypeEditPage** (`leavetypeedit?id=N`, 0 = new) | LeaveTypeEditViewModel | Name, maximum days (1–366), Active switch; warns before changes that rewrite every balance | `GET leavetypes/{id}`, `POST leavetypes`, `PUT leavetypes/{id}` |

### Kinds of UI used

| Kind | Where |
|---|---|
| Tab pages (8) | Home/Request Leave/My Requests/Profile (employee); Home/Team/Approvals/Leave Types (manager) |
| Pushed pages (3) | Employee Detail, Employee Edit, Leave Type Edit |
| Login page (1) | No tab bar |
| Lists | `RefreshView` + `CollectionView` with an `EmptyView` message (pull to refresh) |
| Forms | `Entry`, `Editor`, `Picker`, `DatePicker`, `Switch`, `Button` |
| Cards / tiles | `Border` with the `Card` style |
| Native dialogs | `DisplayAlertAsync` (confirmations, "Saved", "Submitted", "Session expired"), `DisplayPromptAsync` (approve/reject comment) |

## 17. Walkthrough: following one feature end to end

**"Manager approves a leave request"**

1. Manager taps the **Approvals** tab. `ApprovalsPage.OnAppearing` calls `ApprovalsViewModel.LoadAsync()`.
2. `LoadCoreAsync` fires three API calls **in parallel** (`Task.WhenAll`): pending requests, all employees, all leave types. It joins them in memory so each request shows the employee name and leave type name (the API returns only ids).
3. Each card's **Approve** button is bound to `ApproveCommand` and passes the request as the parameter.
4. `DecideAsync` shows a prompt (comment optional). Cancel returns `null` → nothing happens.
5. `RunAsync` sends `PUT api/leaves/{id}/approve` with `{ comments }` through `ApiClient` (token added automatically).
6. **Backend:** `LeaveRequestController` checks the role is Manager and stamps `ApprovedBy` from the token's username → `LeaveRequestService.ApproveLeaveRequestAsync` re-validates (status Pending, employee active, leave type active, enough balance) → sets `Approved`, adds the days to `UsedDays`, saves.
7. Back in the app, the list reloads and the request disappears. If the API refused (e.g. 409 "already approved"), `ErrorMessage` is shown above the list after the reload.
8. The employee's **My Requests** now shows it green with the comment, and their **Home** balance went down.

## 18. Tutorial: add a new screen

Goal: add a read-only **"Leave Policy"** tab for employees that lists the active leave types and their maximum days. (This exact code was compiled against this project and builds with 0 errors.)

### Step 1: ViewModel, `LmsMobile/ViewModels/Staff/PolicyViewModel.cs`

```csharp
using System.Collections.ObjectModel;
using LmsMobile.Models;
using LmsMobile.Services;

namespace LmsMobile.ViewModels.Staff;

public partial class PolicyViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ObservableCollection<LeaveType> LeaveTypes { get; } = new();

    public PolicyViewModel(ApiClient api) => _api = api;

    protected override async Task LoadCoreAsync()
    {
        var types = await _api.GetAsync<List<LeaveType>>("api/leavetypes");

        LeaveTypes.Clear();
        foreach (var t in types.Where(t => t.IsActive))
            LeaveTypes.Add(t);
    }
}
```

### Step 2: View, `LmsMobile/Views/Staff/PolicyPage.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="LmsMobile.Views.Staff.PolicyPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:vm="clr-namespace:LmsMobile.ViewModels.Staff"
    xmlns:m="clr-namespace:LmsMobile.Models"
    x:DataType="vm:PolicyViewModel"
    Style="{StaticResource PageBackground}"
    Title="Leave Policy">

    <Grid RowDefinitions="Auto,*" Padding="18,12,18,0">
        <Label Text="{Binding ErrorMessage}" IsVisible="{Binding HasError}"
               Style="{StaticResource ErrorText}" />

        <RefreshView Grid.Row="1" Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsRefreshing}">
            <CollectionView ItemsSource="{Binding LeaveTypes}">
                <CollectionView.EmptyView>
                    <Label Text="No leave types available." Style="{StaticResource MutedText}"
                           HorizontalOptions="Center" Margin="0,40" />
                </CollectionView.EmptyView>
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="m:LeaveType">
                        <Border Style="{StaticResource Card}">
                            <VerticalStackLayout Spacing="4">
                                <Label Text="{Binding LeaveTypeName}" FontAttributes="Bold" FontSize="16" />
                                <Label Text="{Binding Summary}" Style="{StaticResource MutedText}" />
                            </VerticalStackLayout>
                        </Border>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </RefreshView>
    </Grid>
</ContentPage>
```

`x:DataType` turns on **compiled bindings**: typos in binding names become build errors instead of silent blanks.

### Step 3: Code-behind, `LmsMobile/Views/Staff/PolicyPage.xaml.cs`

```csharp
using LmsMobile.ViewModels.Staff;

namespace LmsMobile.Views.Staff;

public partial class PolicyPage : ContentPage
{
    private readonly PolicyViewModel _vm;

    public PolicyPage(PolicyViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
```

### Step 4: Register with DI, `MauiProgram.cs`

```csharp
builder.Services.AddTransient<PolicyPage>();
builder.Services.AddTransient<PolicyViewModel>();
```

### Step 5: Add the tab, `AppShell.xaml.cs` → `BuildEmployeeTabs()`

```csharp
Tab<MyRequestsPage>("My Requests", "myrequests"),
Tab<PolicyPage>("Policy", "policy"),        // ← new
Tab<ProfilePage>("Profile", "profile")
```

### Step 6: Run and check
Start the API, run the app, sign in as an employee: a fifth tab **Policy** shows Annual 15, Sick 10, Casual 5 (plus any types a manager added).

**For a pushed (non-tab) page** instead of Step 5: add `Routing.RegisterRoute("mypage", typeof(MyPage));` in the `AppShell` constructor, navigate with `Shell.Current.GoToAsync("mypage?id=3")`, and put `[QueryProperty(nameof(Id), "id")]` on the ViewModel (copy `EmployeeDetailViewModel`).

**For a page that needs a new API call:** add the call in `LoadCoreAsync` or a command, and add a matching class to `Models/ApiModels.cs` if the JSON shape is new.

### The checklist for any new screen
1. Model class (if new JSON) in `ApiModels.cs`
2. ViewModel inheriting `BaseViewModel`
3. XAML View with `x:DataType`
4. Code-behind with ViewModel injection + `OnAppearing` → `LoadAsync()`
5. Register Page + ViewModel in `MauiProgram.cs`
6. Add a tab or register a route in `AppShell.xaml.cs`
7. If it needs a new server feature, add the endpoint in the API first (controller → service → DTO)

## 19. Common changes (cookbook)

| I want to… | Do this |
|---|---|
| **Change the API address** | `Services/ApiConfig.cs` → `BaseUrl` |
| **Change the request timeout** | `MauiProgram.cs` → `c.Timeout = TimeSpan.FromSeconds(20)` |
| **Add a field to a form** (e.g. phone number on the employee) | 1) backend: Model + DTO + migration + service, 2) `ApiModels.cs`, 3) ViewModel property, 4) XAML `Entry` bound to it |
| **Change colours / theme** | `Resources/Styles/Colors.xaml` (palette) and `AppStyles.xaml` (card, tag styles). Light/dark values exist for page and card. |
| **Change app name** | `LmsMobile.csproj` → `<ApplicationTitle>` |
| **Change package id** (required before store release) | `LmsMobile.csproj` → `<ApplicationId>` (currently `com.companyname.lmsmobile`) |
| **Change app icon / splash** | Replace `Resources/AppIcon/appicon.svg`, `appiconfg.svg`, `Resources/Splash/splash.svg` |
| **Change version** | `<ApplicationDisplayVersion>` and `<ApplicationVersion>` in the csproj |
| **Rename a status colour** | `Converters/StatusToColorConverter.cs` |
| **Show a new error message from the API** | Nothing to do, `ApiClient` shows the server's `message`/`errors` automatically |
| **Add a business rule** | Add it in the API service (`Services/`) and throw `InvalidOperationException` (→ 400) or `ConflictException` (→ 409); the app displays the message. Optionally mirror it in the ViewModel for instant feedback. |
| **Add an API endpoint** | Controller (role check) → Service (logic) → DTO (validation attributes) → test in Swagger → call from the app |
| **Change the database** | Edit the Model, then in `LeaveManagementSystem/`: `dotnet ef migrations add <Name>` (needs `dotnet tool install --global dotnet-ef`); the API applies it on next start |
| **Add a backend test** | `LeaveManagementSystem.Tests/`, follow `LeaveRequestServiceTests.cs` (in-memory DB) |
| **See network/debug logs** | Visual Studio Output window (Debug logging is on in Debug builds, `MauiProgram.cs`). Use Swagger to replay requests. |
| **Use HTTPS in production** | Remove `usesCleartextTraffic` from `Platforms/Android/AndroidManifest.xml`, host the API behind HTTPS, set `ApiConfig.BaseUrl` to it |
| **Move the JWT key out of source** | `dotnet user-secrets set "Jwt:Key" "<long random string>"` in `LeaveManagementSystem/` (a `UserSecretsId` is already set), then remove it from `appsettings.json` |

**Hot Reload:** in Visual Studio, XAML edits while debugging usually appear instantly without restarting.

## 20. Building for release

*(These commands are standard MAUI; only the Windows debug build was verified for this project.)*

```powershell
# Android (needs signing configured for the store; produces .aab by default)
dotnet publish LmsMobile -f net10.0-android -c Release
# Android APK for sideloading
dotnet publish LmsMobile -f net10.0-android -c Release -p:AndroidPackageFormat=apk

# Windows
dotnet publish LmsMobile -f net10.0-windows10.0.19041.0 -c Release
```

Before shipping: change `ApplicationId`, point `ApiConfig.BaseUrl` at an HTTPS server, remove cleartext HTTP, and move the JWT key out of source.

## 21. Command cheat sheet

```powershell
# --- Backend ---
cd LeaveManagementSystem
dotnet run --launch-profile http          # start API on http://localhost:5245  (+ /swagger)
dotnet build                              # compile only
cd ..
dotnet test LeaveManagementSystem.Tests   # 8 unit tests

# --- Mobile ---
cd LmsMobile
dotnet run -f net10.0-windows10.0.19041.0   # run as Windows app
dotnet build -f net10.0-android -t:Run               # run on running Android emulator/device
dotnet build -f net10.0-windows10.0.19041.0          # compile check only

# --- Database ---
sqlcmd -S localhost -E -C -Q "ALTER DATABASE LeaveManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE LeaveManagementDB"   # reset (stop the API first; re-created on next start)

# --- Environment checks ---
dotnet --list-sdks
dotnet workload list
Get-Service MSSQLSERVER
```

**Demo logins:** `sai` / `Sai@123` (Manager) · `priya` / `Priya@123` · `poojitha` / `Poojitha@123` · `arjun` / `Arjun@123` (Employees)
