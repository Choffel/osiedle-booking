using FluentAssertions;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace UserModule.Domain;

public class UserIdTests
{
    [Fact]
    public void New_ShouldGenerateNonEmptyGuid()
    {
        UserId.New().Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_ShouldGenerateUniqueValues()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => UserId.New()).ToList();

        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void UserId_ShouldBeValueEqual()
    {
        var guid = Guid.NewGuid();

        UserId.From(guid).Should().Be(UserId.From(guid));
        UserId.From(guid).Should().NotBe(UserId.From(Guid.NewGuid()));
    }

    [Fact]
    public void From_ShouldRejectEmptyGuid()
    {
        var act = () => UserId.From(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
