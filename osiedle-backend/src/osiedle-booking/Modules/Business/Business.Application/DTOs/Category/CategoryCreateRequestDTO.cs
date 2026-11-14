namespace Business.Application.DTOs.Category;

public record CategoryCreateRequestDTO(
    string Name,
    string Slug,
    int DisplayOrder
    );