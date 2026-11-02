namespace Business.Application.DTOs.BusinessDTOs;

public record UpdateBusinessInfoDto(
    string NewName,
    string NewDescription,
    string NewStreet,
    string NewNumber,
    string NewCity
    );
