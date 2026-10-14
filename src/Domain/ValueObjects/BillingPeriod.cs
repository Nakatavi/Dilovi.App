namespace Domain.ValueObjects;

/// <summary>
/// A calendar month for which payments are calculated.
/// </summary>
public readonly record struct BillingPeriod
{
    public const int MinYear = 2000;
    public const int MaxYear = 2100;

    public BillingPeriod(int year, int month)
    {
        if (year is < MinYear or > MaxYear)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, $"Year must be between {MinYear} and {MaxYear}.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public DateOnly Start => new(Year, Month, 1);

    public DateOnly End => new(Year, Month, Days);

    public int Days => DateTime.DaysInMonth(Year, Month);

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public static BillingPeriod From(DateOnly date) => new(date.Year, date.Month);

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
