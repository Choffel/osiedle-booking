using System.Net.Http.Json;
using FluentAssertions;
using WebApi.Tests.Support;

namespace WebApi.Tests.Controllers;

public class SmokeTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public SmokeTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Host_Starts_And_AvailableSlots_ReturnsFullSchedule()
    {
        var serviceId = await _factory.SeedBookableServiceAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/Bookings/available-slots?staffId={Guid.NewGuid()}&serviceId={serviceId}&date=2026-12-01");

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "body: {0}", body);
        var slots = await response.Content.ReadFromJsonAsync<List<SlotDto>>();
        slots.Should().HaveCount(52);
        slots!.First().Start.TimeOfDay.Should().Be(new TimeSpan(9, 0, 0));
        slots.Last().End.TimeOfDay.Should().Be(new TimeSpan(18, 0, 0));
    }

    private sealed record SlotDto(DateTime Start, DateTime End);
}
