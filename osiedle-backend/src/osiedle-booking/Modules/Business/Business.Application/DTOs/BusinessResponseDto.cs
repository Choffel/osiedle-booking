namespace Business.Application.DTOs;

public record BusinessResponseDto(
    Guid Id,
    Guid OwnerId,
    string Description,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
    );