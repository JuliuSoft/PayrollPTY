using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "PayrollAdmin,Finance,Auditor")]
public class ReportsController(PayrollDbContext db) : ControllerBase {
    [HttpGet("payroll-cost")]
    public async Task<IActionResult> PayrollCost([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var data = await (
            from r in db.PayrollResults
            join run in db.PayrollRuns on r.RunId equals run.Id
            join p in db.PayPeriods on run.PeriodId equals p.Id
            where p.PayDate >= from && p.PayDate <= to
            group r by p.PayDate into g
            orderby g.Key
            select new { period = g.Key, totalCost = g.Sum(x => x.EmployerCostTotal) }
        ).ToListAsync();

        return Ok(data);
    }

    [HttpGet("gross-vs-net")]
    public async Task<IActionResult> GrossVsNet([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var data = await (
            from r in db.PayrollResults
            join run in db.PayrollRuns on r.RunId equals run.Id
            join p in db.PayPeriods on run.PeriodId equals p.Id
            where p.PayDate >= from && p.PayDate <= to
            group r by p.PayDate into g
            orderby g.Key
            select new { period = g.Key, gross = g.Sum(x => x.Gross), net = g.Sum(x => x.Net) }
        ).ToListAsync();

        return Ok(data);
    }

    [HttpGet("deductions-breakdown")]
    public async Task<IActionResult> Deductions([FromQuery] Guid periodId) {
        var data = await (
            from li in db.PayrollLineItems
            join r in db.PayrollResults on li.ResultId equals r.Id
            join run in db.PayrollRuns on r.RunId equals run.Id
            where run.PeriodId == periodId && li.Category != PayrollLineCategory.Earning
            group li by li.Code into g
            orderby g.Key
            select new { code = g.Key, amount = g.Sum(x => x.Amount) }
        ).ToListAsync();

        return Ok(data);
    }

    [HttpGet("top-earners")]
    public async Task<IActionResult> TopEarners([FromQuery] Guid periodId, [FromQuery] int top = 10) {
        var data = await (
            from r in db.PayrollResults
            join run in db.PayrollRuns on r.RunId equals run.Id
            join e in db.Employees on r.EmployeeId equals e.Id
            where run.PeriodId == periodId
            orderby r.Gross descending
            select new { employee = e.FullName, gross = r.Gross }
        ).Take(top).ToListAsync();

        return Ok(data);
    }

    [HttpGet("payroll-cost/csv")]
    public async Task<IActionResult> PayrollCostCsv([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var rows = await (
            from r in db.PayrollResults
            join run in db.PayrollRuns on r.RunId equals run.Id
            join p in db.PayPeriods on run.PeriodId equals p.Id
            where p.PayDate >= from && p.PayDate <= to
            group r by p.PayDate into g
            orderby g.Key
            select new { period = g.Key, totalCost = g.Sum(x => x.EmployerCostTotal) }
        ).ToListAsync();

        var lines = rows.Select(x => $"{x.period},{x.totalCost}");
        return File(Encoding.UTF8.GetBytes("period,totalCost\n" + string.Join("\n", lines)), "text/csv", "payroll-cost.csv");
    }
}
