using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);

        return Ok(response);
    }
}