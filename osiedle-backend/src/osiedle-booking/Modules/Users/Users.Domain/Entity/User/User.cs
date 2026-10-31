using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User.ValueObjects;

namespace Users.Domain.Entity.User;

public class User : AggregateRoot<UserId>
{
    public Name Name { get; } //TODO вынести имя в VO
    
    public string Email { get; private set; }

    private User(UserId id, string firstName, string lastName, string email)
    {
        Id = id;
        Name.Create(firstName, lastName);
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }
    private User() { }

    public static User Create(UserId id, string firstName, string lastName, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        var user = new User(
            id,
            firstName,
            lastName,
            email
        );
        
        return user;
    }
    
    public void Update(string firstName, string lastName)
    {
        Name.Update(firstName, lastName);
        UpdatedAt = DateTime.UtcNow;
    }
}