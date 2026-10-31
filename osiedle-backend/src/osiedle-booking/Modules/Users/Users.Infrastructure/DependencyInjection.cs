using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Identity.Models;
using Users.Infrastructure.Identity.Services;
using Users.Infrastructure.Options;
using Users.Infrastructure.Repositories.Tokens;
using Users.Infrastructure.Repositories.Users;
using Users.Infrastructure.UnitOtWork;

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
                
                options.Lockout.AllowedForNewUsers = configuration.GetValue<bool>("Identity:Lockout:AllowedForNewUsers", true);
                options.Lockout.MaxFailedAccessAttempts = configuration.GetValue<int>("Identity:Lockout:MaxFailedAccessAttempts", 5);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                    configuration.GetValue<int>("Identity:Lockout:DefaultLockoutMinutes", 15));
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        #endregion

        #region HTTP Context
        services.AddHttpContextAccessor();
        #endregion

        #region Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIdentityAccountService, IdentityAccountService>();
        services.AddScoped<IIdentityCredentialsService, IdentityCredentialsService>();
        services.AddScoped<IIdentityEmailService, IdentityEmailService>();
        services.AddScoped<IRoleService, RoleService>();
        #endregion

        #region Repositories
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenReadRepository, TokenReadRepository>();
        services.AddScoped<ITokenWriteRepository, TokenWriteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        #endregion
        
        #region JWT Authentication
        var jwtSettings = configuration
                              .GetSection(JwtSettings.SectionName)
                              .Get<JwtSettings>() 
                          ?? throw new InvalidOperationException("JwtSettings section is missing in configuration.");

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddAuthorization();
        #endregion
        
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret), 
                "JWT Secret cannot be empty.")
            .Validate(settings => settings.Secret.Length >= 32, 
                "JWT Secret must be at least 256 bits (32 characters) long.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), 
                "JWT Issuer cannot be empty.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), 
                "JWT Audience cannot be empty.")
            .Validate(settings => settings.ExpiryMinutes > 0, 
                "JWT ExpiryMinutes must be greater than 0.")
            .ValidateOnStart();
        
        return services;
    }
}