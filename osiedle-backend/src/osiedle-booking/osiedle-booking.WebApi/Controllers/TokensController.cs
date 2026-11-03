using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using osiedle_booking.WebApi.Extensions;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserSessionService _userSessionService;

    public TokensController(ITokenService tokenService, IUserSessionService userSessionService)
    {
        _tokenService = tokenService;
        _userSessionService = userSessionService;
    }

    [HttpPost("RevokeRefreshToken")]
    [Authorize]
    public async Task<IActionResult> RevokeRefreshToken(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var refreshToken = Request.Headers["X-Refresh-Token"].ToString();
        
        var result = await _userSessionService.RevokeCurrentSessionAsync(UserId.From(userId), refreshToken, ct);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPost("RevokeSession")]
    public async Task<IActionResult> RevokeSession([FromBody]Guid tokenId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var refreshToken = Request.Headers["X-Refresh-Token"].ToString();
        
        var result = await _userSessionService.RevokeSessionAsync(
            UserId.From(userId), RefreshTokenId.From(tokenId), refreshToken, ct);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("NewAccessToken")]
    public async Task<ActionResult<TokenResponseDto>> GetNewAccessToken(CancellationToken cancellationToken)
    {
        var currentRefreshToken = Request.Headers["X-Refresh-Token"].ToString();
        
        var result = await _tokenService.GenerateTokensByRefreshTokenAsync(currentRefreshToken, cancellationToken);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("ActiveSessions")]
    public async Task<ActionResult<IReadOnlyCollection<ActiveSessionDto>>> GetActiveSessionsAsync(CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var refreshToken = Request.Headers["X-Refresh-Token"].ToString();
        
        var result = await _userSessionService.GetActiveSessionsAsync(UserId.From(userId), refreshToken, ct);
        return result.ToActionResult();
    }
}