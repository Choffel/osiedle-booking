namespace Bookings.Domain.Entities.Booking.Value_objects;

public record struct Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }
    
    public static Money Create(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }
    
    public static Money Pln(decimal amount)
    {
        return Create(amount, "PLN");
    }
}