using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class WorkingHours : Entity<WorkingHoursId>
{
    public EmployeeId EmployeeId { get; private set; }
    
    public DateOnly Date { get; private set; }
    
    public TimeOnly StartTime { get; private set; }
    
    public TimeOnly EndTime { get; private set; }
    
    private WorkingHours() { }
    
    private WorkingHours(WorkingHoursId id, EmployeeId employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime) : base(id)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));
        
        EmployeeId = employeeId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public static WorkingHours Create(EmployeeId employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var id = WorkingHoursId.New();
        return new WorkingHours(id, employeeId, date, startTime, endTime);
    }
    
    public void UpdateTime(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));
        
        StartTime = startTime;
        EndTime = endTime;
        UpdatedAt = DateTime.UtcNow;
    }
}
