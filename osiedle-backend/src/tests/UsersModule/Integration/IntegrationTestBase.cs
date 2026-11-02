using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.Roles;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Application.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.Data.InitialData;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Identity.Models;
using Users.Infrastructure.Identity.Services;
using Users.Infrastructure.Options;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Repositories.Tokens;
using Users.Infrastructure.Repositories.Users;
using Users.Infrastructure.Services;
using Users.Infrastructure.Services.Tokens;
using Users.Infrastructure.UnitOtWork;

namespace UserModule.Integration;

public sealed class TestUserContext : IUserContext
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public string RefreshToken { get; set; } = string.Empty;
    public string? IpAddress { get; set; } = "127.0.0.1";
}

public abstract class IntegrationTestBase : IDisposable
{
    protected const string ValidPassword = "Password1";

    private readonly SqliteConnection _connection;
    protected readonly ServiceProvider Provider;
    protected readonly TestUserContext UserContext = new();

    protected IntegrationTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = new string('s', 64),
                ["JwtSettings:Issuer"] = "test-issuer",
                ["JwtSettings:Audience"] = "test-audience",
                ["JwtSettings:AccessTokenExpiryMinutes"] = "15",
                ["JwtSettings:RefreshTokenExpiryDays"] = "7"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IUserContext>(UserContext);
        services.AddLogging();
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
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

        services.AddOptions<JwtSettings>().Bind(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<ITokenReadRepository, TokenReadRepository>();
        services.AddScoped<ITokenWriteRepository, TokenWriteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIdentityAccountService, IdentityAccountService>();
        services.AddScoped<IIdentityCredentialsService, IdentityCredentialsService>();
        services.AddScoped<IIdentityEmailService, IdentityEmailService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IUserService, UserService>();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleIdentity>>();
        RoleInitData.InitializeAsync(new[] { UserRoles.User }, roleManager).GetAwaiter().GetResult();
    }

    protected IServiceScope CreateScope() => Provider.CreateScope();

    protected async Task<Guid> RegisterUserAsync(
        IAuthService authService, string email, string password, string firstName = "Jan")
    {
        var result = await authService.RegisterAsync(
            new RegisterUserRequest(firstName, "Kowalski", email, password), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("registration must succeed, error: {0}", result.Error.Description);
        // RegisterAsync boxes the created id as Result<UserId>.
        return ((Result<UserId>)result).Value.Value;
    }

    protected async Task ConfirmEmailAsync(IServiceScope scope, string email)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        identityUser.Should().NotBeNull();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser!);

        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var result = await authService.ConfirmEmailAsync(new ConfirmEmailRequest(email, token));

        result.IsSuccess.Should().BeTrue("email confirmation must succeed, error: {0}", result.Error.Description);
    }

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
