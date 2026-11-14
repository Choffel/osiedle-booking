using Business.Application.DTOs.Category;
using Business.Application.Interfaces.Category.Services;
using Business.Domain.ValueObjects.Category;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("new")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateAsync(CategoryCreateRequestDTO request, CancellationToken ct)
    {
        var result = await _categoryService.CreateAsync(request, ct);
        return result.ToActionResult();
    }
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct)
    {
        var result = await _categoryService.GetCategoriesAsync(ct);
        return result.ToActionResult();
    }
    
    [HttpPost("archive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ArchiveBySlug(string slug, CancellationToken ct)
    {
        var result = await _categoryService.ArchiveBySlug(Slug.From(slug), ct);
        return result.ToActionResult();
    }
}