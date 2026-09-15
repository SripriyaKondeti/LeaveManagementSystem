using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LeaveManagementSystem.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Validate username
        if (string.IsNullOrWhiteSpace(loginDto.Username))
        {
            throw new InvalidOperationException(
                "Username cannot be empty.");
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(loginDto.Password))
        {
            throw new InvalidOperationException(
                "Password cannot be empty.");
        }

        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == loginDto.Username.Trim());

        // Do not reveal whether username exists
        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        // Check user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        // Verify password
        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            loginDto.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        // Generate JWT
        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "JWT key is not configured.");
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        // Add EmployeeId only when the user is linked to an employee
        if (user.EmployeeId.HasValue)
        {
            claims.Add(
                new Claim(
                    "EmployeeId",
                    user.EmployeeId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
