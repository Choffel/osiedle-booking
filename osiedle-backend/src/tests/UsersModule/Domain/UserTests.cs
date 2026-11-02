using FluentAssertions;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace UserModule.Domain;

public class UserTests
{
    private static User Create(string first, string last, string? id = null) =>
        User.Create(UserId.From(Guid.NewGuid()), Name.Create(first, last), $"{first.ToLowerInvariant()}@example.com");

    [Fact]
    public void Create_ShouldAssignIdAndName()
    {
        var id = UserId.New();

        var user = User.Create(id, Name.Create("Jan", "Kowalski"), "jan@example.com");

        user.Id.Should().Be(id);
        user.Name.FirstName.Should().Be("Jan");
        user.Name.LastName.Should().Be("Kowalski");
        user.Email.Should().Be("jan@example.com");
    }

    [Fact]
    public void Create_TwoUsersWithSameId_ShouldBeEqual()
    {
        var id = UserId.New();

        var first = User.Create(id, Name.Create("Anna", "Kowalska"), "anna@example.com");
        var second = User.Create(id, Name.Create("Inna", "Nazwa"), "inna@example.com");

        first.Should().Be(second);
    }

    [Fact]
    public void Create_UsersWithDifferentIds_ShouldNotBeEqual()
    {
        var first = Create("Anna", "Kowalska");
        var second = Create("Anna", "Kowalska");

        first.Should().NotBe(second);
    }

    [Fact]
    public void Update_ShouldChangeName()
    {
        var user = Create("Stara", "Nazwa");

        user.Update(Name.Create("Nowa", "Nazwa"));

        user.Name.FirstName.Should().Be("Nowa");
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToToday()
    {
        var user = Create("Jan", "Kowalski");

        user.Update(Name.Create("Janek", "Kowalski"));

        user.UpdatedAt!.Value.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmailIsEmpty()
    {
        var act = () => User.Create(UserId.New(), Name.Create("Jan", "Kowalski"), string.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
