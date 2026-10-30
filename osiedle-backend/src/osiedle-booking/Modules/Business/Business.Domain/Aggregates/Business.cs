using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Domain.Aggregates;

public class Business : AggregateRoot<BusinessId> 
{
  public string Name { get; private set; }
    public UserId OwnerId { get; private set; } 
    public string? Description { get; private set; }
    public Address Location { get; private set; } 
    public BusinessStatus Status { get; private set; }

    private readonly List<BookableService> _services = new();
    public IReadOnlyCollection<BookableService> Services => _services.AsReadOnly();

    private readonly List<EmployeeId> _employeeIds = new();
    public IReadOnlyCollection<EmployeeId> EmployeeIds => _employeeIds.AsReadOnly();

    private Business() { } 

    private Business(
        BusinessId id, 
        string name, 
        Address location, 
        UserId ownerId, 
        string? description) : base(id)
    {
        if (ownerId.Value == Guid.Empty)
            throw new ArgumentException("Business owner id cannot be empty.", nameof(ownerId));

        if (location.IsEmpty)
            throw new ArgumentException("Business location cannot be uninitialized.", nameof(location));

        Name = EnsureValidName(name);
        OwnerId = ownerId;
        Location = location;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = BusinessStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Business Create(string name, UserId ownerId, Address location, string? description = null)
    {
        return new Business(BusinessId.New(), name, location, ownerId, description);
    }
    

    public void AddEmployee(EmployeeId employeeId)
    {
        EnsureIsNotArchived();

        if (_employeeIds.Contains(employeeId))
            return;

        _employeeIds.Add(employeeId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddService(
        EmployeeId employeeId, 
        string name, 
        string description, 
        decimal price, 
        TimeSpan durationMinutes)
    {
        EnsureIsNotArchived();
        
        if (!_employeeIds.Contains(employeeId))
        {
            throw new InvalidOperationException($"Employee '{employeeId.Value}' is not associated with this business.");
        }
        
        var trimmedName = EnsureValidName(name);
        if (_services.Any(s => s.ServiceName.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Service with name '{trimmedName}' already exists in this business.");
        }

        var service = BookableService.Create(employeeId, trimmedName, description, price, durationMinutes);
        
        _services.Add(service);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string newName, Address newLocation, string? newDescription = null)
    {
        EnsureIsNotArchived();

        if (newLocation.IsEmpty)
            throw new ArgumentException("New business location cannot be uninitialized.", nameof(newLocation));

        Name = EnsureValidName(newName);
        Location = newLocation;
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveBusiness()
    {
        EnsureIsNotArchived();

        Status = BusinessStatus.Archived;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureIsNotArchived()
    {
        if (Status == BusinessStatus.Archived)
            throw new InvalidOperationException("Cannot perform operation on an archived business.");
    }

    private static string EnsureValidName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new ArgumentException("Business name must be between 2 and 100 characters.", nameof(name));

        return trimmed;
    }
}