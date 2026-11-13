namespace Business.Application.DTOs.Category;

public record CategoryInfoDTO(
    Guid CategoryId,
    string Name,
    string Slug
    );