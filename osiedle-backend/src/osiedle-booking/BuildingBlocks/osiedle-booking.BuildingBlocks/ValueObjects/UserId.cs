namespace Users.Domain.Entity.User.Value_objects;

public sealed record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
}