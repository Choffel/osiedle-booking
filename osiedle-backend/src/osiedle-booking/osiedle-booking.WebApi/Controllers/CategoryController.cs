using Business.Application.DTOs.CategoryDTOs;
using Business.Application.DTOs;
using Business.Application.Interfaces.Categories.Services;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct)
    {
        var result = await _categoryService.GetCategoriesAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("businesses/{categorySlug}")]
    public async Task<ActionResult<IReadOnlyList<BusinessResponseDto>>> GetBusinessesByCategoryAsync(
        [FromRoute] string categorySlug,
        CancellationToken ct)
    {
        var result = await _categoryService.GetAllBusinessesByCategoryAsync(categorySlug, ct);
        return result.ToActionResult();
    }
}
