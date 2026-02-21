# Runbook (Windows On-Prem)

## 1) Prerequisites
- SQL Server 2019/2022 (local or on-prem instance)
- .NET 8 SDK
- Node 20+
- EF Core CLI:
  - `dotnet tool install --global dotnet-ef`

## 2) Generate database objects (tables/indexes/constraints)
From API startup project folder:

```bash
cd backend/Payroll.Api
dotnet restore
```

### Option A (recommended for dev): apply migrations directly
```bash
dotnet ef migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
```

### Option B (recommended for controlled on-prem releases): generate SQL script
```bash
dotnet ef migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet ef migrations script --project ../Payroll.Infrastructure --startup-project . --output ./migrations.sql
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
