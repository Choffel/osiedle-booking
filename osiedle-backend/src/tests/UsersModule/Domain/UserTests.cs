using FluentAssertions;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;

namespace UserModule.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldAssignIdAndDisplayName()
    {
        var id = UserId.New();

        var user = User.Create(id, "Jan Kowalski");

        user.Id.Should().Be(id);
        user.DisplayName.Should().Be("Jan Kowalski");
    }

    [Fact]
    public void Create_TwoUsersWithSameId_ShouldBeEqual()
    {
        var id = UserId.New();

        var first = User.Create(id, "Anna");
        var second = User.Create(id, "Inna nazwa");

        first.Should().Be(second);
    }

    [Fact]
    public void Create_UsersWithDifferentIds_ShouldNotBeEqual()
    {
        var first = User.Create(UserId.New(), "Anna");
        var second = User.Create(UserId.New(), "Anna");

        first.Should().NotBe(second);
    }

    [Fact]
    public void Update_ShouldChangeDisplayName()
    {
        var user = User.Create(UserId.New(), "Stara nazwa");

        user.Update("Nowa nazwa");

        user.DisplayName.Should().Be("Nowa nazwa");
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToToday()
    {
        var user = User.Create(UserId.New(), "Jan");

        user.Update("Janek");

        user.UpdatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
