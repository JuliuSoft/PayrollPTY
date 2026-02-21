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

## 4) Run frontend
```bash
cd frontend
npm install
npm run dev
```

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
