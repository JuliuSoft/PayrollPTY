using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Infrastructure;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase {
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) {
        var token = await authService.Login(request.Username, request.Password);
        return token is null ? Unauthorized(new { error = "Invalid credentials" }) : Ok(new { token });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new { user = User.Identity?.Name, role = User.Claims.FirstOrDefault(c=>c.Type.EndsWith("role"))?.Value });
}

public record LoginRequest(string Username, string Password);
