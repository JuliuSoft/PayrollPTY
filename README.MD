# PayrollPTY MVP
On-prem single-company payroll system for Panama with deterministic engine + audit trail.

## Structure
- `backend/` .NET 8 Web API + EF Core + SQL Server.
- `frontend/` React + TypeScript + Recharts.
- `docs/` architecture, runbook, payroll rules.

## Quick answer: how to generate database objects
### Option 1 (run inside API folder)
```bash
cd backend/Payroll.Api
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet tool run dotnet-ef -- database update --project ../Payroll.Infrastructure --startup-project .
```

### Option 2 (run from repository root)
```bash
dotnet restore backend/Payroll.Api/Payroll.Api.csproj
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
dotnet tool run dotnet-ef -- database update --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
```

To generate a SQL deployment script instead:
```bash
dotnet tool run dotnet-ef -- migrations script --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj --output ./backend/Payroll.Api/migrations.sql
```

See `docs/runbook.md` for complete setup.

### If you get `No project was found in directory '.'`
You ran the command from a folder that does not contain a `.csproj` while using `--startup-project .`.

Fix:
- either run `cd backend/Payroll.Api` first, then keep `--startup-project .`
- or keep running from root and pass explicit `.csproj` paths (Option 2 above)

### If you get `get_LockReleaseBehavior` when running migrations
You likely have mixed EF versions installed. Use the repo-pinned tool and aligned EF packages:
- `dotnet tool restore`
- `dotnet tool run dotnet-ef -- --version`
- `dotnet nuget locals all --clear`
- `dotnet restore`
