using Business.Application.DTOs;
using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.Enums;

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
            new LocationDto(business.Location.City, business.Location.Street),
            business.Services
                .Select(s => new ServiceShortInfoDto(
                    s.Id.Value,
                    s.EmployeeId.Value,
                    s.Category,
                    s.ServiceName,
                    s.Price,
                    s.Duration
                ))
                .ToList(),
            new List<EmployeeShortInfoDto>(),
            business.Status.ToString(),
            business.Category.ToSlug(),
            business.CreatedAt,
            business.UpdatedAt
        );
    }
}
