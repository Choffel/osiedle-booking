using Business.Domain.ValueObjects;

namespace Business.Application.DTOs.BusinessDTOs;

public record UpdateBusinessInfoDto(
    string NewName,
    string NewDescription,
    Address NewLocation
    );
