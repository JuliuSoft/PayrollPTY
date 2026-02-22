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


## Frontend on Windows 11 (quick steps)
```powershell
cd C:\Users\<your-user>\source\repos\PayrollPTY\frontend
node -v
npm -v
npm install
npm run dev
```
Then open the Vite URL (usually `http://localhost:5173`). Keep backend running on `http://localhost:5000`.


## SQL sample scripts (Jan/Feb 2026)
- Seed sample payroll data: `docs/sql/sample-data-2026-jan-feb.sql`
- Verify loaded data: `docs/sql/verify-sample-data-2026-jan-feb.sql`
