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
    
    private BookableService(BookableServiceId id, EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan duration) : base(id)   
    {
        ValidateAndAssign(employeeId, serviceName, description, price, duration);
        Status = ServiceStatus.Active;
    }
    
    public static BookableService Create(EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan duration)
    {
        var id = BookableServiceId.New();
        return new BookableService(id, employeeId, serviceName, description, price, duration);
    }
    
    public void UpdateInfo(string serviceName, string description, decimal price, TimeSpan duration)
    {
        ValidateAndAssign(EmployeeId, serviceName, description, price, duration);
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateAndAssign(EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        if (price <= 0)
            throw new ArgumentException("Service price must be greater than zero.", nameof(price));

        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Service duration must be greater than zero.", nameof(duration));

        EmployeeId = employeeId;
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = duration;
    }
    
    public void DeactivateService()
    {
        if (Status == ServiceStatus.Inactive)
            return; 

        Status = ServiceStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ActivateService()
    {
        if (Status == ServiceStatus.Active)
            return;

        Status = ServiceStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ArchiveService()
    {
        Status = ServiceStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }
}
