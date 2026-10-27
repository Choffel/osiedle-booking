namespace osiedle_booking.BuildingBlocks.Domain;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}