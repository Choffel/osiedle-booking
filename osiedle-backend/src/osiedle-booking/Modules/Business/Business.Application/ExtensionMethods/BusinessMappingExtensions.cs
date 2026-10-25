using Business.Application.DTOs;

namespace Business.Application.ExtensionMethods;

public static class BusinessMappingExtensions
{
    public static BusinessResponseDto ToResponseDto(this Domain.Aggregates.Business business)
    {
        return new BusinessResponseDto(
            business.Id.Value,
            business.OwnerId.Value,
            business.Name,
            business.Description,
            business.Location,
            business.Status.ToString(),
            business.CreatedAt,
            business.UpdatedAt
        );
    }
}