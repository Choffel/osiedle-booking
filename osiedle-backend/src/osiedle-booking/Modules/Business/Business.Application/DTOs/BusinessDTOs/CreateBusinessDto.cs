using Business.Domain.Enums;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.DTOs.BusinessDTOs;

public record CreateBusinessDto(
    string Name,
    Guid OwnerId,
    string Description,
    string Location,
    Category Category
    );