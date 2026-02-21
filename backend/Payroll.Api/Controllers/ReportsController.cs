using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "PayrollAdmin,Finance,Auditor")]
public class ReportsController(PayrollDbContext db) : ControllerBase {
    [HttpGet("payroll-cost")]
    public async Task<IActionResult> PayrollCost([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var data = await Query(from, to).GroupBy(x=>x.PayDate).Select(g=> new { period = g.Key, totalCost = g.Sum(x=>x.EmployerCostTotal)}).OrderBy(x=>x.period).ToListAsync();
        return Ok(data);
    }

    [HttpGet("gross-vs-net")]
    public async Task<IActionResult> GrossVsNet([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var data = await Query(from, to).GroupBy(x=>x.PayDate).Select(g=> new { period = g.Key, gross = g.Sum(x=>x.Gross), net = g.Sum(x=>x.Net)}).OrderBy(x=>x.period).ToListAsync();
        return Ok(data);
    }

    [HttpGet("deductions-breakdown")]
    public async Task<IActionResult> Deductions([FromQuery] Guid periodId) {
        var data = await db.PayrollLineItems.Where(li=>li.Category.ToString()!="Earning" && db.PayrollResults.Any(r=>r.Id==li.ResultId && db.PayrollRuns.Any(pr=>pr.Id==r.RunId && pr.PeriodId==periodId)))
            .GroupBy(li=>li.Code).Select(g=> new { code=g.Key, amount=g.Sum(x=>x.Amount)}).ToListAsync();
        return Ok(data);
    }

    [HttpGet("top-earners")]
    public async Task<IActionResult> TopEarners([FromQuery] Guid periodId, [FromQuery] int top = 10) {
        var data = await (from r in db.PayrollResults join run in db.PayrollRuns on r.RunId equals run.Id join e in db.Employees on r.EmployeeId equals e.Id where run.PeriodId==periodId orderby r.Gross descending select new { employee=e.FullName, gross=r.Gross }).Take(top).ToListAsync();
        return Ok(data);
    }

    [HttpGet("payroll-cost/csv")]
    public async Task<IActionResult> PayrollCostCsv([FromQuery] DateOnly from, [FromQuery] DateOnly to) {
        var data = await Query(from,to).GroupBy(x=>x.PayDate).Select(g=> $"{g.Key},{g.Sum(x=>x.EmployerCostTotal)}").ToListAsync();
        return File(Encoding.UTF8.GetBytes("period,totalCost\n"+string.Join("\n", data)), "text/csv", "payroll-cost.csv");
    }

    private IQueryable<ResultRow> Query(DateOnly from, DateOnly to) => from r in db.PayrollResults
        join run in db.PayrollRuns on r.RunId equals run.Id
        join p in db.PayPeriods on run.PeriodId equals p.Id
        where p.PayDate >= from && p.PayDate <= to
        select new ResultRow(p.PayDate, r.Gross, r.Net, r.EmployerCostTotal);

    private record ResultRow(DateOnly PayDate, decimal Gross, decimal Net, decimal EmployerCostTotal);
}
