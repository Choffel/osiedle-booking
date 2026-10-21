using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Users.Infrastructure.Identity;

namespace UserModule.Infrastructure.Identity;

public class RoleServiceTests
{
    private readonly Mock<RoleManager<RoleIdentity>> _roleManagerMock;
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        _roleManagerMock = new Mock<RoleManager<RoleIdentity>>(
            Mock.Of<IRoleStore<RoleIdentity>>(),
            Array.Empty<IRoleValidator<RoleIdentity>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<RoleIdentity>>>());

        _sut = new RoleService(_roleManagerMock.Object);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleIsPresent()
    {
        _roleManagerMock.Setup(m => m.RoleExistsAsync("User")).ReturnsAsync(true);

        var result = await _sut.RoleExistsAsync("User", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleIsMissing()
    {
        _roleManagerMock.Setup(m => m.RoleExistsAsync("Ghost")).ReturnsAsync(false);

        var result = await _sut.RoleExistsAsync("Ghost", CancellationToken.None);

        result.Should().BeFalse();
    }
}
