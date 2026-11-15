using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Domain.Aggregates;


public class Employee : AggregateRoot<EmployeeId>
{
    public string FirstName { get; private set; }
    
    public string LastName { get; private set; }
    
    public string Position { get; private set; }
    
    public string? Review { get; private set; }
    
    public string Email { get; private set; }

    public BusinessId? BusinessId { get; private set; } = null;
    
    public EmploymentStatus Status { get; private set; } = EmploymentStatus.Active;
    
    private readonly List<BookableService> _bookableServices = new();
    public IReadOnlyCollection<BookableService> BookableServices => _bookableServices;
    
    private Employee(){}
    
    
    private Employee(
        EmployeeId id, BusinessId? businessId,
        string firstName, 
        string lastName, 
        string position, 
        string? review,
        string email
        ) : base(id)
    {
        BusinessId = businessId;
        ValidateAndAssign(firstName, lastName, position, review, email);
    }
    
    public void UpdateInfo(string firstName, string lastName, string position, string? review, string email)
    {
        ValidateAndAssign(firstName, lastName, position, review, email);
        UpdatedAt = DateTime.UtcNow; 
    }
    
    public void ChangeStatus(EmploymentStatus newStatus)
    {
        if (Status == newStatus)
            return;
        
        if (Status == EmploymentStatus.Terminated)
        {
            throw new InvalidOperationException("employee is terminated.");
        }

        Status = newStatus;
    }
    
    private void ValidateAndAssign(string firstName, string lastName, string position, string? review, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(position);

        FirstName = firstName;
        LastName = lastName;
        Position = position;
        Review = string.IsNullOrWhiteSpace(review) ? null : review;
        Email = NormalizeEmail(email);
    }

    public static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToLowerInvariant();
    }
    
    public static Employee Create(BusinessId? businessId, string firstName, string lastName, string position, string? review, string email)
    {
        var employeeId = EmployeeId.New();
        return new Employee(employeeId, businessId, firstName, lastName, position, review, email);
    }

}