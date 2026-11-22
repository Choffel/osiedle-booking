using Business.Application.DTOs;
using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Services.BusinessServices;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Application.Tests.Services;

public class BusinessServiceTests
{
    private static readonly Location ValidLocation = Location.Create("Warsaw", "Main Street 1");
    private static readonly LocationDto ValidLocationDto = new("Warsaw", "Main Street 1");

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
        var dto = new CreateBusinessDto("Beauty Salon", Guid.NewGuid(), "Haircuts and more", ValidLocationDto, Category.Haircut.ToSlug());
        BusinessAggregate? added = null;

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(UserId.From(dto.OwnerId), It.IsAny<CancellationToken>()))
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
        Assert.Equal(ValidLocation, added.Location);
        Assert.Equal(BusinessStatus.Active, added.Status);
        Assert.Equal(added.Id.Value, result.Value.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBusinessAsync_WhenOwnerAlreadyHasBusiness_ReturnsConflict()
    {
        var dto = new CreateBusinessDto("Beauty Salon", Guid.NewGuid(), "Description", ValidLocationDto, Category.Haircut.ToSlug());

        _businessRepository
            .Setup(r => r.ExistsByOwnerIdAsync(UserId.From(dto.OwnerId), It.IsAny<CancellationToken>()))
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
        var userId = UserId.New();
        var business = BusinessAggregate.Create("Beauty Salon", userId, "Description", ValidLocation, Category.Haircut);
        var businessResponse = new BusinessResponseDto(
            business.Id.Value,
            business.OwnerId.Value,
            business.Name,
            business.Description,
            ValidLocationDto,
            new List<ServiceShortInfoDto>(),
            new List<EmployeeShortInfoDto>(),
            business.Status.ToString(),
            Category.Haircut.ToSlug(),
            business.CreatedAt,
            business.UpdatedAt);

        _businessRepository
            .Setup(r => r.GetBusinessByIdForReadAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessResponse);

        var result = await _sut.GetBusinessByIdAsync(business.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(businessResponse.Id, result.Value.Id);
        Assert.Equal(businessResponse.Name, result.Value.Name);
        Assert.Equal(businessResponse.Status, result.Value.Status);
        Assert.Equal(ValidLocationDto, result.Value.Location);
    }

    [Fact]
    public async Task GetBusinessByIdAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        var businessId = BusinessId.New();
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.GetBusinessByIdAsync(businessId);

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetBusinessesAsync_PassesCityFilterToRepository()
    {
        var pagedResult = new PagedResult<BusinessShortInfoDto>([], 0, 1, 10);
        _businessRepository
            .Setup(r => r.GetBusinessesAsync(1, 10, "Warsaw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _sut.GetBusinessesAsync(1, 10, "Warsaw");

        Assert.True(result.IsSuccess);
        Assert.Same(pagedResult, result.Value);
        _businessRepository.Verify(r => r.GetBusinessesAsync(1, 10, "Warsaw", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessExists_UpdatesFieldsAndSaves()
    {
        var userId = UserId.New();
        var business = BusinessAggregate.Create("Old name", userId, "Old description", ValidLocation, Category.Haircut);
        var newLocationDto = new LocationDto("Krakow", "New Street 2");
        var dto = new UpdateBusinessInfoDto(
            "New name", "New description", newLocationDto, Category.BeardTrim.ToSlug());

        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateBusinessInfoAsync(userId, business.Id, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.NewName, business.Name);
        Assert.Equal(dto.NewDescription, business.Description);
        Assert.Equal("Krakow", business.Location.City);
        Assert.Equal("New Street 2", business.Location.Street);
        Assert.Equal(Category.BeardTrim, business.Category);
        Assert.Equal(dto.NewName, result.Value.Name);
        Assert.Equal(newLocationDto, result.Value.Location);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBusinessInfoAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        var userId = UserId.New();
        var businessId = BusinessId.New();
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.UpdateBusinessInfoAsync(
            userId, businessId,
            new UpdateBusinessInfoDto("New name", "New description", ValidLocationDto, Category.BeardTrim.ToSlug()));

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveBusinessAsync_WhenBusinessExists_ArchivesAndSaves()
    {
        var userId = UserId.New();
        var business = BusinessAggregate.Create("Name", userId, "Description", ValidLocation, Category.Haircut);

        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ArchiveBusinessAsync(userId, business.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(BusinessStatus.Archived, business.Status);
        Assert.NotNull(business.DeletedAt);
        Assert.Equal(nameof(BusinessStatus.Archived), result.Value.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveBusinessAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        var userId = UserId.New();
        var businessId = BusinessId.New();
        _businessRepository
            .Setup(r => r.GetBusinessByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessAggregate?)null);

        var result = await _sut.ArchiveBusinessAsync(userId, businessId);

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
