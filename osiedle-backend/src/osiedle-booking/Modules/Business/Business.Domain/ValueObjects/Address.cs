namespace Business.Domain.ValueObjects;

//TODO: add postal code
public readonly record struct Address
{
    private  string Street { get; }
    private  string Number { get; }
    private string City { get; }
    
    private Address(string street, string number, string city)
    {
        Street = street;
        Number = number;
        City = city;
    }


    public static Address Create(string street, string number, string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        
        var normalizedStreet = street.Trim();
        var normalizedNumber = number.Trim();
        var normalizedCity = city.Trim();
        
        return new Address(normalizedStreet, normalizedNumber, normalizedCity);
    }
    
    public bool IsEmpty => string.IsNullOrEmpty(Street);

    public override string ToString() => $"{Street} {Number}, {City}";
}