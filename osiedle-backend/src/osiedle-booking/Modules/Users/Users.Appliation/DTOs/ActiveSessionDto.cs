namespace Users.Appliation.DTOs;

public record ActiveSessionDto(
    Guid TokenId,
    DateOnly CreatedAt,
    bool IsCurrentSession
);