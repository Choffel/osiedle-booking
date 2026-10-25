using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Domain.Aggregates;

public class Business : AggregateRoot<BusinessId> 
{
    public string Name { get; private set; } = null!;
    public UserId OwnerId { get; private set; } 
    
    public string Description { get; private set; }
    public string Location { get; private set; } 
    
    private readonly List<BookableService> _services = new();
    public IReadOnlyList<BookableService> Services => _services;
    
    private readonly List<EmployeeId> _employeeIds = new();
    public IReadOnlyList<EmployeeId> EmployeeIds => _employeeIds.AsReadOnly();
   
    
    
    public BusinessStatus Status { get; private set; }
    
    private Business() { }

    private Business(BusinessId id, string name, string location, UserId ownerId, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));
        
        if(string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Business location cannot be empty.", nameof(location));

        if(string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Business description cannot be empty.", nameof(description));

        Name = name;
        OwnerId = ownerId;
        Description = description;
        Location = location;
        Status = BusinessStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static Business Create(string name, UserId ownerId, string description, string location)
    {
        var businessId = BusinessId.New();
        return new Business(businessId, name, location, ownerId, description);
    }

    public void AddService(EmployeeId employeeId, string name, string description, decimal price, TimeSpan durationMinutes)
    {
        if (Status == BusinessStatus.Archived)
            throw new InvalidOperationException("Cannot modify an archived business.");

        if (_services.Any(s => s.ServiceName == name))
            throw new InvalidOperationException("Service with this name already exists.");
        
        //TODO: fix ANY method to check if employee is associated with this business
        if(!_services.Any(s => s.EmployeeId == employeeId))
            throw new InvalidOperationException("Employee is not associated with this business.");

        var service = BookableService.Create(employeeId, name, description, price, durationMinutes);
        
        _services.Add(service);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddEmployee(EmployeeId employeeId)
    {
        if (_employeeIds.Contains(employeeId))
            return;

        _employeeIds.Add(employeeId);
    }
    
    public void UpdateInfo(string newName, string newDescription, string newLocation)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New business name cannot be empty.", nameof(newName));
        
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("New business description cannot be empty.", nameof(newDescription));
        
        if (string.IsNullOrWhiteSpace(newLocation))
            throw new ArgumentException("New business location cannot be empty.", nameof(newLocation));

        Name = newName;
        Location = newLocation;
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ArchiveBusiness()
    {
        Status = BusinessStatus.Archived;
        DeletedAt = DateTime.UtcNow;
    }
}