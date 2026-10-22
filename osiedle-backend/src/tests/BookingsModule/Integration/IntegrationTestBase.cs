using Bookings.Application.Abstractions.Repositories;
using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Application.Services;
using Bookings.Domain.Services;
using Bookings.Infrastructure.Data;
using Bookings.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace BookingsModule.Integration;

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
    // Dynamic future date: Booking.Create rejects past slots, so the fixture must stay
    // in the future regardless of when the suite runs.
    protected static readonly DateOnly TestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30));

    protected static readonly Guid Business = Guid.Parse("11111111-1111-1111-1111-111111111111");
    protected static readonly Guid Staff = Guid.Parse("22222222-2222-2222-2222-222222222222");
    protected static readonly Guid OtherStaff = Guid.Parse("33333333-3333-3333-3333-333333333333");
    protected static readonly Guid Client = Guid.Parse("44444444-4444-4444-4444-444444444444");
    protected static readonly Guid OtherClient = Guid.Parse("55555555-5555-5555-5555-555555555555");
    protected static readonly Guid Service = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private readonly SqliteConnection _connection;
    protected readonly ServiceProvider Provider;
    protected readonly TestUserContext UserContext = new();

    protected IntegrationTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUserContext>(UserContext);
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ISlotCalculator, SlotCalculator>();
        services.AddScoped<IBookingService, BookingService>();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    protected IServiceScope CreateScope() => Provider.CreateScope();

    protected static DateTime At(int hour, int minute) =>
        TestDate.ToDateTime(new TimeOnly(hour, minute));

    protected static CreateBookingRequest Request(DateTime start, DateTime end, Guid? staff = null, Guid? client = null) =>
        new(Business, client ?? Client, staff ?? Staff, Service, start, end);

    protected static GetAvailableSlotsRequest SlotsRequest(Guid? staff = null, int serviceMinutes = 30) =>
        new(staff ?? Staff, new TimeOnly(0, serviceMinutes), TestDate);

    protected async Task<Guid> CreateBookingAsync(
        IBookingService bookingService, DateTime start, DateTime end, Guid? staff = null, Guid? client = null)
    {
        var result = await bookingService.CreateBookingAsync(Request(start, end, staff, client), CancellationToken.None);
        result.IsSuccess.Should().BeTrue("booking creation must succeed, error: {0}", result.Error);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var booking = await db.Bookings.AsNoTracking()
            .SingleAsync(b => b.Slot.Start == start && b.Slot.End == end);
        return booking.Id.Value;
    }

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
