using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StudyTracker.Api.Auth;
using StudyTracker.Api.Data;
using StudyTracker.Api.Dtos.Auth;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    StudyTrackerDbContext db,
    IConfiguration configuration,
    PasswordHasher<User> passwordHasher) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim(), cancellationToken);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return Unauthorized(new { error = "E-posta veya şifre yanlış." });

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { error = "E-posta veya şifre yanlış." });

        return Ok(IssueToken(user));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? email
            : request.DisplayName.Trim();

        var user = new User
        {
            Email = email,
            DisplayName = displayName
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg
                                           && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { error = "Bu e-posta zaten kayıtlı." });
        }

        return Created("/api/auth/login", IssueToken(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var id = User.GetRequiredUserId();
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return Unauthorized();

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var id = User.GetRequiredUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return Unauthorized();

        var name = request.DisplayName.Trim();
        if (name.Length == 0)
            return BadRequest(new { error = "Ad boş olamaz." });

        user.DisplayName = name;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    private LoginResponse IssueToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key eksik.");
        var issuer = configuration["Jwt:Issuer"] ?? "StudyTracker";
        var audience = configuration["Jwt:Audience"] ?? "StudyTracker";
        var expires = DateTimeOffset.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}
