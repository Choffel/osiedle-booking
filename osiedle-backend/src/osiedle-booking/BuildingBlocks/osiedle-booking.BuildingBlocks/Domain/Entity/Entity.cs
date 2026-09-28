namespace osiedle_booking.BuildingBlocks.Domain.Entity;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateOnly CreatedAt { get; set; }
    public DateOnly UpdatedAt { get; set; }
    public DateOnly DeletedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b) =>
        a is null && b is null || (a is not null && a.Equals(b));

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);
}