using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;

namespace Users.Application.Abstractions.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(UserId userId, string email, IReadOnlyCollection<string> roles);
    string GenerateRefreshToken();
}