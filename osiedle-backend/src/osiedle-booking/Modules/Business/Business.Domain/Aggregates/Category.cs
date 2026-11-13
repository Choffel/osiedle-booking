using Business.Domain.ValueObjects.Category;
using osiedle_booking.BuildingBlocks.Domain;

namespace Business.Domain.Aggregates;

public class Category : AggregateRoot<CategoryId>
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public int DisplayOrder { get; private set; }
    
    private Category() { }
    private Category(CategoryId id, string name, string slug, int displayOrder) : base(id)
    {
        ValidateAndAssign(ref name, displayOrder);

        Name = name;
        Slug = Slug.Create(slug);
        DisplayOrder = displayOrder;
    }

    public static Category Create(string name, string slug, int displayOrder)
    {
        return new Category
        (
            CategoryId.New(),
            name,
            slug,
            displayOrder
        );
    }

    public void Update(string name, int displayOrder)
    {
        ValidateAndAssign(ref name, displayOrder);
        
        Name = name;
        DisplayOrder = displayOrder;
    }

    public void Archive()
    {
        if (DeletedAt != null)
            throw new InvalidOperationException("Category is already archived.");
        
        DeletedAt = DateTime.UtcNow;
    }

    private void ValidateAndAssign(ref string name, int displayOrder)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        name = name.Trim();
        
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        
        if(displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order cannot be negative.");
        
        if (DeletedAt != null)
            throw new InvalidOperationException("Cannot update a deleted category.");
    }
}