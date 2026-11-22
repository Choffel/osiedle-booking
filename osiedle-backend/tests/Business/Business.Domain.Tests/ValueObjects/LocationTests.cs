using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.ValueObjects;

public class LocationTests
{
    [Fact]
    public void Create_WithValidData_ReturnsLocation()
    {
        var location = Location.Create("Warszawa", "ul. Główna 10");

        Assert.Equal("Warszawa", location.City);
        Assert.Equal("ul. Główna 10", location.Street);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var location = Location.Create("  Warszawa  ", "  ul. Główna 10  ");

        Assert.Equal("Warszawa", location.City);
        Assert.Equal("ul. Główna 10", location.Street);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidCity_ThrowsArgumentException(string? city)
    {
        Assert.Throws<ArgumentException>(() => Location.Create(city!, "ul. Główna 10"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidStreet_ThrowsArgumentException(string? street)
    {
        Assert.Throws<ArgumentException>(() => Location.Create("Warszawa", street!));
    }

    [Fact]
    public void ToString_ReturnsFormattedAddress()
    {
        var location = Location.Create("Warszawa", "ul. Główna 10");

        Assert.Equal("ul. Główna 10, Warszawa", location.ToString());
    }
}
