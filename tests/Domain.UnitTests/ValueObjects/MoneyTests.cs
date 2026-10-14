using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(10.125, 10.13)]
    [InlineData(10.124, 10.12)]
    [InlineData(-0.125, -0.13)]
    [InlineData(413.709677, 413.71)]
    public void Of_RoundsToKopecksAwayFromZero(decimal amount, decimal expected)
    {
        Assert.Equal(expected, Money.Of(amount).Amount);
    }

    [Fact]
    public void Addition_SumsAmounts()
    {
        var total = Money.Of(0.10m) + Money.Of(0.20m);

        Assert.Equal(0.30m, total.Amount);
    }

    [Fact]
    public void ToString_UsesInvariantFormatWithTwoDecimals()
    {
        Assert.Equal("1760.26", Money.Of(1760.26m).ToString());
        Assert.Equal("5.00", Money.Of(5m).ToString());
    }
}
