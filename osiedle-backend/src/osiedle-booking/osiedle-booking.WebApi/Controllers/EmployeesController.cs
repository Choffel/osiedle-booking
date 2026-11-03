using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        return (await _employeeManagementService.CreateEmployeeAsync(dto, ct)).ToActionResult();
    }

    [HttpPut("{employeeId:guid}/profile")]
    public async Task<ActionResult> UpdateProfileAsync(
        Guid employeeId,
        [FromBody] UpdateEmployeeProfileDto dto,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        
        return (await _employeeManagementService.UpdateProfileAsync(employee, dto, ct))
            .ToActionResult();
    }

    [HttpPost("{employeeId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateEmployeeAsync(
        Guid employeeId,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        
        return (await _employeeManagementService.DeactivateEmployeeAsync(employee, ct))
            .ToActionResult();
    }

    [HttpPut("{employeeId:guid}/working-hours")]
    public async Task<ActionResult> SetWorkingHoursAsync(
        Guid employeeId,
        [FromBody] SetWorkingHoursDto dto,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        
        return (await _employeeWorkingHoursService.SetWorkingHoursAsync(employee, dto, ct)).ToActionResult();
    }

    [HttpDelete("{employeeId:guid}/working-hours")]
    public async Task<ActionResult> ClearWorkingHoursAsync(
        Guid employeeId,
        [FromQuery] DateOnly date,
        CancellationToken ct = default)
    {
        var employee = EmployeeId.From(employeeId);
        
        return (await _employeeWorkingHoursService.ClearWorkingHoursAsync(employee, date, ct)).ToActionResult();
    }
}
