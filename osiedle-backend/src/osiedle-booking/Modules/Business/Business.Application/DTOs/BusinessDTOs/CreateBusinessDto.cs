namespace Business.Application.DTOs.BusinessDTOs;

public record CreateBusinessDto(
    string Name,
    Guid OwnerId,
    string Description,
    string Street,
    string Number,
    string City
    );
