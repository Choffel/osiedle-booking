using Business.Domain.Enums;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class BookableService : Entity<BookableServiceId>
{
    public EmployeeId EmployeeId { get; private set; }
    
    public string ServiceName { get; private set; }
    
    public string Description { get; private set; }
    
    public decimal Price { get; private set; }
    
    public TimeSpan Duration { get; private set; }
    
    public ServiceStatus Status { get; private set; }
    
    private BookableService() { }
    
    private BookableService(
        BookableServiceId id, 
        EmployeeId employeeId, 
        string serviceName, 
        string? description, 
        decimal price, 
        TimeSpan duration) : base(id)
    {
        if (employeeId.Value == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));

        EmployeeId = employeeId;
        ServiceName = EnsureValidServiceName(serviceName);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = EnsureValidPrice(price);
        Duration = EnsureValidDuration(duration);
        Status = ServiceStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static BookableService Create(
        EmployeeId employeeId, 
        string serviceName, 
        string? description, 
        decimal price, 
        TimeSpan duration)
    {
        return new BookableService(
            BookableServiceId.New(), 
            employeeId, 
            serviceName, 
            description, 
            price, 
            duration);
    }

    public void UpdateDetails(string newName, string? newDescription)
    {
        EnsureIsNotArchived();

        ServiceName = EnsureValidServiceName(newName);
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePrice(decimal newPrice)
    {
        EnsureIsNotArchived();

        var validPrice = EnsureValidPrice(newPrice);
        if (Price == validPrice)
            return;

        Price = validPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDuration(TimeSpan newDuration)
    {
        EnsureIsNotArchived();

        var validDuration = EnsureValidDuration(newDuration);
        if (Duration == validDuration)
            return;

        Duration = validDuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        EnsureIsNotArchived();

        if (Status == ServiceStatus.Active)
            return;

        Status = ServiceStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        EnsureIsNotArchived();

        if (Status == ServiceStatus.Inactive)
            return;

        Status = ServiceStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        EnsureIsNotArchived();

        Status = ServiceStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private void EnsureIsNotArchived()
    {
        if (Status == ServiceStatus.Archived)
            throw new InvalidOperationException("Cannot perform operation on an archived service.");
    }

    private static string EnsureValidServiceName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new ArgumentException("Service name must be between 2 and 100 characters.", nameof(name));

        return trimmed;
    }

    private static decimal EnsureValidPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Service price cannot be negative.", nameof(price));

        return price;
    }

    private static TimeSpan EnsureValidDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMinutes(5) || duration > TimeSpan.FromHours(24))
            throw new ArgumentException("Service duration must be between 5 minutes and 24 hours.", nameof(duration));

        return duration;
    }
}
