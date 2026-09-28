using osiedle_booking.BuildingBlocks.Domain.Entity;
using Users.Domain.Entity.User.Value_objects;

namespace Users.Domain.Entity.RefreshToken;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public DateOnly CreatedAt { get; set; }
    public DateOnly UpdatedAt { get; set; }
    public DateOnly DeletedAt { get; set; }
}