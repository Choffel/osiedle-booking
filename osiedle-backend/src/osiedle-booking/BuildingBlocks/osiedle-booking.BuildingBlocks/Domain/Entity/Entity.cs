namespace osiedle_booking.BuildingBlocks.Domain.Entity;

public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Entity ID cannot be default.", nameof(id));

        Id = id;
    }
    
    protected Entity() { }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) 
        => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() 
        => (GetType().GetHashCode() * 397) ^ Id.GetHashCode();

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b) 
        => a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) 
        => !(a == b);
}