# Migrations / Database Object Generation

## Option A: run from `backend/Payroll.Api`
```bash
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
```

Apply database objects directly:

```bash
dotnet tool run dotnet-ef -- database update --project ../Payroll.Infrastructure --startup-project .
```

## Option B: run from repository root
```bash
dotnet restore backend/Payroll.Api/Payroll.Api.csproj
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
dotnet tool run dotnet-ef -- database update --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj
```

Or generate deployable SQL (for DBA-controlled environments):

```bash
dotnet tool run dotnet-ef -- migrations script --project backend/Payroll.Infrastructure/Payroll.Infrastructure.csproj --startup-project backend/Payroll.Api/Payroll.Api.csproj --output ./backend/Payroll.Api/migrations.sql
```

The generated script contains CREATE/ALTER statements for tables, indexes, constraints, and EF migration history.

## Fix for `No project was found in directory '.'`
`--startup-project .` only works when current directory contains a project file.
- If you are at repo root, do not use `.`.
- Use `--startup-project backend/Payroll.Api/Payroll.Api.csproj` instead.

## Fix for `get_LockReleaseBehavior` design-time error
This error indicates a mismatch between EF runtime/provider/tools versions.

Checklist:
1. Ensure EF packages are aligned (all `8.0.8` in this repo).
2. Use local pinned tool (`dotnet tool restore`, then `dotnet tool run dotnet-ef -- ...`).
3. Clear NuGet caches and restore:
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
