using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class BillingPeriodTests
{
    [Theory]
    [InlineData(2026, 8, 31)]
    [InlineData(2026, 2, 28)]
    [InlineData(2028, 2, 29)] // leap year
    [InlineData(2026, 4, 30)]
    public void Days_ReturnsNumberOfDaysInMonth(int year, int month, int expectedDays)
    {
        var period = new BillingPeriod(year, month);

        Assert.Equal(expectedDays, period.Days);
        Assert.Equal(new DateOnly(year, month, 1), period.Start);
        Assert.Equal(new DateOnly(year, month, expectedDays), period.End);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(1999, 5)]
    [InlineData(2101, 5)]
    public void Constructor_WithInvalidValues_Throws(int year, int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BillingPeriod(year, month));
    }

    [Theory]
    [InlineData("2026-08-01", true)]
    [InlineData("2026-08-31", true)]
    [InlineData("2026-07-31", false)]
    [InlineData("2026-09-01", false)]
    public void Contains_ChecksInclusiveBounds(string date, bool expected)
    {
        Assert.Equal(expected, new BillingPeriod(2026, 8).Contains(DateOnly.Parse(date)));
    }

    [Fact]
    public void ToString_UsesYearMonthFormat()
    {
        Assert.Equal("2026-03", new BillingPeriod(2026, 3).ToString());
    }
}
