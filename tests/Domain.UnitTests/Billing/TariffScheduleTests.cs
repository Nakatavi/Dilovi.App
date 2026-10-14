using Domain.Billing;
using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using static Domain.UnitTests.TestData;

namespace Domain.UnitTests.Billing;

public class TariffScheduleTests
{
    private static readonly BillingPeriod August = new(2026, 8);

    [Fact]
    public void GetEffectivePrice_WithSingleTariff_ReturnsItsPrice()
    {
        var tariffs = new[] { Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 30, "2026-01-01") };

        Assert.Equal(30m, TariffSchedule.GetEffectivePrice(tariffs, August));
    }

    [Fact]
    public void GetEffectivePrice_WhenTariffChangesMidMonth_WeightsPriceByDays()
    {
        var tariffs = new[]
        {
            Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 30, "2026-01-01", "2026-08-15"),
            Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 36, "2026-08-16")
        };

        var price = TariffSchedule.GetEffectivePrice(tariffs, August);

        // 15 days at 30 + 16 days at 36 over 31 days
        Assert.Equal((15 * 30m + 16 * 36m) / 31, price);
    }

    [Fact]
    public void GetEffectivePrice_IgnoresTariffsOutsidePeriod()
    {
        var tariffs = new[]
        {
            Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 20, "2025-01-01", "2025-12-31"),
            Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 30, "2026-01-01")
        };

        Assert.Equal(30m, TariffSchedule.GetEffectivePrice(tariffs, August));
    }

    [Fact]
    public void GetEffectivePrice_WhenTariffsDoNotCoverWholePeriod_Throws()
    {
        var tariffs = new[] { Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 30, "2026-08-10") };

        var ex = Assert.Throws<DomainException>(() => TariffSchedule.GetEffectivePrice(tariffs, August));

        Assert.Equal("ColdWater tariffs cover 22 of 31 days in period 2026-08.", ex.Message);
    }

    [Fact]
    public void GetEffectivePrice_WithoutTariffs_Throws()
    {
        Assert.Throws<DomainException>(() => TariffSchedule.GetEffectivePrice([], August));
    }

    [Fact]
    public void EnsureNoOverlaps_WithAdjacentTariffs_Passes()
    {
        TariffSchedule.EnsureNoOverlaps(
        [
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 100, "2026-01-01", "2026-06-30"),
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, "2026-07-01")
        ]);
    }

    [Theory]
    [InlineData("2026-06-30")] // shares one day with the first tariff
    [InlineData("2026-03-01")]
    public void EnsureNoOverlaps_WithOverlappingTariffs_Throws(string secondFrom)
    {
        Assert.Throws<DomainException>(() => TariffSchedule.EnsureNoOverlaps(
        [
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 100, "2026-01-01", "2026-06-30"),
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, secondFrom)
        ]));
    }

    [Fact]
    public void EnsureNoOverlaps_WithOpenEndedTariffFollowedByAnother_Throws()
    {
        Assert.Throws<DomainException>(() => TariffSchedule.EnsureNoOverlaps(
        [
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 100, "2026-01-01"),
            Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, "2027-01-01")
        ]));
    }
}
