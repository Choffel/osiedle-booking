using Business.Domain.Enums;
using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class Employee : Entity<EmployeeId>
{
    public string FirstName { get; private set; }
    
    public string LastName { get; private set; }
    
    public string Position { get; private set; }
    
    public string? Review { get; private set; }
    
    public BusinessId BusinessId { get; private set; }
    
    private Employee(){}
    
    
    private Employee(
        EmployeeId id, BusinessId businessId,
        string firstName, 
        string lastName, 
        string position, 
        string review) : base(id)
    {
        BusinessId = businessId;
        ValidateAndAssign(firstName, lastName, position, review);
    }
    
    public void Update(string firstName, string lastName, string position, string review)
    {
        ValidateAndAssign(firstName, lastName, position, review);
        UpdatedAt = DateTime.UtcNow; 
    }

    private void ValidateAndAssign(string firstName, string lastName, string position, string review)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position cannot be empty.", nameof(position));

        if (string.IsNullOrWhiteSpace(review))
            throw new ArgumentException("Review cannot be empty.", nameof(review));

        FirstName = firstName;
        LastName = lastName;
        Position = position;
        Review = review;
    }
    
    public static Employee Create(EmployeeId id, BusinessId businessId, string firstName, string lastName, string position, string review)
    {
        var employeeId = EmployeeId.New();
        return new Employee()
        {
            Id = employeeId,
            FirstName = firstName,
            LastName = lastName,
            Position = position,
            BusinessId = businessId,
            Review = review
        };
    }
    
}