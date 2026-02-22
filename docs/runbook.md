# Runbook (Windows On-Prem)

## 1) Prerequisites
- SQL Server 2019/2022 (local or on-prem instance)
- .NET 8 SDK
- Node 20+
- EF Core CLI (pinned in repo):
  - `dotnet tool restore`
  - Verify: `dotnet tool run dotnet-ef -- --version`

## 2) Generate database objects (tables/indexes/constraints)

### Option A (recommended): run from API startup project folder
```bash
cd backend/Payroll.Api
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet tool run dotnet-ef -- database update --project ../Payroll.Infrastructure --startup-project .
```

### Option B: run from repository root using explicit project paths
```bash
dotnet restore backend/Payroll.Api/Payroll.Api.csproj
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
dotnet tool run dotnet-ef -- database update --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
```

### Generate SQL script for DBA-controlled deployment
```bash
dotnet tool run dotnet-ef -- migrations script --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj --output ./backend/Payroll.Api/migrations.sql
```
Then execute `migrations.sql` in SQL Server Management Studio against the target database.

## 3) Run backend
```bash
cd backend/Payroll.Api
dotnet run
```
- API applies pending migrations on startup and seeds dev data (`admin`, employees, schedules, periods, sample runs).

## 4) Run frontend (Windows 11 step-by-step)

Open a **new PowerShell terminal** (keep backend running in another terminal):

```powershell
cd C:\Users\<your-user>\source\repos\PayrollPTY\frontend
node -v
npm -v
```

Install dependencies:

```powershell
npm install
```

Run Vite dev server:

```powershell
npm run dev
```

Open the URL shown by Vite (usually `http://localhost:5173`).

If API runs on `http://localhost:5000` (default in this repo), keep it running before opening frontend pages.

### Frontend troubleshooting on Windows 11
- If scripts are blocked in PowerShell:
  - `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`
- If `npm install` fails because of proxy:
  - `npm config rm proxy`
  - `npm config rm https-proxy`
- If port `5173` is busy:
  - `npm run dev -- --port 5174`
- If browser cannot reach API:
  - verify backend terminal shows API started and listening on expected URL.

## 5) Login
- Username: `admin`
- Password: `Admin123!`

## Troubleshooting EF design-time DbContext errors
### Error: `No project was found in directory '.'`
Cause:
- You ran `--startup-project .` from a folder without a `.csproj` file.

Fix:
- `cd backend/Payroll.Api` and rerun, or
- pass explicit startup project path:
  - `--startup-project backend/Payroll.Api/Payroll.Api.csproj`

### Error: `Method 'get_LockReleaseBehavior' ... does not have an implementation`
it usually means **EF Core package version mismatch** between runtime/provider/tools. Use these rules:
- Keep all EF packages on the same version (in this repo: `8.0.8`).
- Use the repo-pinned CLI tool (`dotnet tool restore` + `dotnet tool run dotnet-ef -- ...`) instead of a different global `dotnet-ef`.
- Clear stale caches and restore again:
  - `dotnet nuget locals all --clear`
  - `dotnet restore`

This repository includes:
- `Microsoft.EntityFrameworkCore.Design` 8.0.8 in `Payroll.Infrastructure`
- a design-time factory (`DesignTimePayrollDbContextFactory`)
- local tool manifest pinning `dotnet-ef` to 8.0.8


### If you get: `Your startup project 'Payroll.Api' doesn't reference Microsoft.EntityFrameworkCore.Design`
Fix:
- Ensure `backend/Payroll.Api/Payroll.Api.csproj` includes:
  - `Microsoft.EntityFrameworkCore.Design` (same version as other EF packages, here `8.0.8`)
- Restore packages:
  - `dotnet restore backend/Payroll.Api/Payroll.Api.csproj`
- Re-run EF command with explicit startup project path if needed.


### Error: `Unable to retrieve project metadata. Ensure it's an SDK-style project`
Typical causes:
- `--project` or `--startup-project` points to a folder/file that is not the intended SDK-style `.csproj`.
- Running from a different working directory with relative paths that no longer resolve.
- Custom `BaseIntermediateOutputPath` / `MSBuildProjectExtensionsPath` in your local setup.

Fix (prefer explicit project files):
```bash
dotnet tool run dotnet-ef -- migrations add InitialCreate \
  --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj \
  --startup-project backend/Payroll.Api/Payroll.Api.csproj
```

If your environment customizes `obj` path, pass it explicitly:
```bash
dotnet tool run dotnet-ef -- migrations add InitialCreate \
  --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj \
  --startup-project backend/Payroll.Api/Payroll.Api.csproj \
  --msbuildprojectextensionspath backend/Payroll.Api/obj
```

Also verify the startup project is SDK-style (`<Project Sdk=...>`) and restore first:
- `dotnet restore backend/Payroll.Api/Payroll.Api.csproj`
- `dotnet restore backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj`
