using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain;

public class Business : AggregateRoot<BusinessId> 
{
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; } 
    
    private Business() { }

    private Business(BusinessId id, string name, Guid ownerId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));

        Name = name;
        OwnerId = ownerId;
    }
    
    public static Business Create(string name, Guid ownerId)
    {
        var businessId = BusinessId.From(Guid.NewGuid()); 
        return new Business(businessId, name, ownerId);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New business name cannot be empty.", nameof(newName));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }
}