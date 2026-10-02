using Business.Domain.Enums;
using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain.Aggregates;

public class Business : AggregateRoot<BusinessId> 
{
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; } 
    
    public string Description { get; private set; }
    
    public BusinessStatus Status { get; private set; }
    
    private Business() { }

    private Business(BusinessId id, string name, Guid ownerId, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Business name cannot be empty.", nameof(name));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));

        Name = name;
        OwnerId = ownerId;
        Description = description;
        Status = BusinessStatus.Active;
    }
    
    public static Business Create(string name, Guid ownerId, string description)
    {
        var businessId = BusinessId.New();
        return new Business(businessId, name, ownerId, description);
    }

    public void UpdateName(string newName, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New business name cannot be empty.", nameof(newName));
        
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("New business description cannot be empty.", nameof(newDescription));

        Name = newName;
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ArchiveBusiness()
    {
        Status = BusinessStatus.Archived;
        DeletedAt = DateTime.UtcNow;
    }
}