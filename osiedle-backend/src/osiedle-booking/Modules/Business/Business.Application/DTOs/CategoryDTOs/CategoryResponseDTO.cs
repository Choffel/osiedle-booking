namespace Business.Application.DTOs.CategoryDTOs;


public record CategoryResponseDTO(
    int Id,
    string DisplayName,
    string Slug
);
