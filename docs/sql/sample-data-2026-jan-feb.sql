/*
  PayrollPTY sample data for Jan/Feb 2026
  Target DB: PayrollPTY (SQL Server)
  Notes:
  - Uses deterministic GUIDs for reproducibility.
  - Inserts only when records do not already exist.
  - Creates pay periods + payroll runs + results + line items for Jan/Feb 2026.
*/

SET NOCOUNT ON;

-- -----------------------------------------------------------------------------
-- 1) Ensure schedules exist
-- -----------------------------------------------------------------------------
DECLARE @ScheduleMensual UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000001';
DECLARE @ScheduleQuincenal UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM PaySchedules WHERE Id = @ScheduleMensual)
BEGIN
    INSERT INTO PaySchedules (Id, [Type], [Name])
    VALUES (@ScheduleMensual, 1, 'Mensual');
END

IF NOT EXISTS (SELECT 1 FROM PaySchedules WHERE Id = @ScheduleQuincenal)
BEGIN
    INSERT INTO PaySchedules (Id, [Type], [Name])
    VALUES (@ScheduleQuincenal, 0, 'Quincenal');
END

-- -----------------------------------------------------------------------------
-- 2) Ensure statutory config for 2026-01-01 exists
-- -----------------------------------------------------------------------------
DECLARE @Cfg2026 UNIQUEIDENTIFIER = '20000000-0000-0000-0000-000000000001';
IF NOT EXISTS (SELECT 1 FROM StatutoryConfigs WHERE Id = @Cfg2026)
BEGIN
    INSERT INTO StatutoryConfigs (Id, EffectiveDate, CssEmployeeRate, CssEmployerRate, IsrBracketsJson, RoundingRules)
    VALUES (
        @Cfg2026,
        '2026-01-01',
        0.0975,
        0.1275,
        '[{"From":0,"To":1000,"Rate":0.0,"FixedQuota":0.0},{"From":1000,"To":5000,"Rate":0.15,"FixedQuota":0.0},{"From":5000,"To":null,"Rate":0.25,"FixedQuota":600.0}]',
        'AwayFromZero'
    );
END

-- -----------------------------------------------------------------------------
-- 3) Ensure 10 employees for sample runs
-- -----------------------------------------------------------------------------
;WITH E AS (
    SELECT CAST('30000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER) Id, 'Ana Gómez' FullName, '8-900-0001' NationalId, 'HR' Department, 1400.00 BaseSalary UNION ALL
    SELECT '30000000-0000-0000-0000-000000000002', 'Luis Pérez', '8-900-0002', 'Finance', 1550.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000003', 'María Chen', '8-900-0003', 'Operations', 1650.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000004', 'Carlos Díaz', '8-900-0004', 'Finance', 1750.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000005', 'Sofía Ríos', '8-900-0005', 'HR', 1850.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000006', 'Pedro Núñez', '8-900-0006', 'Operations', 1950.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000007', 'Elena Cruz', '8-900-0007', 'IT', 2050.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000008', 'José Moreno', '8-900-0008', 'IT', 2150.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000009', 'Valeria Ruiz', '8-900-0009', 'Legal', 2250.00 UNION ALL
    SELECT '30000000-0000-0000-0000-000000000010', 'Tomás Vega', '8-900-0010', 'Legal', 2350.00
)
INSERT INTO Employees (Id, FullName, NationalId, HireDate, Department, [Status], BankAccount, SalaryType, BaseSalary, HourlyRate, TaxConfig, CreatedAt)
SELECT
    e.Id, e.FullName, e.NationalId, '2025-01-15', e.Department, 0,
    CONCAT('PA-SAMPLE-', RIGHT(e.NationalId, 4)),
    0,
    e.BaseSalary,
    0,
    'default',
    GETUTCDATE()
FROM E e
WHERE NOT EXISTS (SELECT 1 FROM Employees x WHERE x.Id = e.Id OR x.NationalId = e.NationalId);

-- -----------------------------------------------------------------------------
-- 4) Jan/Feb 2026 monthly pay periods
-- -----------------------------------------------------------------------------
DECLARE @PeriodJan UNIQUEIDENTIFIER = '40000000-0000-0000-0000-000000000001';
DECLARE @PeriodFeb UNIQUEIDENTIFIER = '40000000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM PayPeriods WHERE Id = @PeriodJan)
BEGIN
    INSERT INTO PayPeriods (Id, ScheduleId, StartDate, EndDate, PayDate, [Status])
    VALUES (@PeriodJan, @ScheduleMensual, '2026-01-01', '2026-01-31', '2026-01-31', 'Closed');
END

