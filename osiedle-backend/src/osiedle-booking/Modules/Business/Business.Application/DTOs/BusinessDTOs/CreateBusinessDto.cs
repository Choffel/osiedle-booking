using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.DTOs.BusinessDTOs;

public record CreateBusinessDto(
    string Name,
    UserId OwnerId,
    string Description,
    Address Location
    );