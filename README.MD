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
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet tool run dotnet-ef -- database update --project ../Payroll.Infrastructure --startup-project .
```

To generate a SQL deployment script instead:
```bash
dotnet tool run dotnet-ef -- migrations script --project ../Payroll.Infrastructure --startup-project . --output ./migrations.sql
```

See `docs/runbook.md` for complete setup.


### If you get `get_LockReleaseBehavior` when running migrations
You likely have mixed EF versions installed. Use the repo-pinned tool and aligned EF packages:
- `dotnet tool restore`
- `dotnet tool run dotnet-ef -- --version`
- `dotnet nuget locals all --clear`
- `dotnet restore`
