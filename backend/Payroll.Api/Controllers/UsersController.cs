using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Domain;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = "PayrollAdmin")]
public class UsersController(PayrollDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Users.Select(u => new { u.Id, u.Username, u.Role }).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        if (await db.Users.AnyAsync(x => x.Username == req.Username)) return BadRequest(new { error = "Username already exists" });
        var user = new AppUser { Username = req.Username, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), Role = req.Role };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await audit.LogAsync(User.Identity!.Name!, "Create", "User", user.Id.ToString(), null, new { user.Username, user.Role });
        return Ok(new { user.Id, user.Username, user.Role });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        var before = new { user.Username, user.Role };
        user.Role = req.Role;
        if (!string.IsNullOrWhiteSpace(req.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        await db.SaveChangesAsync();
        await audit.LogAsync(User.Identity!.Name!, "Update", "User", id.ToString(), before, new { user.Username, user.Role });
        return Ok(new { user.Id, user.Username, user.Role });
    }
}

public record CreateUserRequest(string Username, string Password, string Role);
public record UpdateUserRequest(string Role, string? Password);
