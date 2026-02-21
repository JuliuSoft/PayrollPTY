# Migrations / Database Object Generation

From `backend/Payroll.Api`:

```bash
dotnet restore
dotnet ef migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
```

Apply database objects directly:

```bash
dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
```

Or generate deployable SQL (for DBA-controlled environments):

```bash
dotnet ef migrations script --project ../Payroll.Infrastructure --startup-project . --output ./migrations.sql
```

The generated script contains CREATE/ALTER statements for tables, indexes, constraints, and EF migration history.
