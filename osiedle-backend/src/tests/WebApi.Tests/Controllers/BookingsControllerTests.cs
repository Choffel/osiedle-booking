using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

public class BookingsControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _serviceId;
    private readonly DateOnly _date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30));

    private static readonly Guid Business = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Staff = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ClientA = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ClientB = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // Tests in this class share one database, so each test owns a distinct time window.
    public BookingsControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _serviceId = factory.SeedBookableServiceAsync().GetAwaiter().GetResult();
    }

    private string At(int hour, int minute) => Dt(hour, minute).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

    private DateTime Dt(int hour, int minute) => _date.ToDateTime(new TimeOnly(hour, minute));

    private string BookingUrl(string start, string end, Guid? client = null) =>
        $"/api/Bookings/booking?businessId={Business}&clientId={client ?? ClientA}" +
        $"&staffId={Staff}&serviceId={_serviceId}&startTime={start}&endTime={end}";

    private Task<HttpResponseMessage> CreateBookingAsync(string start, string end, Guid? client = null) =>
        _client.PostAsync(BookingUrl(start, end, client), null);

    private string Bearer(Guid clientId) => _factory.CreateAccessToken(clientId, $"client-{clientId:N}@example.com");

    private HttpRequestMessage DeleteReservation(Guid bookingId, Guid clientId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Bookings/reservations")
        {
            Content = JsonContent.Create(new { bookingId }),
        };
        request.Headers.Authorization = new("Bearer", Bearer(clientId));
        return request;
    }

    // ---------- GET available-slots ----------

    [Fact]
    public async Task AvailableSlots_UnknownService_Returns404WithDomainCode()
    {
        var response = await _client.GetAsync(
            $"/api/Bookings/available-slots?staffId={Staff}&serviceId={Guid.NewGuid()}&date={_date:yyyy-MM-dd}");

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "BookableService.NotFound");
    }

    // ---------- POST booking ----------

    [Fact]
    public async Task CreateBooking_Returns204_AndPersistsConfirmedBooking()
    {
        var response = await CreateBookingAsync(At(10, 0), At(10, 30));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var booking = await _factory.QueryBookingsDbAsync(db =>
            db.Bookings.AsNoTracking().SingleAsync(b => b.Slot.Start == Dt(10, 0)));
        booking.Status.Should().Be(Bookings.Domain.Entities.Booking.Enum.BookingStatus.Confirmed);
        booking.Slot.Start.TimeOfDay.Should().Be(new TimeSpan(10, 0, 0));
        booking.ClientId.Value.Should().Be(ClientA);
    }

    [Fact]
    public async Task CreateBooking_OverlappingSlot_Returns400WithBookingCode()
    {
        (await CreateBookingAsync(At(12, 0), At(12, 30))).EnsureSuccessStatusCode();

        var overlapping = await CreateBookingAsync(At(12, 15), At(12, 45));

        await overlapping.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Booking.Overlapping");

        var count = await _factory.QueryBookingsDbAsync(db => db.Bookings.AsNoTracking()
            .CountAsync(b => b.Slot.Start >= Dt(12, 0) && b.Slot.Start < Dt(13, 0)));
        count.Should().Be(1, "the rejected booking must not be persisted");
    }

    [Fact]
    public async Task CreateBooking_BeforeWorkingHours_Returns400()
    {
        var response = await CreateBookingAsync(At(8, 0), At(8, 30));

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Booking.Overlapping");
    }

    [Fact]
    public async Task CreateBooking_UnknownService_Returns404()
    {
        var response = await _client.PostAsync(
            $"/api/Bookings/booking?businessId={Business}&clientId={ClientA}&staffId={Staff}" +
            $"&serviceId={Guid.NewGuid()}&startTime={At(17, 0)}&endTime={At(17, 30)}", null);

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "BookableService.NotFound");
    }

    // ---------- GET reservations ----------

    [Fact]
    public async Task Reservations_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/Bookings/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reservations_ReturnsOnlyOwnBookings()
    {
        (await CreateBookingAsync(At(13, 0), At(13, 30), client: ClientA)).EnsureSuccessStatusCode();
        (await CreateBookingAsync(At(14, 0), At(14, 30), client: ClientB)).EnsureSuccessStatusCode();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/Bookings/reservations");
        request.Headers.Authorization = new("Bearer", Bearer(ClientA));
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var reservations = body.RootElement.EnumerateArray().ToList();
        reservations.Should().HaveCount(1, "bookings of other clients must not be visible");
        reservations[0].GetProperty("date").GetDateTime().TimeOfDay.Should().Be(new TimeSpan(13, 0, 0));
    }

    // ---------- DELETE reservations ----------

    [Fact]
    public async Task Cancel_OwnBooking_Returns204_AndCancels()
    {
        (await CreateBookingAsync(At(15, 0), At(15, 30), client: ClientA)).EnsureSuccessStatusCode();
        var created = await _factory.QueryBookingsDbAsync(db =>
            db.Bookings.AsNoTracking().SingleAsync(b => b.Slot.Start == Dt(15, 0)));

        var response = await _client.SendAsync(DeleteReservation(created.Id.Value, ClientA));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var booking = await _factory.QueryBookingsDbAsync(db =>
            db.Bookings.AsNoTracking().SingleAsync(b => b.Id == created.Id));
        booking.Status.Should().Be(Bookings.Domain.Entities.Booking.Enum.BookingStatus.Cancelled);
        booking.Cancellation.Should().NotBeNull();
    }

    [Fact]
    public async Task Cancel_OthersBooking_Returns400AndKeepsItConfirmed()
    {
        (await CreateBookingAsync(At(16, 0), At(16, 30), client: ClientA)).EnsureSuccessStatusCode();
        var created = await _factory.QueryBookingsDbAsync(db =>
            db.Bookings.AsNoTracking().SingleAsync(b => b.Slot.Start == Dt(16, 0)));

        var response = await _client.SendAsync(DeleteReservation(created.Id.Value, ClientB));

        await response.ShouldFailWithAsync(HttpStatusCode.BadRequest, "Booking.NotOwnedByClient");
        var booking = await _factory.QueryBookingsDbAsync(db =>
            db.Bookings.AsNoTracking().SingleAsync(b => b.Id == created.Id));
        booking.Status.Should().Be(Bookings.Domain.Entities.Booking.Enum.BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Cancel_UnknownBooking_Returns404()
    {
        var response = await _client.SendAsync(DeleteReservation(Guid.NewGuid(), ClientA));

        await response.ShouldFailWithAsync(HttpStatusCode.NotFound, "Booking.NotFound");
    }
}
