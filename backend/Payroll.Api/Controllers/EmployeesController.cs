using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("employees")]
[Authorize(Roles = "PayrollAdmin,HR,Auditor")]
public class EmployeesController(PayrollDbContext db, IAuditService audit) : ControllerBase {
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? q) => Ok(await db.Employees.Where(e => q == null || e.FullName.Contains(q)).ToListAsync());
    [HttpPost] [Authorize(Roles = "PayrollAdmin,HR")] public async Task<IActionResult> Create(Employee e) { db.Employees.Add(e); await db.SaveChangesAsync(); await audit.LogAsync(User.Identity!.Name!, "Create", "Employee", e.Id.ToString(), null, e); return Ok(e); }
    [HttpPut("{id:guid}")] [Authorize(Roles = "PayrollAdmin,HR")] public async Task<IActionResult> Update(Guid id, Employee update) {
        var e = await db.Employees.FindAsync(id); if (e is null) return NotFound(); var before = new { e.FullName, e.Department, e.BankAccount, e.BaseSalary, e.Status };
        e.FullName = update.FullName; e.Department = update.Department; e.BankAccount = update.BankAccount; e.BaseSalary = update.BaseSalary; e.Status = update.Status; await db.SaveChangesAsync();
        await audit.LogAsync(User.Identity!.Name!, "Update", "Employee", id.ToString(), before, e);
        return Ok(e);
    }
}
