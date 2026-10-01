using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain;

public class BusinessService : AggregateRoot<BusinessId>
{
    public BusinessServiceId Id { get; private set; }
    
    public Guid EmployeeId { get; private set; }
    
    public string ServiceName { get; private set; }
    
    public string Description { get; private set; }
    
    public double Price { get; private set; }
    
    //TODO: Change type for Timeslot or create new ValueObject for it
    public DateTime Date { get; private set; }
    
    private BusinessService() { }
    
}