namespace Business.Application.DTOs.Category;

public record CategoryResponseDTO(
    string Name,
    string Slug,
    int DisplayOrder
    );