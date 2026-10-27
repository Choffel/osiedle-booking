namespace Users.Application.DTOs;

public record ActiveSessionDto(
    Guid TokenId,
    DateOnly CreatedAt,
    bool IsCurrentSession
);