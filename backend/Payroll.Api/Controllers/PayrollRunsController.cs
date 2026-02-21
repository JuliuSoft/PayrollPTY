using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Application;
using Payroll.Domain;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("payroll-runs")]
[Authorize]
public class PayrollRunsController(PayrollDbContext db, IPayrollEngine engine, IAuditService audit) : ControllerBase {
    [HttpPost] [Authorize(Roles="PayrollAdmin,Finance")]
    public async Task<IActionResult> Create(CreatePayrollRunRequest req) {
        var run = new PayrollRun{ PeriodId=req.PeriodId, RunType=req.RunType, CreatedBy=User.Identity!.Name!}; db.PayrollRuns.Add(run); await db.SaveChangesAsync(); await audit.LogAsync(User.Identity!.Name!,"Create","PayrollRun",run.Id.ToString(),null,run); return Ok(run);
    }
    [HttpGet] public async Task<IActionResult> List() => Ok(await db.PayrollRuns.OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id) => Ok(await db.PayrollRuns.FindAsync(id) ?? throw new Exception("not found"));
    [HttpGet("{id:guid}/results")] public async Task<IActionResult> Results(Guid id) => Ok(await db.PayrollResults.Where(x=>x.RunId==id).ToListAsync());

    [HttpPost("{id:guid}/validate")] public async Task<IActionResult> Validate(Guid id) => await Step(id, PayrollRunStatus.Validated);
    [HttpPost("{id:guid}/approve")] [Authorize(Roles="PayrollAdmin,Finance")] public async Task<IActionResult> Approve(Guid id) => await Step(id, PayrollRunStatus.Approved, true);
    [HttpPost("{id:guid}/lock")] [Authorize(Roles="PayrollAdmin")]
    public async Task<IActionResult> Lock(Guid id) {
        var run = await db.PayrollRuns.FindAsync(id); if (run is null) return NotFound(); if (run.Status != PayrollRunStatus.Approved) return BadRequest(new { error="Run must be approved before lock"});
        run.Status = PayrollRunStatus.Locked; run.LockedAt = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.LogAsync(User.Identity!.Name!,"Lock","PayrollRun",id.ToString(),null,run); return Ok(run);
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id) {
        var run = await db.PayrollRuns.FindAsync(id); if (run is null) return NotFound(); if (run.Status == PayrollRunStatus.Locked) return BadRequest(new { error="Run is locked/immutable"});
        var config = await db.StatutoryConfigs.OrderByDescending(x=>x.EffectiveDate).FirstAsync();
        var employees = await db.Employees.Where(x=>x.Status==EmployeeStatus.Active).ToListAsync();
        var old = db.PayrollResults.Where(x=>x.RunId==id); db.PayrollResults.RemoveRange(old); await db.SaveChangesAsync();
        foreach (var e in employees) {
            var calc = engine.Compute(new PayrollInput{ Employee=e, Config=config});
            var r = new PayrollResult{ RunId=id, EmployeeId=e.Id, Gross=calc.Gross, Deductions=calc.Deductions, Net=calc.Net, EmployerCostTotal=calc.EmployerCost};
            db.PayrollResults.Add(r); await db.SaveChangesAsync();
            db.PayrollLineItems.AddRange(calc.LineItems.Select(li => new PayrollLineItem{ ResultId=r.Id, Category=li.Category, Code=li.Code, Description=li.Description, Amount=li.Amount, Base=li.Base, Rate=li.Rate, RuleId=li.RuleId, EffectiveDate=li.EffectiveDate, RoundingMode=li.RoundingMode }));
        }
        run.Status = PayrollRunStatus.Preview;
        await db.SaveChangesAsync();
        return Ok(new { runId=id, results = await db.PayrollResults.CountAsync(x=>x.RunId==id)});
    }

    [HttpGet("{id:guid}/payslips/{employeeId:guid}")]
    public async Task<IActionResult> Payslip(Guid id, Guid employeeId) {
        var result = await db.PayrollResults.FirstOrDefaultAsync(x=>x.RunId==id && x.EmployeeId==employeeId); if (result is null) return NotFound();
        var content = Encoding.UTF8.GetBytes($"Payslip Run:{id}\nEmployee:{employeeId}\nGross:{result.Gross}\nNet:{result.Net}");
        return File(content, "application/pdf", $"payslip-{employeeId}.pdf");
    }

    private async Task<IActionResult> Step(Guid id, PayrollRunStatus status, bool setApprover = false) {
        var run = await db.PayrollRuns.FindAsync(id); if (run is null) return NotFound(); if (run.Status==PayrollRunStatus.Locked) return BadRequest(new { error="Run is locked/immutable"});
        run.Status = status; if (setApprover) run.ApprovedBy = User.Identity!.Name!; await db.SaveChangesAsync(); await audit.LogAsync(User.Identity!.Name!, status.ToString(), "PayrollRun", id.ToString(), null, run); return Ok(run);
    }
}

public record CreatePayrollRunRequest(Guid PeriodId, PayrollRunType RunType);
