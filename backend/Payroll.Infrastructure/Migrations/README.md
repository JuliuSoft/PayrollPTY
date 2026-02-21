# Migrations / Database Object Generation

From `backend/Payroll.Api`:

```bash
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
```

Apply database objects directly:

```bash
dotnet tool run dotnet-ef -- database update --project ../Payroll.Infrastructure --startup-project .
```

Or generate deployable SQL (for DBA-controlled environments):

```bash
dotnet tool run dotnet-ef -- migrations script --project ../Payroll.Infrastructure --startup-project . --output ./migrations.sql
```

The generated script contains CREATE/ALTER statements for tables, indexes, constraints, and EF migration history.


## Fix for `get_LockReleaseBehavior` design-time error
This error indicates a mismatch between EF runtime/provider/tools versions.

Checklist:
1. Ensure EF packages are aligned (all `8.0.8` in this repo).
2. Use local pinned tool (`dotnet tool restore`, then `dotnet tool run dotnet-ef -- ...`).
3. Clear NuGet caches and restore:
   - `dotnet nuget locals all --clear`
   - `dotnet restore`
