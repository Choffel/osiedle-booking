using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain.Aggregates;

public class Employee : AggregateRoot<EmployeeId>
{
    public EmployeeName EmployeeName { get; private set; }
    public string Position { get; private set; }
    public string? Review { get; private set; }
    public string Email { get; private set; }

    public BusinessId? BusinessId { get; private set; }
    public EmploymentStatus Status { get; private set; } = EmploymentStatus.Active;

    private readonly List<BookableService> _bookableServices = new();
    public IReadOnlyCollection<BookableService> BookableServices => _bookableServices.AsReadOnly();

    private Employee() { } 

    private Employee(
        EmployeeId id, 
        BusinessId? businessId,
        EmployeeName employeeName,
        string position, 
        string? review,
        string email) : base(id)
    {
        if (employeeName.IsEmpty)
            throw new ArgumentException("Employee name cannot be uninitialized.", nameof(employeeName));

        BusinessId = businessId;
        EmployeeName = employeeName; 
        
        Position = EnsureValidPosition(position);
        Review = string.IsNullOrWhiteSpace(review) ? null : review.Trim();
        Email = NormalizeEmail(email);
        CreatedAt = DateTime.UtcNow;
    }

    public static Employee Create(
        BusinessId? businessId, 
        EmployeeName employeeName, 
        string position, 
        string? review, 
        string email)
    {
        return new Employee(EmployeeId.New(), businessId, employeeName, position, review, email);
    }
    
    public void UpdateEmployeeName(EmployeeName newName)
    {
        EnsureIsNotArchived();

        if (newName.IsEmpty)
            throw new ArgumentException("New name cannot be uninitialized.", nameof(newName));

        EmployeeName = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(string newEmail)
    {
        EnsureIsNotArchived();
        Email = NormalizeEmail(newEmail);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePosition(string newPosition)
    {
        EnsureIsNotArchived();

        Position = EnsureValidPosition(newPosition);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(EmploymentStatus newStatus)
    {
        EnsureIsNotArchived();

        if (Status == newStatus)
            return;

        if (Status == EmploymentStatus.Terminated && newStatus != EmploymentStatus.Archived)
        {
            throw new InvalidOperationException("Cannot change status of a terminated employee except archiving.");
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveEmployee()
    {
        EnsureIsNotArchived();

        Status = EmploymentStatus.Archived;
        DeletedAt = DateTime.UtcNow; 
        UpdatedAt = DateTime.UtcNow;
    }
    
    private void EnsureIsNotArchived()
    {
        if (Status == EmploymentStatus.Archived)
            throw new InvalidOperationException("Employee is archived and cannot be modified.");
    }

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var trimmed = email.Trim().ToLowerInvariant();

        if (!trimmed.Contains('@'))
            throw new ArgumentException("Invalid email format.", nameof(email));

        return trimmed;
    }

    private static string EnsureValidPosition(string position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(position);
        var trimmed = position.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new ArgumentException("Position must be between 2 and 100 characters.", nameof(position));

        return trimmed;
    }
}