using System.Security.Claims;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks.ValueObjects;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeManagementService _employeeManagementService;
    private readonly IEmployeeWorkingHoursService _employeeWorkingHoursService;

    public EmployeesController(
        IEmployeeManagementService employeeManagementService,
        IEmployeeWorkingHoursService employeeWorkingHoursService)
    {
        _employeeManagementService = employeeManagementService;
        _employeeWorkingHoursService = employeeWorkingHoursService;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeId>> CreateEmployeeAsync(
        [FromBody] EmployeeCreateDto dto,
        CancellationToken ct = default)
    {
        var userId = UserId.From(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        return (await _employeeManagementService.CreateEmployeeAsync(userId, dto, ct)).ToActionResult();
    }
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeResponseDto>>> GetAllEmployeesByBusinessIdAsync(
        Guid businessId,
        CancellationToken ct = default)
    {
        var business = BusinessId.From(businessId);
        return (await _employeeManagementService.GetAllEmployeesByBusinessIdAsync(business, ct)).ToActionResult();
    }

    [HttpPut("{employeeId:guid}/profile")]
    public async Task<ActionResult> UpdateProfileAsync(
        Guid employeeId,
        [FromBody] UpdateEmployeeProfileDto dto,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        var userId = UserId.From(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        return (await _employeeManagementService.UpdateProfileAsync(userId, employee, dto, ct))
            .ToActionResult();
    }

    [HttpPost("{employeeId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateEmployeeAsync(
        Guid employeeId,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        var userId = UserId.From(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        return (await _employeeManagementService.DeactivateEmployeeAsync(userId, employee, ct))
            .ToActionResult();
    }

    [HttpPut("{employeeId:guid}/working-hours")]
    public async Task<ActionResult> SetWorkingHoursAsync(
        Guid employeeId,
        [FromBody] SetWorkingHoursDto dto,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        var userId = UserId.From(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        return (await _employeeWorkingHoursService.SetWorkingHoursAsync(userId, employee, dto, ct)).ToActionResult();
    }

    [HttpDelete("{employeeId:guid}/working-hours")]
    public async Task<ActionResult> ClearWorkingHoursAsync(
        Guid employeeId,
        [FromQuery] DateOnly date,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        var userId = UserId.From(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

        return (await _employeeWorkingHoursService.ClearWorkingHoursAsync(userId, employee, date, ct)).ToActionResult();
    }
}
