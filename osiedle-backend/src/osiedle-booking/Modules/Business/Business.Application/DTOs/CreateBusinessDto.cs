namespace Business.Application.DTOs;

public record CreateBusinessDto(
    string Name,
    Guid OwnerId,
    string Description,
    string Location
    );