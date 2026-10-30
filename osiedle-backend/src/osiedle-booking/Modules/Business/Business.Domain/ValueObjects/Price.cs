namespace Business.Domain.ValueObjects;

public readonly record struct Price
{
    public const string DefaultCurrency = "PLN";

    public decimal Amount { get; }
    public string Currency { get; }
    
    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price Create(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        return new Price(amount, currency.Trim().ToUpperInvariant());
    }
    
    public static Price Free(string currency = DefaultCurrency) => new(0m, currency);
    
    public bool IsEmpty => string.IsNullOrEmpty(Currency);
    
    public override string ToString() => $"{Amount:F2} {Currency}";
    
}