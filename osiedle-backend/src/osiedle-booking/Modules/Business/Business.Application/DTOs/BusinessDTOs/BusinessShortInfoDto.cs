namespace Business.Application.DTOs.BusinessDTOs;

public record BusinessShortInfoDto(
    Guid BusinessId,      
    Guid OwnerId,
    string Name,        
    string PhotoUrl,
    int Raiting,
    string Description,     
    LocationDto Location
);
