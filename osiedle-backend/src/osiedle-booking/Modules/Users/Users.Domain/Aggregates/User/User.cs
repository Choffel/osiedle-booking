using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User.ValueObjects;

namespace Users.Domain.Entity.User;

public class User : AggregateRoot<UserId>
{
    public Name Name { get; private set; }
    public string Email { get; private set; }
    
    private readonly List<RefreshToken.RefreshToken> _refreshTokens = new();
    public IReadOnlyList<RefreshToken.RefreshToken> RefreshTokens => _refreshTokens;

    private User() { }

    private User(UserId id, Name name, string email)
        : base(id)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(UserId id, Name name, string email)
    {
        var user = new User(
            id,
            name,
            email
        );
        
        return user;
    }
    
    public void UpdateName(Name name) // TODO 
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(UserId userId)
    {
        if (userId != Id)
            throw new ArgumentException("You cannot delete an account that does not belong to you.", nameof(userId));

        DeletedAt = DateTime.UtcNow;
    }
}