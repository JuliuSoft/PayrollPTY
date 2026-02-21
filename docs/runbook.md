# Runbook (Windows On-Prem)
1. Install SQL Server, .NET 8 SDK, Node 20.
2. Backend:
   - `cd backend/Payroll.Api`
   - `dotnet restore`
   - `dotnet ef database update --project ../Payroll.Infrastructure`
   - `dotnet run`
3. Frontend:
   - `cd frontend`
   - `npm install`
   - `npm run dev`
4. Login with `admin / Admin123!`.
