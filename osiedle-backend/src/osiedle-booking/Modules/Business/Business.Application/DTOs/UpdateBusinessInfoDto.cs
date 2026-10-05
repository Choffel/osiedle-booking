namespace Business.Application.DTOs;

public record UpdateBusinessInfoDto(
    Guid BusinessId,
    string NewName,
    string NewDescription
    );