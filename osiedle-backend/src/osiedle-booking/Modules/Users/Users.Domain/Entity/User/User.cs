using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User.ValueObjects;

namespace Users.Domain.Entity.User;

public class User : AggregateRoot<UserId>
{
    public Name Name { get; private set; }
    public string Email { get; private set; }

    private User(UserId id, Name name, string email)
        : base(id)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        if(name is null)
            throw new ArgumentNullException(nameof(name));
        
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }
    private User() { }

    public static User Create(UserId id, Name name, string email)
    {
        var user = new User(
            id,
            name,
            email
        );
        
        return user;
    }
    
    public void Update(Name name)
    {
        if(name is null)
            throw new ArgumentNullException(nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}