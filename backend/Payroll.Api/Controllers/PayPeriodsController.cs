using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("pay-periods")]
[Authorize]
public class PayPeriodsController(PayrollDbContext db) : ControllerBase {
    [HttpGet] public async Task<IActionResult> List() => Ok(await db.PayPeriods.ToListAsync());
    [HttpPost("generate")] [Authorize(Roles="PayrollAdmin,HR")]
    public async Task<IActionResult> Generate([FromBody] GeneratePeriodsRequest req) {
        var periods = new List<PayPeriod>();
        for(int i=0;i<req.Count;i++) periods.Add(new PayPeriod{ ScheduleId=req.ScheduleId, StartDate=req.StartDate.AddDays(i*req.StepDays), EndDate=req.EndDate.AddDays(i*req.StepDays), PayDate=req.PayDate.AddDays(i*req.StepDays)});
        db.PayPeriods.AddRange(periods); await db.SaveChangesAsync(); return Ok(periods);
    }
}
public record GeneratePeriodsRequest(Guid ScheduleId, DateOnly StartDate, DateOnly EndDate, DateOnly PayDate, int Count, int StepDays);
