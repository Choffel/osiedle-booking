using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain.Aggregates;

public class Business : AggregateRoot<BusinessId> 
{
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; } 
    
    public string Description { get; private set; }
    public string Location { get; private set; } 
    
    private readonly List<BusinessService> _services = new();
    private readonly List<Employee> _employees = new();

    public IReadOnlyList<BusinessService> Services => _services;
    public IReadOnlyList<Employee> Employees => _employees;
    
    
    public BusinessStatus Status { get; private set; }
    
    private Business() { }

    private Business(BusinessId id, string name, string location, Guid ownerId, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));
        
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
    
    public static Business Create(string name, Guid ownerId, string description, string location)
    {
        var businessId = BusinessId.New();
        return new Business(businessId, name, location, ownerId, description);
    }

    public void AddService(EmployeeId employeeId, string name, string description, decimal price, int durationMinutes)
    {
        if (Status == BusinessStatus.Archived)
            throw new InvalidOperationException("Cannot modify an archived business.");

        if (_services.Any(s => s.ServiceName == name))
            throw new InvalidOperationException("Service with this name already exists.");
        
        var duration = TimeSpan.FromMinutes(durationMinutes);

        var service = BusinessService.Create(employeeId, name, description, price, duration);
        
        _services.Add(service);
        UpdatedAt = DateTime.UtcNow;
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