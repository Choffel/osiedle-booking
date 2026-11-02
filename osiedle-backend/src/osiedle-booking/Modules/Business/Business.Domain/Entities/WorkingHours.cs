using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace Business.Domain.Entities;

public class WorkingHours : Entity<WorkingHoursId>
{
    public EmployeeId EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeSlot Slot { get; private set; } 

    private WorkingHours() { } 

    private WorkingHours(
        WorkingHoursId id, 
        EmployeeId employeeId, 
        DateOnly date, 
        TimeSlot slot) : base(id)
    {
        if (employeeId.Value == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));

        if (slot.IsEmpty)
            throw new ArgumentException("TimeSlot cannot be uninitialized.", nameof(slot));

        EnsureDateIsNotInPast(date);

        EmployeeId = employeeId;
        Date = date;
        Slot = slot;
        CreatedAt = DateTime.UtcNow;
    }

    public static WorkingHours Create(EmployeeId employeeId, DateOnly date, TimeSlot slot)
    {
        return new WorkingHours(WorkingHoursId.New(), employeeId, date, slot);
    }

    

    public void UpdateSlot(TimeSlot newSlot)
    {
        if (newSlot.IsEmpty)
            throw new ArgumentException("New time slot cannot be uninitialized.", nameof(newSlot));
        
        EnsureDateIsNotInPast(Date);

        if (Slot == newSlot)
            return;

        Slot = newSlot;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static void EnsureDateIsNotInPast(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date < today)
        {
            throw new InvalidOperationException("Cannot set or update working hours for a past date.");
        }
    }
}
