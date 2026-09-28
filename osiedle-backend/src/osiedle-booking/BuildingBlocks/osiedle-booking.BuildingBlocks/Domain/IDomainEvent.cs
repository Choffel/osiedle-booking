namespace osiedle_booking.BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredOn => DateTime.UtcNow;
}