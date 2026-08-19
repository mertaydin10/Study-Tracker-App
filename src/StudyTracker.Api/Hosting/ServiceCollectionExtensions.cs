using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyTracker.Api.Data;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStudyTrackerApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddSingleton<PasswordHasher<User>>();

        var connectionString = configuration.GetConnectionString("StudyTracker")
            ?? throw new InvalidOperationException("Connection string 'StudyTracker' is missing.");

        services.AddDbContext<StudyTrackerDbContext>(options =>
            options.UseNpgsql(connectionString));

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
        services.AddAuthorization();

        return services;
    }
}
