using osiedle_booking.BuildingBlocks.Domain.Entity;

namespace osiedle_booking.BuildingBlocks.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    
    protected AggregateRoot() { }
}