using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("RevokeRefreshToken")]
    [Authorize]
    public async Task<Result<bool>> RevokeRefreshToken(CancellationToken cancellationToken)
    {
        var result = await _tokenService.RevokeRefreshTokenAsync(cancellationToken);
        return result;
    }

    [Authorize]
    [HttpPost("RevokeSession")]
    public async Task<Result<string>> RevokeSession([FromBody]Guid tokenId, CancellationToken ct = default)
    {
        var result = await _tokenService.RevokeSessionAsync(tokenId, ct);
        return result;
    }

    [AllowAnonymous]
    [HttpPost("NewAccessToken")]
    public async Task<Result<TokenResponseDto>> GetNewAccessToken(CancellationToken cancellationToken)
    {
        var result = await _tokenService.GetNewAccessTokenAsync(cancellationToken);
        return result;
    }

    [Authorize]
    [HttpGet("ActiveSessions")]
    public async Task<Result<IEnumerable<ActiveSessionDto>>> GetActiveSessionsAsync(CancellationToken ct = default)
    {
        var result = await _tokenService.GetActiveSessionsAsync(ct);
        return result;
    }
}