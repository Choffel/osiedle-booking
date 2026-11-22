namespace Business.Domain.ValueObjects;

public readonly record struct Location
{
    public string City { get; }
    public string Street { get; }

    public Location(string city, string street)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        City = city.Trim();
        Street = street.Trim();
    }

    public static Location Create(string city, string street) => new(city, street);

    public override string ToString() => $"{Street}, {City}";
}
