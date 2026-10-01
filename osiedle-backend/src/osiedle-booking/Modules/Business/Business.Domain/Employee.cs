using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain.Entity;
using Users.Domain.Entity.User.Value_objects;

namespace Business.Domain;

public class Employee : Entity<EmployeeId>
{
    public BusinessId BusinessId { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string FirstName { get; private set; }
    
    public string LastName { get; private set; }
    
    public string Position { get; private set; }
    
    public string? Review { get; private set; }
}