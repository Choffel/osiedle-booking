using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Users.Appliation.Abstractions;
using Users.Appliation.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Services;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database
        var connectionString = configuration.GetConnectionString("Database")
                               ?? configuration["CONNECTION_STRING"];
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        #endregion

        #region Identity
        services.AddIdentity<ApplicationUser, RoleIdentity>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        #endregion

        #region HTTP Context
        services.AddHttpContextAccessor();
        #endregion

        #region Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRoleService, RoleService>();
        #endregion

        #region Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        #endregion
        
        #region JWT Authentication
        var jwtKey = configuration["JWT_TOKEN_SETTINGS:SYMMETRIC_SECURITY_KEY"]!;
        var jwtIssuer = configuration["JWT_TOKEN_SETTINGS:ISSUER"]!;
        var jwtAudience = configuration["JWT_TOKEN_SETTINGS:AUDIENCE"]!;
        
        Console.WriteLine($"JWT Key: {jwtKey}");
        Console.WriteLine($"JWT Issuer: {jwtIssuer}");
        Console.WriteLine($"JWT Audience: {jwtAudience}");
        Console.WriteLine($"Connection String: {connectionString}");
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddAuthorization();
        #endregion
        
        return services;
    }
}