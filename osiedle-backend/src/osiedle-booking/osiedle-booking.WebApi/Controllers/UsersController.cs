using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.User.Value_objects;

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
    public async Task<Result<string>> UpdateAsync([FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateUserAsync(request, cancellationToken);
        return result;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync()
    {
        var result = await _userService.GetCurrentUserAsync();
        return result;
    }
}