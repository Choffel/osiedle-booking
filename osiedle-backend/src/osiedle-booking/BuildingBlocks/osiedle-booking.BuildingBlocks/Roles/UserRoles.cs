namespace osiedle_booking.BuildingBlocks.Roles;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly IReadOnlyCollection<string> All = new[] 
    { 
        Admin, 
        User 
    };
}