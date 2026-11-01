namespace Business.Domain.ValueObjects;

//TODO: add postal code
public readonly record struct Address
{
    public string Street { get; }
    public string Number { get; }
    public string City { get; }
    
    public Address(string street, string number, string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        
        Street = street.Trim();
        Number = number.Trim();
        City = city.Trim();
    }


    public static Address Create(string street, string number, string city)
    {
        return new Address(street, number, city);
    }
    
    public bool IsEmpty => string.IsNullOrEmpty(Street);

    public override string ToString() => $"{Street} {Number}, {City}";
}
