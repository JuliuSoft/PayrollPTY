Run EF Core migrations from backend/Payroll.Api:

```bash
dotnet ef migrations add InitialCreate --project ../Payroll.Infrastructure --startup-project .
dotnet ef database update
```
