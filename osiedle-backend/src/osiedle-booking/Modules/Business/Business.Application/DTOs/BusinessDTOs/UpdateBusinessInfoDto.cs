namespace Business.Application.DTOs;

public record UpdateBusinessInfoDto(
    string NewName,
    string NewDescription,
    string NewLocation
    );
