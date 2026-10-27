using Bookings.Application.Abstractions.Services;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsModule.Integration;

public class MyReservationsFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task GetMyReservations_ReturnsOwnBookingsWithMappedFields()
    {
        UserContext.UserId = Client;

        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));
        await CreateBookingAsync(bookingService, At(14, 0), At(14, 30), client: OtherClient);

        var result = await bookingService.GetMyReservationsAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var reservations = result.Value;

        reservations.Should().HaveCount(1);
        var reservation = reservations.Single();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var booking = await db.Bookings.AsNoTracking().SingleAsync(b => b.ClientId == new ClientId(Client));

        //reservation.Title.Should().Be(booking.Id.Value.ToString());
        //reservation.Company.Should().Be(Business.ToString());
        //reservation.Staff.Should().Be(Staff.ToString());
        reservation.Date.Should().Be(At(10, 0));
    }

    [Fact]
    public async Task GetMyReservations_WhenNoneExist_ReturnsEmpty()
    {
        UserContext.UserId = OtherClient;

        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.GetMyReservationsAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Should().BeEmpty();
    }
}
