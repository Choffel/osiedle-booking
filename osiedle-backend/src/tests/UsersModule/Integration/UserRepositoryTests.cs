using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Repositories;
using Users.Domain.Entity.User;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class UserRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripUser()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var id = UserId.New();

        await repository.AddAsync(User.Create(id, "Jan Kowalski"), CancellationToken.None);
        var loaded = await repository.GetByIdAsync(id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.DisplayName.Should().Be("Jan Kowalski");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var loaded = await repository.GetByIdAsync(UserId.New(), CancellationToken.None);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdBelongsToAnotherUser()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await repository.AddAsync(User.Create(UserId.New(), "Anna"), CancellationToken.None);

        var loaded = await repository.GetByIdAsync(UserId.New(), CancellationToken.None);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldPersistNewDisplayName()
    {
        var id = UserId.New();
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            await repository.AddAsync(User.Create(id, "Stara nazwa"), CancellationToken.None);

            var user = await repository.GetByIdAsync(id, CancellationToken.None);
            user!.Update("Nowa nazwa");
            await repository.Update(user);
        }

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reloaded = await db.Users.AsNoTracking().SingleAsync(u => u.Id == id);

        reloaded.DisplayName.Should().Be("Nowa nazwa");
    }
}
