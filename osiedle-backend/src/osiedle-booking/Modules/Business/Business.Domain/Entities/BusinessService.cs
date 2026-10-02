using Business.Domain.Enums;
using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class BusinessService : Entity<BusinessServiceId>
{
    public EmployeeId EmployeeId { get; private set; }
    
    public string ServiceName { get; private set; }
    
    public string Description { get; private set; }
    
    public decimal Price { get; private set; }
    
    public TimeSpan Date { get; private set; }
    
    public ServiceStatus Status { get; private set; }
    
    private BusinessService() { }
    
    private BusinessService(BusinessServiceId id, EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan date) : base(id)   
    {
        EmployeeId = employeeId;
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Date = date;
        Status = ServiceStatus.Active;
    }
    
    public static BusinessService Create(EmployeeId employeeId, string serviceName, string description, decimal price, TimeSpan date)
    {
        var id = BusinessServiceId.New();
        var businessService = new BusinessService(id, employeeId, serviceName, description, price, date);
        return businessService;
    }
    
    public void UpdateInfo(string serviceName, string description, decimal price, TimeSpan date)
    {
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Date = date;
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