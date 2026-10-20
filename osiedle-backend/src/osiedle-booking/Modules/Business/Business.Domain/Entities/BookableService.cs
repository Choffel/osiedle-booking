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
    
    private BookableService(BookableServiceId id, EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan date) : base(id)   
    {
        EmployeeId = employeeId;
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = date;
        Status = ServiceStatus.Active;
    }
    
    public static BookableService Create(EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan duration)
    {
        var id = BookableServiceId.New();
        return new BookableService(id, employeeId, serviceName, description, price, duration);
    }
    
    public void UpdateInfo(string serviceName, string description, decimal price, TimeSpan duration)
    {
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = duration;
        UpdatedAt = DateTime.UtcNow;
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
