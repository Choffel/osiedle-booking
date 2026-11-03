namespace Business.Application.DTOs;

public record BusinessResponseDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    string Location,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
    );
    
    