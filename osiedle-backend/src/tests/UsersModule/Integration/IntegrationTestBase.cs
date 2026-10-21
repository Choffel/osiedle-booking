using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Application.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.Data.InitialData;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Services;

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
                ["JWT_TOKEN_SETTINGS:SYMMETRIC_SECURITY_KEY"] = new string('s', 64),
                ["JWT_TOKEN_SETTINGS:VALID_ISSUER"] = "test-issuer",
                ["JWT_TOKEN_SETTINGS:VALID_AUDIENCE"] = "test-audience"
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleIdentity>>();
        RoleInitData.InitializeAsync(roleManager).GetAwaiter().GetResult();
    }

    protected IServiceScope CreateScope() => Provider.CreateScope();

    protected async Task<Guid> RegisterUserAsync(
        IUserService userService, string email, string password, string firstName = "Jan")
    {
        var result = await userService.RegisterAsync(
            new RegisterUserRequest(firstName, "Kowalski", email, password), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("registration must succeed, error: {0}", result.Error);
        return result.Value!.Value;
    }

    protected async Task ConfirmEmailAsync(IServiceScope scope, string email)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        identityUser.Should().NotBeNull();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser!);

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var result = await userService.ConfirmEmailAsync(new ConfirmEmailRequest(email, token));

        result.IsSuccess.Should().BeTrue("email confirmation must succeed, error: {0}", result.Error);
    }

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
