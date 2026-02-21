using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Payroll.Application;
using Payroll.Domain;

namespace Payroll.Infrastructure;

public interface IAuthService { Task<string?> Login(string username, string password); }
public class AuthService(PayrollDbContext db, IConfiguration config) : IAuthService {
    public async Task<string?> Login(string username, string password) {
        var user = await db.Users.FirstOrDefaultAsync(x=>x.Username==username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "dev-secret-key-very-long"));
        var token = new JwtSecurityToken(claims: [new Claim(ClaimTypes.Name,user.Username), new Claim(ClaimTypes.Role,user.Role)], expires: DateTime.UtcNow.AddHours(8), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public interface IAuditService { Task LogAsync(string userId,string action,string entity,string entityId,object? before,object? after); }
public class AuditService(PayrollDbContext db) : IAuditService {
    public async Task LogAsync(string userId, string action, string entity, string entityId, object? before, object? after) {
        db.AuditEvents.Add(new AuditEvent{UserId=userId,Action=action,Entity=entity,EntityId=entityId,BeforeJson=before is null?null:System.Text.Json.JsonSerializer.Serialize(before),AfterJson=after is null?null:System.Text.Json.JsonSerializer.Serialize(after)});
        await db.SaveChangesAsync();
    }
}
