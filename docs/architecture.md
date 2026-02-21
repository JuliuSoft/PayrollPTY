# Architecture
- Backend uses Clean-ish layering: Domain (entities), Application (payroll engine), Infrastructure (EF Core, auth, audit), API (controllers).
- SQL Server via EF Core DbContext and seeded data.
- Frontend React + Vite + Recharts with RBAC-aware API consumption.
- Deterministic payroll engine stores trace line items with rule metadata.
