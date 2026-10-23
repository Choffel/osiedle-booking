using Business.Application.DTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Services.BusinessServices;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Application.Tests.Services;

public class BusinessServiceTests
{
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
        var dto = new CreateBusinessDto("Beauty Salon", Guid.NewGuid(), "Haircuts and more", "Main Street 1");
        BusinessAggregate? added = null;

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(dto.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _businessRepository
            .Setup(r => r.Add(It.IsAny<BusinessAggregate>(), It.IsAny<CancellationToken>()))
            .Callback<BusinessAggregate, CancellationToken>((business, _) => added = business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateBusinessAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(dto.Name, added!.Name);
        Assert.Equal(dto.OwnerId, added.OwnerId);
        Assert.Equal(dto.Description, added.Description);
        Assert.Equal(dto.Location, added.Location);
        Assert.Equal(BusinessStatus.Active, added.Status);
        Assert.Equal(added.Id.Value, result.Value.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBusinessAsync_WhenOwnerAlreadyHasBusiness_ReturnsConflict()
    {
        var dto = new CreateBusinessDto("Beauty Salon", Guid.NewGuid(), "Description", "Location");

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(dto.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateBusinessAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.AlreadyExists, result.Error);
        _businessRepository.Verify(r => r.Add(It.IsAny<BusinessAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBusinessByIdAsync_WhenBusinessExists_ReturnsMappedBusiness()
    {
        var business = BusinessAggregate.Create("Beauty Salon", Guid.NewGuid(), "Description", "Location");
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        var result = await _sut.GetBusinessByIdAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(business.Id.Value, result.Value.Id);
        Assert.Equal(business.Name, result.Value.Name);
        Assert.Equal(business.Status.ToString(), result.Value.Status);
    }

    [Fact]
    public async Task GetBusinessByIdAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.GetBusinessByIdAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessExists_UpdatesFieldsAndSaves()
    {
        var business = BusinessAggregate.Create("Old name", Guid.NewGuid(), "Old description", "Old location");
        var dto = new UpdateBusinessInfoDto("New name", "New description", "New location");

        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateBusinessInfoAsync(business.Id.Value, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.NewName, business.Name);
        Assert.Equal(dto.NewDescription, business.Description);
        Assert.Equal(dto.NewLocation, business.Location);
        Assert.Equal(dto.NewName, result.Value.Name);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.UpdateBusinessInfoAsync(
            Guid.NewGuid(), new UpdateBusinessInfoDto("New name", "New description", "New location"));

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveBusinessAsync_WhenBusinessExists_ArchivesAndSaves()
    {
        var business = BusinessAggregate.Create("Name", Guid.NewGuid(), "Description", "Location");

        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ArchiveBusinessAsync(business.Id.Value);

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
            .Setup(r => r.GetBusinessByIdAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.ArchiveBusinessAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
