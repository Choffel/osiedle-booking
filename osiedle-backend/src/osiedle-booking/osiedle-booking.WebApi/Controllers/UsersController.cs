using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks.ValueObjects;
using osiedle_booking.WebApi.Extensions;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [Authorize]
    [HttpPut("Update")]
    public async Task<ActionResult> UpdateAsync([FromBody] UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var email = User.FindFirstValue(ClaimTypes.Email);
        
        var result = await _userService.UpdateUserAsync(UserId.From(userId), email, request, cancellationToken);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var result = await _userService.GetUserByIdAsync(UserId.From(userId), ct);
        return result.ToActionResult();
    }
}