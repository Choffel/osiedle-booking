namespace Users.Domain.Entity.User.ValueObjects;

public record Name
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    private Name(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Name Create(string firstName, string lastName)
    {
        if(string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name and last name cannot be null or empty.", nameof(lastName));
        
        if(string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name and last name cannot be null or empty.", nameof(lastName));
        
        return new Name(firstName, lastName);
    }
    
    public void Update(string firstName, string lastName)
    {
        if(string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name and last name cannot be null or empty.", nameof(lastName));
        
        if(string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name and last name cannot be null or empty.", nameof(lastName));
        
        FirstName = firstName;
        LastName = lastName;
    }
}