using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Application.Services.BookableItemServices;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.Tests.InternalApis;

public class BookableServiceInternalApiTests
{
    private readonly Mock<IBookableServiceRepository> _bookableServiceRepository = new();
    private readonly BookableServiceInternalApi _sut;

    public BookableServiceInternalApiTests()
    {
        _sut = new BookableServiceInternalApi(_bookableServiceRepository.Object);
    }

    [Fact]
    public async Task ExistsAsync_DelegatesToRepository()
    {
        _bookableServiceRepository
            .Setup(r => r.ExistsAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsAsync(BookableServiceId.New());

        Assert.True(result);
    }

    [Fact]
    public async Task BelongsToEmployeeAsync_DelegatesToRepository()
    {
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.BelongsToEmployeeAsync(BookableServiceId.New(), EmployeeId.New());

        Assert.False(result);
    }

    [Fact]
    public async Task GetServiceInfoAsync_WhenServiceExists_ReturnsShortInfo()
    {
        var serviceInfo = new ServiceShortInfoDto(
            Guid.NewGuid(), Guid.NewGuid(), "Haircut", 50m, TimeSpan.FromMinutes(45));
        _bookableServiceRepository
            .Setup(r => r.GetBasicInfoByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceInfo);

        var result = await _sut.GetServiceInfoAsync(BookableServiceId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(serviceInfo, result.Value);
    }

    [Fact]
    public async Task GetServiceInfoAsync_WhenServiceMissing_ReturnsNotFound()
    {
        _bookableServiceRepository
            .Setup(r => r.GetBasicInfoByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceShortInfoDto?)null);

        var result = await _sut.GetServiceInfoAsync(BookableServiceId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetServicesByEmployeeAsync_ReturnsRepositoryCollection()
    {
        IReadOnlyList<ServiceShortInfoDto> services =
        [
            new ServiceShortInfoDto(Guid.NewGuid(), Guid.NewGuid(), "Haircut", 50m, TimeSpan.FromMinutes(45)),
            new ServiceShortInfoDto(Guid.NewGuid(), Guid.NewGuid(), "Beard trim", 30m, TimeSpan.FromMinutes(20))
        ];
        _bookableServiceRepository
            .Setup(r => r.GetShortInfosByEmployeeIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        var result = await _sut.GetServicesByEmployeeAsync(EmployeeId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(services, result.Value);
    }
}
