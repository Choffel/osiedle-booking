using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class UserRepositoryTests : IntegrationTestBase
{
    private static async Task SaveAsync(IServiceScope scope) =>
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

    [Fact]
    public async Task Add_ThenGetByIdAsync_ShouldRoundTripUser()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var id = UserId.New();

        repository.Add(User.Create(id, Name.Create("Jan", "Kowalski"), "jan@example.com"));
        await SaveAsync(scope);

        var loaded = await repository.GetByIdAsync(id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.Name.FirstName.Should().Be("Jan");
        loaded.Name.LastName.Should().Be("Kowalski");
        loaded.Email.Should().Be("jan@example.com");
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
        repository.Add(User.Create(UserId.New(), Name.Create("Anna", "Kowalska"), "anna@example.com"));
        await SaveAsync(scope);

        var loaded = await repository.GetByIdAsync(UserId.New(), CancellationToken.None);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldPersistNewName_ThroughChangeTracking()
    {
        var id = UserId.New();
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            repository.Add(User.Create(id, Name.Create("Stara", "Nazwa"), "stara@example.com"));
            await SaveAsync(scope);

            // The repository exposes no Update method: GetByIdAsync returns a tracked
            // entity and SaveChangesAsync persists the domain change.
            var user = await repository.GetByIdAsync(id, CancellationToken.None);
            user!.Update(Name.Create("Nowa", "Nazwa"));
            await SaveAsync(scope);
        }

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reloaded = await db.Users.AsNoTracking().SingleAsync(u => u.Id == id);

        reloaded.Name.FirstName.Should().Be("Nowa");
        reloaded.UpdatedAt.Should().NotBeNull();
    }
}
