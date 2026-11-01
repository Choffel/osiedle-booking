namespace Business.Application.DTOs.BusinessDTOs;

public record UpdateBusinessInfoDto(
    string NewName,
    string NewDescription,
    AddressDto NewLocation
    );
