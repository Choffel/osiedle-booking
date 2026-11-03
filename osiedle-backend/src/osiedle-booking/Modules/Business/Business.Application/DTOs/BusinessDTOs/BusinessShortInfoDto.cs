namespace Business.Application.DTOs.BusinessDTOs;

public record BusinessShortInfoDto(
    Guid BusinessId,      
    Guid OwnerId,
    string Name,             
    string Description,     
    string Location
    );