namespace Business.Domain.ValueObjects;

public readonly  record struct EmployeeName
{
    private string FirstName { get; }
    private string LastName { get; }
    
    private EmployeeName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static EmployeeName Create(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        
        var normalizedFirstName = firstName.Trim();
        var normalizedLastName = lastName.Trim();
        
        return new EmployeeName(normalizedFirstName, normalizedLastName);
    }

    //$$ LastName
    public bool IsEmpty => string.IsNullOrEmpty(FirstName);
    
    public override string ToString() => $"{FirstName} {LastName}";
}