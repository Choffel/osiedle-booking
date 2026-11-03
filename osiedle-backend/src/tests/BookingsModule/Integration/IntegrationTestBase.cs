using Bookings.Application.Abstractions;
using Bookings.Application.Abstractions.Repositories;
using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Application.Services;
using Bookings.Domain.Services;
using Bookings.Infrastructure.Data;
using Bookings.Infrastructure.Repositories;
using Bookings.Infrastructure.Repositories.Booking;
using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks;
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

/// <summary>
/// In-memory stand-in for the Business module's service catalog. Scoped so individual
/// tests can vary duration/price without racing the other test classes.
/// </summary>
public sealed class FakeBookableServiceApi : IBookableServiceInternalApi
{
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);
    public decimal Price { get; set; } = 100m;

    public Task<bool> ExistsAsync(BookableServiceId id, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<bool> BelongsToEmployeeAsync(BookableServiceId id, EmployeeId employeeId, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<Result<ServiceShortInfoDto>> GetServiceInfoAsync(BookableServiceId id, CancellationToken ct = default) =>
        Task.FromResult(Result<ServiceShortInfoDto>.Success(
            new ServiceShortInfoDto(id.Value, Guid.Empty, "Test service", Price, Duration)));

    public Task<Result<IReadOnlyCollection<ServiceShortInfoDto>>> GetServicesByEmployeeAsync(
        EmployeeId employeeId, CancellationToken ct = default) =>
        Task.FromResult(Result<IReadOnlyCollection<ServiceShortInfoDto>>.Success(
            Array.Empty<ServiceShortInfoDto>()));
}

/// <summary>
/// Booking flows under test never read working hours (the code path is commented out in
/// BookingService), so any unexpected call fails loudly instead of silently faking data.
/// </summary>
public sealed class FakeEmployeeInternalApi : IEmployeeInternalApi
{
    public Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<Result<EmployeeStatusDto>> GetStatusAsync(EmployeeId employeeId, CancellationToken ct = default) =>
        throw new NotSupportedException("Not used by the booking flows under test.");

    public Task<Result<EmployeeShortInfoDto>> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default) =>
        throw new NotSupportedException("Not used by the booking flows under test.");

    public Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursAsync(
        EmployeeId employeeId, DateOnly date, CancellationToken ct) =>
        throw new NotSupportedException("Not used by the booking flows under test.");
}

public abstract class IntegrationTestBase : IDisposable
{
    // Dynamic future date so the suite keeps working whenever it is run.
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
        services.AddScoped<IBookingReadRepository, BookingReadRepository>();
        services.AddScoped<ISlotCalculator, SlotCalculator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<FakeBookableServiceApi>();
        services.AddScoped<IBookableServiceInternalApi>(sp => sp.GetRequiredService<FakeBookableServiceApi>());
        services.AddSingleton<IEmployeeInternalApi, FakeEmployeeInternalApi>();
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

    protected static GetAvailableSlotsRequest SlotsRequest(Guid? staff = null) =>
        new(staff ?? Staff, Service, TestDate);

    protected async Task<Guid> CreateBookingAsync(
        IBookingService bookingService, DateTime start, DateTime end, Guid? staff = null, Guid? client = null)
    {
        var result = await bookingService.CreateBookingAsync(Request(start, end, staff, client), CancellationToken.None);
        result.IsSuccess.Should().BeTrue("booking creation must succeed, error: {0}", result.Error.Description);

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
