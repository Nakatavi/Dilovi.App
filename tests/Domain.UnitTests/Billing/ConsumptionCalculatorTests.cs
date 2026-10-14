using Domain.Billing;
using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using static Domain.UnitTests.TestData;

namespace Domain.UnitTests.Billing;

public class ConsumptionCalculatorTests
{
    private static readonly BillingPeriod August = new(2026, 8);

    [Fact]
    public void Calculate_UsesLastReadingBeforePeriodAsBaseline()
    {
        var readings = new[]
        {
            Cold(90, "2026-06-30"),
            Cold(100, "2026-07-31"),
            Cold(112.5m, "2026-08-31")
        };

        var consumption = ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August);

        Assert.Equal(12.5m, consumption);
    }

    [Fact]
    public void Calculate_WithSeveralReadingsInPeriod_SumsAllDeltas()
    {
        var readings = new[]
        {
            Cold(100, "2026-07-31"),
            Cold(104, "2026-08-10"),
            Cold(110, "2026-08-31")
        };

        Assert.Equal(10m, ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));
    }

    [Fact]
    public void Calculate_FirstReadingOfMeter_IsBaselineWithZeroConsumption()
    {
        var readings = new[] { Cold(100, "2026-08-31") };

        Assert.Equal(0m, ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));
    }

    [Fact]
    public void Calculate_WithoutBaseline_CountsFromFirstReadingInPeriod()
    {
        var readings = new[]
        {
            Cold(100, "2026-08-05"),
            Cold(103, "2026-08-31")
        };

        Assert.Equal(3m, ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));
    }

    [Fact]
    public void Calculate_WithMeterReplacement_CountsOldAndNewMeterSeparately()
    {
        var readings = new[]
        {
            Hot(50, "2026-07-31"),
            Hot(53, "2026-08-20"),                  // old meter, final value
            Hot(0, "2026-08-20", isNewMeter: true), // new meter, initial value
            Hot(2, "2026-08-31")
        };

        Assert.Equal(5m, ConsumptionCalculator.Calculate(readings, MeterType.HotWater, August));
    }

    [Fact]
    public void Calculate_OrdersReplacementDayReadingsRegardlessOfInputOrder()
    {
        var readings = new[]
        {
            Hot(2, "2026-08-31"),
            Hot(0, "2026-08-20", isNewMeter: true),
            Hot(50, "2026-07-31"),
            Hot(53, "2026-08-20")
        };

        Assert.Equal(5m, ConsumptionCalculator.Calculate(readings, MeterType.HotWater, August));
    }

    [Fact]
    public void Calculate_WithDecreasingReadingWithoutReplacement_Throws()
    {
        var readings = new[]
        {
            Cold(100, "2026-07-31"),
            Cold(95, "2026-08-31")
        };

        var ex = Assert.Throws<DomainException>(
            () => ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));

        Assert.Contains("lower than the previous reading 100", ex.Message);
    }

    [Fact]
    public void Calculate_WithoutReadingInPeriod_Throws()
    {
        var readings = new[] { Cold(100, "2026-07-31") };

        var ex = Assert.Throws<DomainException>(
            () => ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));

        Assert.Equal("No ColdWater meter reading for period 2026-08.", ex.Message);
    }

    [Fact]
    public void Calculate_IgnoresReadingsAfterPeriodAndOtherMeters()
    {
        var readings = new[]
        {
            Cold(100, "2026-07-31"),
            Cold(110, "2026-08-31"),
            Cold(150, "2026-09-30"),
            Hot(500, "2026-08-31")
        };

        Assert.Equal(10m, ConsumptionCalculator.Calculate(readings, MeterType.ColdWater, August));
    }

    [Fact]
    public void EnsureCanFollow_WithoutPreviousReading_Passes()
    {
        ConsumptionCalculator.EnsureCanFollow(null, Cold(100, "2026-08-31"));
    }

    [Fact]
    public void EnsureCanFollow_WithEarlierDate_Throws()
    {
        Assert.Throws<DomainException>(
            () => ConsumptionCalculator.EnsureCanFollow(Cold(100, "2026-08-31"), Cold(110, "2026-08-01")));
    }

    [Fact]
    public void EnsureCanFollow_WithLowerValue_Throws()
    {
        Assert.Throws<DomainException>(
            () => ConsumptionCalculator.EnsureCanFollow(Cold(100, "2026-07-31"), Cold(90, "2026-08-31")));
    }

    [Fact]
    public void EnsureCanFollow_WithLowerValueOfNewMeter_Passes()
    {
        ConsumptionCalculator.EnsureCanFollow(Cold(100, "2026-07-31"), Cold(0, "2026-08-31", isNewMeter: true));
    }
}
