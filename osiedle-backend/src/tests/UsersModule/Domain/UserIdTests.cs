using FluentAssertions;
using Users.Domain.Entity.User.Value_objects;

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

        new UserId(guid).Should().Be(new UserId(guid));
        new UserId(guid).Should().NotBe(new UserId(Guid.NewGuid()));
    }
}
