using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Domain.Entity.RefreshToken;

public class RefreshToken : AggregateRoot<RefreshTokenId>
{
    public string Token { get; private set; } = null!;
    public UserId UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive(DateTime utcNow) => RevokedAt == null && utcNow < ExpiresAt;

    private RefreshToken() { }

    private RefreshToken(RefreshTokenId id, string token, UserId userId, DateTime expiresAt)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(
        RefreshTokenId id, 
        string token, 
        UserId userId, 
        DateTime expiresAt,
        DateTime utcNow) 
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));
            
        if (expiresAt <= utcNow)
            throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

        return new RefreshToken(id, token, userId, expiresAt);
    }

    public void Revoke(UserId userId, DateTime utcNow)
    {
        if (!IsOwnedBy(userId))
            throw new ArgumentException("Token is not owned by the specified user.", nameof(userId));

        if (RevokedAt != null)
            return;

        RevokedAt = utcNow;
    }
    
    public bool IsOwnedBy(UserId userId) => UserId == userId;
}