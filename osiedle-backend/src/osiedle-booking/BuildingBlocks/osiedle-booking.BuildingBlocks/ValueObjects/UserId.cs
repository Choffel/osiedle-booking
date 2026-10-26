namespace osiedle_booking.BuildingBlocks.ValueObjects;

public sealed record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    
    public static UserId From(Guid value) =>  new UserId(value);
}