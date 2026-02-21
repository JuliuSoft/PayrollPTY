# PayrollPTY MVP
On-prem single-company payroll system for Panama with deterministic engine + audit trail.

## Structure
- `backend/` .NET 8 Web API + EF Core + SQL Server.
- `frontend/` React + TypeScript + Recharts.
- `docs/` architecture, runbook, payroll rules.

## Quick answer: how to generate database objects
```bash
cd backend/Payroll.Api
dotnet restore
dotnet ef migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
```

To generate a SQL deployment script instead:
```bash
dotnet ef migrations script --project ../Payroll.Infrastructure --startup-project . --output ./migrations.sql
```

See `docs/runbook.md` for complete setup.
