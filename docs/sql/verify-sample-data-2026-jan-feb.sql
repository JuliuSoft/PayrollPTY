/* Verification queries for Jan/Feb 2026 sample dataset */

SELECT p.PayDate, pr.Id AS RunId, pr.Status, COUNT(r.Id) AS Employees,
       SUM(r.Gross) AS TotalGross, SUM(r.Deductions) AS TotalDeductions, SUM(r.Net) AS TotalNet,
       SUM(r.EmployerCostTotal) AS TotalEmployerCost
FROM PayrollRuns pr
JOIN PayPeriods p ON p.Id = pr.PeriodId
LEFT JOIN PayrollResults r ON r.RunId = pr.Id
WHERE p.PayDate IN ('2026-01-31', '2026-02-28')
GROUP BY p.PayDate, pr.Id, pr.Status
ORDER BY p.PayDate;

SELECT p.PayDate, li.Code, SUM(li.Amount) AS TotalAmount
FROM PayrollRuns pr
JOIN PayPeriods p ON p.Id = pr.PeriodId
JOIN PayrollResults r ON r.RunId = pr.Id
JOIN PayrollLineItems li ON li.ResultId = r.Id
WHERE p.PayDate IN ('2026-01-31', '2026-02-28')
GROUP BY p.PayDate, li.Code
ORDER BY p.PayDate, li.Code;
