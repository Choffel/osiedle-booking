using Business.Application.DTOs;
using Business.Domain.Enums;

namespace Business.Application.ExtensionMethods;

//TODO: Add services and employee 
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
            business.Category.ToSlug(),
            business.CreatedAt,
            business.UpdatedAt
        );
    }
}