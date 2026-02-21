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