IF NOT EXISTS (SELECT 1 FROM PayPeriods WHERE Id = @PeriodFeb)
BEGIN
    INSERT INTO PayPeriods (Id, ScheduleId, StartDate, EndDate, PayDate, [Status])
    VALUES (@PeriodFeb, @ScheduleMensual, '2026-02-01', '2026-02-28', '2026-02-28', 'Closed');
END

-- -----------------------------------------------------------------------------
-- 5) Payroll runs (locked) for Jan/Feb 2026
-- -----------------------------------------------------------------------------
DECLARE @RunJan UNIQUEIDENTIFIER = '50000000-0000-0000-0000-000000000001';
DECLARE @RunFeb UNIQUEIDENTIFIER = '50000000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM PayrollRuns WHERE Id = @RunJan)
BEGIN
    INSERT INTO PayrollRuns (Id, PeriodId, RunType, [Status], CreatedBy, ApprovedBy, LockedAt, CreatedAt)
    VALUES (@RunJan, @PeriodJan, 0, 4, 'admin', 'admin', GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM PayrollRuns WHERE Id = @RunFeb)
BEGIN
    INSERT INTO PayrollRuns (Id, PeriodId, RunType, [Status], CreatedBy, ApprovedBy, LockedAt, CreatedAt)
    VALUES (@RunFeb, @PeriodFeb, 0, 4, 'admin', 'admin', GETUTCDATE(), GETUTCDATE());
END

-- -----------------------------------------------------------------------------
-- 6) Payroll results + line items (deterministic formula)
--    gross = BaseSalary
--    cssEmp = round(gross * 9.75%)
--    isr = round(case taxable<=1000 then 0 else (taxable-1000)*0.15 end)
--    net = gross - cssEmp - isr
--    cssEmpl = round(gross * 12.75%)
--    employerCost = gross + cssEmpl
-- -----------------------------------------------------------------------------
DECLARE @EffectiveDate DATE = '2026-01-01';

;WITH BaseData AS (
    SELECT e.Id AS EmployeeId, e.BaseSalary AS Gross
    FROM Employees e
    WHERE e.Id BETWEEN '30000000-0000-0000-0000-000000000001' AND '30000000-0000-0000-0000-000000000010'
), Calc AS (
    SELECT
        EmployeeId,
        CAST(ROUND(Gross, 2) AS DECIMAL(18,2)) AS Gross,
        CAST(ROUND(Gross * 0.0975, 2) AS DECIMAL(18,2)) AS CssEmp,
        CAST(ROUND(CASE WHEN (Gross - (Gross * 0.0975)) <= 1000 THEN 0 ELSE ((Gross - (Gross * 0.0975)) - 1000) * 0.15 END, 2) AS DECIMAL(18,2)) AS Isr,
        CAST(ROUND(Gross * 0.1275, 2) AS DECIMAL(18,2)) AS CssEmpl
    FROM BaseData
), FinalCalc AS (
    SELECT
        EmployeeId,
        Gross,
        CAST(CssEmp + Isr AS DECIMAL(18,2)) AS Deductions,
        CAST(Gross - CssEmp - Isr AS DECIMAL(18,2)) AS Net,
        CAST(Gross + CssEmpl AS DECIMAL(18,2)) AS EmployerCostTotal,
        CssEmp, Isr, CssEmpl
    FROM Calc
)
INSERT INTO PayrollResults (Id, RunId, EmployeeId, Gross, Deductions, Net, EmployerCostTotal)
SELECT
    NEWID(),
    @RunJan,
    c.EmployeeId,
    c.Gross,
    c.Deductions,
    c.Net,
    c.EmployerCostTotal
FROM FinalCalc c
WHERE NOT EXISTS (
    SELECT 1 FROM PayrollResults r WHERE r.RunId = @RunJan AND r.EmployeeId = c.EmployeeId
);

;WITH BaseData AS (
    SELECT e.Id AS EmployeeId, e.BaseSalary AS Gross
    FROM Employees e
    WHERE e.Id BETWEEN '30000000-0000-0000-0000-000000000001' AND '30000000-0000-0000-0000-000000000010'
), Calc AS (
    SELECT
        EmployeeId,
        CAST(ROUND(Gross + 50, 2) AS DECIMAL(18,2)) AS Gross,
        CAST(ROUND((Gross + 50) * 0.0975, 2) AS DECIMAL(18,2)) AS CssEmp,
        CAST(ROUND(CASE WHEN ((Gross + 50) - ((Gross + 50) * 0.0975)) <= 1000 THEN 0 ELSE (((Gross + 50) - ((Gross + 50) * 0.0975)) - 1000) * 0.15 END, 2) AS DECIMAL(18,2)) AS Isr,
        CAST(ROUND((Gross + 50) * 0.1275, 2) AS DECIMAL(18,2)) AS CssEmpl
    FROM BaseData
), FinalCalc AS (
    SELECT
        EmployeeId,
        Gross,
        CAST(CssEmp + Isr AS DECIMAL(18,2)) AS Deductions,
        CAST(Gross - CssEmp - Isr AS DECIMAL(18,2)) AS Net,
        CAST(Gross + CssEmpl AS DECIMAL(18,2)) AS EmployerCostTotal,
        CssEmp, Isr, CssEmpl
    FROM Calc
)
INSERT INTO PayrollResults (Id, RunId, EmployeeId, Gross, Deductions, Net, EmployerCostTotal)
SELECT
    NEWID(),
    @RunFeb,
    c.EmployeeId,
    c.Gross,
    c.Deductions,
    c.Net,
    c.EmployerCostTotal
