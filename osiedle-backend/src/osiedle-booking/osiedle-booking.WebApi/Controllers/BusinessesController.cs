using System.Security.Claims;
using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces.Business;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessManagementService _businessManagementService;

    public BusinessesController(IBusinessManagementService businessManagementService)
    {
        _businessManagementService = businessManagementService;
    }

    [HttpPost]
    public async Task<ActionResult<BusinessResponseDto>> CreateBusinessAsync(
        [FromBody] CreateBusinessDto dto,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await _businessManagementService.CreateBusinessAsync(dto with { OwnerId = userId }, ct)).ToActionResult();
    }

    [HttpGet("{businessId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessResponseDto>> GetBusinessByIdAsync(
        [FromRoute] Guid businessId,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        return (await _businessManagementService.GetBusinessByIdAsync(business, ct)).ToActionResult();
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<BusinessShortInfoDto>>> GetBusinesses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 3,
        [FromQuery] string? city = null,
        CancellationToken cancellationToken = default)
    {
        return (await _businessManagementService.GetBusinessesAsync(page, pageSize, city, cancellationToken))
            .ToActionResult();
    }

    [HttpPut("{businessId:guid}")]
    public async Task<ActionResult<BusinessResponseDto>> UpdateBusinessInfoAsync(
        Guid businessId,
        [FromBody] UpdateBusinessInfoDto dto,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = UserId.From(userId);
        
        return (await _businessManagementService.UpdateBusinessInfoAsync(user, business, dto, ct)).ToActionResult();
    }

    [HttpPost("{businessId:guid}/archive")]
    public async Task<ActionResult<BusinessResponseDto>> ArchiveBusinessAsync(
        [FromRoute] Guid businessId,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = UserId.From(userId);
        
        return (await _businessManagementService.ArchiveBusinessAsync(user, business, ct)).ToActionResult();
    }
}
