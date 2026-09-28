using System.Security.AccessControl;
using Users.Domain.Entity.User.Value_objects;
using osiedle_booking.BuildingBlocks.Domain;

namespace Users.Domain.Entity.User;

public class User : AggregateRoot<UserId>
{
    public string DisplayName { get; private set; }

    private User(UserId id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public static User Create(UserId id, string displayName)
    {
        var user = new User(id, displayName);
        return user;
    }
    
    public void Update(string displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}