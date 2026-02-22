using System.Text.Json;
using Payroll.Application;
using Payroll.Domain;

namespace Payroll.Infrastructure;

public static class SeedData {
    public static async Task EnsureSeedAsync(PayrollDbContext db) {
        if (db.Employees.Any()) return;
        db.Users.Add(new AppUser{ Username="admin", PasswordHash=BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role="PayrollAdmin"});
        var scheduleQ = new PaySchedule { Name = "Quincenal", Type = PayScheduleType.Quincenal };
        var scheduleM = new PaySchedule { Name = "Mensual", Type = PayScheduleType.Mensual };
        db.PaySchedules.AddRange(scheduleQ,scheduleM);
        var periods = Enumerable.Range(0,6).Select(i => new PayPeriod{
            ScheduleId = i%2==0?scheduleQ.Id:scheduleM.Id,
            StartDate = new DateOnly(2025,1,1).AddDays(i*15), EndDate = new DateOnly(2025,1,15).AddDays(i*15), PayDate = new DateOnly(2025,1,20).AddDays(i*15)
        }).ToList();
        db.PayPeriods.AddRange(periods);
        var employees = Enumerable.Range(1,10).Select(i=> new Employee{ FullName=$"Employee {i}", NationalId=$"8-000-{i:0000}", HireDate=new DateOnly(2024,1,1), Department=i%2==0?"Finance":"HR", SalaryType=SalaryType.Monthly, BaseSalary=1200+i*50, BankAccount=$"PA{i:000000}", TaxConfig="default"}).ToList();
        db.Employees.AddRange(employees);
        var brackets = new List<IsrBracket>{ new(0,1000,0,0), new(1000,5000,0.15m,0), new(5000,null,0.25m,600)};
        db.StatutoryConfigs.Add(new StatutoryConfig{ EffectiveDate = new DateOnly(2025,1,1), CssEmployeeRate = 0.0975m, CssEmployerRate = 0.1275m, IsrBracketsJson = JsonSerializer.Serialize(brackets), RoundingRules="AwayFromZero"});
        await db.SaveChangesAsync();

        var run1 = new PayrollRun{ PeriodId=periods[0].Id, CreatedBy="admin", Status=PayrollRunStatus.Locked, LockedAt=DateTime.UtcNow};
        var run2 = new PayrollRun{ PeriodId=periods[1].Id, CreatedBy="admin", Status=PayrollRunStatus.Preview};
        db.PayrollRuns.AddRange(run1,run2);
        var engine = new PayrollEngine();
        var cfg = db.StatutoryConfigs.First();
        foreach (var e in employees.Take(5)) {
            var calc = engine.Compute(new PayrollInput{ Employee=e, Config=cfg });
            var result = new PayrollResult{ RunId=run1.Id, EmployeeId=e.Id, Gross=calc.Gross, Deductions=calc.Deductions, Net=calc.Net, EmployerCostTotal=calc.EmployerCost};
            db.PayrollResults.Add(result);
            db.PayrollLineItems.AddRange(calc.LineItems.Select(li=> new PayrollLineItem{ ResultId=result.Id, Category=li.Category, Code=li.Code, Description=li.Description, Amount=li.Amount, Base=li.Base, Rate=li.Rate, RuleId=li.RuleId, EffectiveDate=li.EffectiveDate, RoundingMode=li.RoundingMode }));
        }
        await db.SaveChangesAsync();
    }
}
