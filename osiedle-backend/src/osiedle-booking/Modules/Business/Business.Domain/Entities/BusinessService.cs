using Business.Domain.Aggregates;
using Business.Domain.Enums;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class BusinessService : Entity<BusinessServiceId>
{
    public EmployeeId EmployeeId { get; private set; }
    
    public Employee Employee { get; private set; }
    
    public string ServiceName { get; private set; }
    
    public string Description { get; private set; }
    
    public decimal Price { get; private set; }
    
    public TimeSpan Duration { get; private set; }
    
    public ServiceStatus Status { get; private set; }
    
    private BusinessService() { }
    
    private BusinessService(BusinessServiceId id, EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan date) : base(id)   
    {
        EmployeeId = employeeId;
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = date;
        Status = ServiceStatus.Active;
    }
    
    public static BusinessService Create(EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan duration)
    {
        var id = BusinessServiceId.New();
        return new BusinessService(id, employeeId, serviceName, description, price, duration);
    }
    
    public void UpdateInfo(string serviceName, string description, decimal price, TimeSpan duration)
    {
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = duration;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddToEmployee(EmployeeId employeeId)
    {
        EmployeeId = employeeId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveFromEmployee(EmployeeId employeeId)
    {
        EmployeeId = employeeId;
        Status = ServiceStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ArchiveService()
    {
        Status = ServiceStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }
}