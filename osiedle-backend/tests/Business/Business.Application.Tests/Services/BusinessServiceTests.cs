using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Services.BusinessServices;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Application.Tests.Services;

public class BusinessServiceTests
{
    private static readonly Address Location = Address.Create("Main Street", "1", "Warsaw");
    private static readonly AddressDto LocationDto = new("Main Street", "1", "Warsaw");

    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly BusinessService _sut;

    public BusinessServiceTests()
    {
        _sut = new BusinessService(_businessRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateBusinessAsync_WithFreeOwner_AddsBusinessAndSaves()
    {
        var dto = new CreateBusinessDto("Beauty Salon", UserId.New(), "Haircuts and more", LocationDto);
        BusinessAggregate? added = null;

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(dto.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _businessRepository
            .Setup(r => r.Add(It.IsAny<BusinessAggregate>()))
            .Callback<BusinessAggregate>(business => added = business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateBusinessAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(dto.Name, added!.Name);
        Assert.Equal(dto.OwnerId, added.OwnerId);
        Assert.Equal(dto.Description, added.Description);
        Assert.Equal(Address.Create(dto.Location.Street, dto.Location.Number, dto.Location.City), added.Location);
        Assert.Equal(BusinessStatus.Active, added.Status);
        Assert.Equal(added.Id.Value, result.Value.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBusinessAsync_WhenOwnerAlreadyHasBusiness_ReturnsConflict()
    {
        var dto = new CreateBusinessDto("Beauty Salon", UserId.New(), "Description", LocationDto);

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(dto.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateBusinessAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.AlreadyExists, result.Error);
        _businessRepository.Verify(r => r.Add(It.IsAny<BusinessAggregate>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBusinessByIdAsync_WhenBusinessExists_ReturnsMappedBusiness()
    {
        var business = BusinessAggregate.Create("Beauty Salon", UserId.New(), Location, "Description");
        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        var result = await _sut.GetBusinessByIdAsync(BusinessId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(business.Id.Value, result.Value.Id);
        Assert.Equal(business.Name, result.Value.Name);
        Assert.Equal(business.Status.ToString(), result.Value.Status);
    }

    [Fact]
    public async Task GetBusinessByIdAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.GetBusinessByIdAsync(BusinessId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessExists_UpdatesFieldsAndSaves()
    {
        var business = BusinessAggregate.Create("Old name", UserId.New(), Location, "Old description");
        var newLocation = new AddressDto("New Street", "2", "Krakow");
        var dto = new UpdateBusinessInfoDto("New name", "New description", newLocation);

        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateBusinessInfoAsync(business.Id, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.NewName, business.Name);
        Assert.Equal(dto.NewDescription, business.Description);
        Assert.Equal(newLocation.Street, business.Location.Street);
        Assert.Equal(newLocation.Number, business.Location.Number);
        Assert.Equal(newLocation.City, business.Location.City);
        Assert.Equal(dto.NewName, result.Value.Name);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.UpdateBusinessInfoAsync(
            BusinessId.New(), new UpdateBusinessInfoDto("New name", "New description", LocationDto));

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveBusinessAsync_WhenBusinessExists_ArchivesAndSaves()
    {
        var business = BusinessAggregate.Create("Name", UserId.New(), Location, "Description");

        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ArchiveBusinessAsync(business.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(BusinessStatus.Archived, business.Status);
        Assert.NotNull(business.DeletedAt);
        Assert.Equal(nameof(BusinessStatus.Archived), result.Value.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveBusinessAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.ArchiveBusinessAsync(BusinessId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
