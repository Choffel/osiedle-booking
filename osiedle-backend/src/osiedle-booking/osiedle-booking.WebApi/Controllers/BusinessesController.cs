using Business.Application.DTOs;
using Business.Application.DTOs.BusinessDTOs;
using Business.Application.Interfaces.Business;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks.ValueObjects;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        return (await _businessManagementService.CreateBusinessAsync(dto, ct)).ToActionResult();
    }

    [HttpGet("{businessId:guid}")]
    public async Task<ActionResult<BusinessResponseDto>> GetBusinessByIdAsync(
        Guid businessId,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        
        return (await _businessManagementService.GetBusinessByIdAsync(business, ct)).ToActionResult();
    }

    [HttpPut("{businessId:guid}")]
    public async Task<ActionResult<BusinessResponseDto>> UpdateBusinessInfoAsync(
        Guid businessId,
        [FromBody] UpdateBusinessInfoDto dto,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        
        return (await _businessManagementService.UpdateBusinessInfoAsync(business, dto, ct)).ToActionResult();
    }

    [HttpPost("{businessId:guid}/archive")]
    public async Task<ActionResult<BusinessResponseDto>> ArchiveBusinessAsync(
        Guid businessId,
        CancellationToken ct = default)
    {
        
        var business = BusinessId.From(businessId);
        
        return (await _businessManagementService.ArchiveBusinessAsync(business, ct)).ToActionResult();
    }
}
