namespace Users.Application.DTOs;

public record ActiveSessionDto(
    Guid TokenId,
    DateTime CreatedAt,
    bool IsCurrentSession
);