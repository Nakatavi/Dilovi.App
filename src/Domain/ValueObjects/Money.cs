namespace Domain.ValueObjects;

/// <summary>
/// Monetary amount in UAH, always rounded to kopecks (2 decimal places, away from zero).
/// </summary>
public readonly record struct Money
{
    public static readonly Money Zero = new(0m);

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public static Money Of(decimal amount) => new(Math.Round(amount, 2, MidpointRounding.AwayFromZero));

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public override string ToString() => Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
