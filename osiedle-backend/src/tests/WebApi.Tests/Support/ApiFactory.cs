using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Business.Domain.Aggregates;
using Business.Domain.Entities;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using UsersDbContext = Users.Infrastructure.Data.ApplicationDbContext;
using BookingsDbContext = Bookings.Infrastructure.Data.ApplicationDbContext;
using Business.Infrastructure.Persistence;

namespace WebApi.Tests.Support;

/// <summary>
/// Boots the real WebApi host for tests: JWT settings are overridden, the three
/// Npgsql contexts are swapped to isolated SQLite databases (one per context),
/// and helpers seed data / craft access tokens the way production issues them.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string JwtSecret = "test-secret-test-secret-test-secret-test-secret-1234567890";
    public const string JwtIssuer = "test-issuer";
    public const string JwtAudience = "test-audience";

    private readonly SqliteConnection _usersDb = NewConnection();
    private readonly SqliteConnection _bookingsDb = NewConnection();
    private readonly SqliteConnection _businessDb = NewConnection();
    private bool _bookingsSchemaEnsured;

    private static SqliteConnection NewConnection() =>
        new($"DataSource=file:api-tests-{Guid.NewGuid():N}?mode=memory&cache=shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("JwtSettings:Secret", JwtSecret);
        builder.UseSetting("JwtSettings:Issuer", JwtIssuer);
        builder.UseSetting("JwtSettings:Audience", JwtAudience);
        builder.UseSetting("JwtSettings:AccessTokenExpiryMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpiryDays", "7");

        builder.ConfigureServices(services =>
        {
            Open(_usersDb);
            Open(_bookingsDb);
            Open(_businessDb);

            services.RemoveAll<IDbContextOptionsConfiguration<UsersDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BookingsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BusinessDbContext>>();

            // The snapshot is authored for Npgsql, so a SQLite-built model always
            // differs from it — MigrateAsync's pending-changes validation must be off.
            services.AddDbContext<UsersDbContext>(o => o
                .UseSqlite(_usersDb)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.AddDbContext<BookingsDbContext>(o => o
                .UseSqlite(_bookingsDb)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.AddDbContext<BusinessDbContext>(o => o
                .UseSqlite(_businessDb)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        // Runs after Program's startup block (migrations + role seeding) and before
        // any request can reach the Bookings endpoints. Must use the returned host's
        // services: the factory's `Services` property would re-enter StartServer here.
        EnsureBookingsSchema(host.Services);
        return host;
    }

    private static void Open(SqliteConnection connection)
    {
        if (connection.State != ConnectionState.Open)
            connection.Open();
    }

    /// <summary>
    /// Program migrates only Users and Business at startup, so the Bookings schema
    /// is created once here (idempotent) before the first booking endpoint runs.
    /// </summary>
    public void EnsureBookingsSchema() => EnsureBookingsSchema(Services);

    private void EnsureBookingsSchema(IServiceProvider services)
    {
        if (_bookingsSchemaEnsured)
            return;

        _bookingsSchemaEnsured = true;
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        db.Database.EnsureCreated();
    }

    // ---------- access tokens ----------

    public string CreateAccessToken(Guid userId, string email, params string[] roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            JwtIssuer,
            JwtAudience,
            claims,
            notBefore: now.AddMinutes(-1),
            expires: now.AddMinutes(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return handler.WriteToken(token);
    }

    // ---------- users ----------

    /// <summary>Registers a user over HTTP and returns the created user id.</summary>
    public async Task<Guid> RegisterUserAsync(
        HttpClient client, string email, string password, string? firstName = null, string lastName = "Kowalski")
    {
        // Identity's username is the first name and is unique per database,
        // so an explicit name is only safe when the test asserts on it.
        firstName ??= "u" + Guid.NewGuid().ToString("N")[..8];

        var response = await client.PostAsJsonAsync("/api/Authentication/register", new
        {
            firstName,
            lastName,
            email,
            password,
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed: {(int)response.StatusCode} {body}");
        }
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>Generates a real password-reset token through UserManager (test stand-in for the mail).</summary>
    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Users.Infrastructure.Identity.Models.ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new InvalidOperationException($"No identity user for {email}");
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <summary>Confirms the email through the API using a token generated via UserManager.</summary>
    public async Task ConfirmEmailAsync(HttpClient client, string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Users.Infrastructure.Identity.Models.ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new InvalidOperationException($"No identity user for {email}");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var response = await client.PostAsJsonAsync("/api/Authentication/confirm-email", new { email, token });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Confirm failed: {(int)response.StatusCode} {body}");
        }
    }

    // ---------- seeding ----------

    /// <summary>Seeds a domain user plus an active refresh token; returns the token id.</summary>
    public async Task<Guid> SeedRefreshTokenAsync(Guid userId, string refreshToken, string? email = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var owner = await db.Users.SingleOrDefaultAsync(u => u.Id == UserId.From(userId));
        if (owner is null)
        {
            owner = User.Create(
                UserId.From(userId),
                Name.Create("Jan", "Kowalski"),
                email ?? $"owner-{userId:N}@example.com");
            db.Users.Add(owner);
        }

        var tokenId = RefreshTokenId.New();
        db.RefreshTokens.Add(RefreshToken.Create(
            tokenId, refreshToken, UserId.From(userId), DateTime.UtcNow.AddDays(30), DateTime.UtcNow));

        await db.SaveChangesAsync();
        return tokenId.Value;
    }

    public async Task<Guid> SeedBookableServiceAsync(decimal price = 100m, TimeSpan? duration = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BusinessDbContext>();

        // BookableServices.EmployeeId is a foreign key to Employees (configured from
        // the Employee aggregate), so the employee row must exist first.
        var employee = Employee.Create(
            businessId: null, "Test", "Employee", "Barber", null, $"emp-{Guid.NewGuid():N}@example.com");
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var service = BookableService.Create(
            employee.Id, "Strzyzenie", "Test service", price, duration ?? TimeSpan.FromMinutes(30));
        db.BookableServices.Add(service);
        await db.SaveChangesAsync();

        return service.Id.Value;
    }

    // ---------- misc ----------

    public async Task<T> QueryUsersDbAsync<T>(Func<UsersDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<UsersDbContext>());
    }

    public async Task<T> QueryBookingsDbAsync<T>(Func<BookingsDbContext, Task<T>> query)
    {
        EnsureBookingsSchema();
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<BookingsDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _usersDb.Dispose();
        _bookingsDb.Dispose();
        _businessDb.Dispose();
    }
}
