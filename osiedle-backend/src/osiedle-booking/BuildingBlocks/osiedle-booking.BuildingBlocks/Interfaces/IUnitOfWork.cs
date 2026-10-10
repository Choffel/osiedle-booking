namespace osiedle_booking.BuildingBlocks.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangeAsync(CancellationToken cancellationToken = default);
}