namespace Business.Application.DTOs.BusinessDTOs;

public record CreateBusinessDto(
    string Name,
    Guid OwnerId,
    string Description,
    LocationDto Location,
    string Category
);
