using Business.Application.DTOs.BusinessDTOs;

namespace Business.Application.DTOs;

public record BusinessResponseDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    LocationDto Location,
    string Status,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
