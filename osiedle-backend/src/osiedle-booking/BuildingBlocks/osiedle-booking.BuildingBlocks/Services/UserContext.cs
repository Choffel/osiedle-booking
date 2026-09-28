using System.Security.Claims;
using osiedle_booking.BuildingBlocks.Interfaces;
using Microsoft.AspNetCore.Http;

namespace osiedle_booking.BuildingBlocks.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private Guid? _userId;
    private List<string>? _roles;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    
    public Guid UserId => _userId ??= GetUserId();
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string RefreshToken => _httpContextAccessor.HttpContext?.Request.Headers["X-Refresh-Token"].ToString() ?? string.Empty;

    public IReadOnlyCollection<string> Roles => _roles ??= User?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList() ?? [];

    private Guid GetUserId()
    {
        var id = User?.FindFirstValue("userId");

        return Guid.TryParse(id, out var parsed) ? parsed : Guid.Empty;
    }
}