FROM FinalCalc c
WHERE NOT EXISTS (
    SELECT 1 FROM PayrollResults r WHERE r.RunId = @RunFeb AND r.EmployeeId = c.EmployeeId
);

-- Line items for Jan
INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 0, 'GROSS', 'Gross earnings', r.Gross, r.Gross, 1.00, 'RULE_GROSS', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunJan
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'GROSS');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 1, 'CSS_EMP', 'CSS employee', ROUND(r.Gross * 0.0975,2), r.Gross, 0.0975, 'RULE_CSS_EMP', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunJan
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'CSS_EMP');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 2, 'ISR', 'ISR withholding', ROUND(CASE WHEN (r.Gross - (r.Gross * 0.0975)) <= 1000 THEN 0 ELSE ((r.Gross - (r.Gross * 0.0975)) - 1000) * 0.15 END,2), (r.Gross - (r.Gross * 0.0975)), 0.15, 'RULE_ISR', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunJan
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'ISR');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 3, 'CSS_EMPL', 'CSS employer', ROUND(r.Gross * 0.1275,2), r.Gross, 0.1275, 'RULE_CSS_EMPL', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunJan
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'CSS_EMPL');

-- Line items for Feb
INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 0, 'GROSS', 'Gross earnings', r.Gross, r.Gross, 1.00, 'RULE_GROSS', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunFeb
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'GROSS');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 1, 'CSS_EMP', 'CSS employee', ROUND(r.Gross * 0.0975,2), r.Gross, 0.0975, 'RULE_CSS_EMP', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunFeb
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'CSS_EMP');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 2, 'ISR', 'ISR withholding', ROUND(CASE WHEN (r.Gross - (r.Gross * 0.0975)) <= 1000 THEN 0 ELSE ((r.Gross - (r.Gross * 0.0975)) - 1000) * 0.15 END,2), (r.Gross - (r.Gross * 0.0975)), 0.15, 'RULE_ISR', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunFeb
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'ISR');

INSERT INTO PayrollLineItems (Id, ResultId, Category, Code, Description, Amount, [Base], Rate, RuleId, EffectiveDate, RoundingMode)
SELECT NEWID(), r.Id, 3, 'CSS_EMPL', 'CSS employer', ROUND(r.Gross * 0.1275,2), r.Gross, 0.1275, 'RULE_CSS_EMPL', @EffectiveDate, 'AwayFromZero'
FROM PayrollResults r
WHERE r.RunId = @RunFeb
  AND NOT EXISTS (SELECT 1 FROM PayrollLineItems x WHERE x.ResultId = r.Id AND x.Code = 'CSS_EMPL');

-- -----------------------------------------------------------------------------
-- 7) Audit markers
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM AuditEvents WHERE Entity = 'PayrollRun' AND EntityId = CAST(@RunJan AS NVARCHAR(64)) AND Action = 'SeedSampleData')
BEGIN
    INSERT INTO AuditEvents (Id, UserId, Action, Entity, EntityId, BeforeJson, AfterJson, CreatedAt)
    VALUES (NEWID(), 'admin', 'SeedSampleData', 'PayrollRun', CAST(@RunJan AS NVARCHAR(64)), NULL, '{"month":"2026-01"}', GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM AuditEvents WHERE Entity = 'PayrollRun' AND EntityId = CAST(@RunFeb AS NVARCHAR(64)) AND Action = 'SeedSampleData')
BEGIN
    INSERT INTO AuditEvents (Id, UserId, Action, Entity, EntityId, BeforeJson, AfterJson, CreatedAt)
    VALUES (NEWID(), 'admin', 'SeedSampleData', 'PayrollRun', CAST(@RunFeb AS NVARCHAR(64)), NULL, '{"month":"2026-02"}', GETUTCDATE());
END

PRINT 'Sample Jan/Feb 2026 payroll data script completed.';
