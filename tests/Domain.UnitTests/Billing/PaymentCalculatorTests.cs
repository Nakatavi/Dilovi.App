using Domain.Billing;
using Domain.Billing.Strategies;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using static Domain.UnitTests.TestData;

namespace Domain.UnitTests.Billing;

public class PaymentCalculatorTests
{
    private static readonly BillingPeriod August = new(2026, 8);

    private readonly PaymentCalculator _calculator = new(
    [
        new MeterConsumptionChargeStrategy(),
        new AreaChargeStrategy(),
        new PerResidentChargeStrategy()
    ]);

    private static readonly Tariff[] Tariffs =
    [
        Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 30, "2026-01-01", "2026-08-15"),
        Tariff(ServiceType.ColdWater, ChargeBasis.MeterConsumption, 36, "2026-08-16"),
        Tariff(ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, "2026-01-01"),
        Tariff(ServiceType.BuildingMaintenance, ChargeBasis.Area, 8.5m, "2026-01-01"),
        Tariff(ServiceType.WasteCollection, ChargeBasis.PerResident, 45, "2026-01-01")
    ];

    private static readonly MeterReading[] Readings =
    [
        Cold(100, "2026-07-31"),
        Cold(112.5m, "2026-08-31"),
        Hot(50, "2026-07-31"),
        Hot(53, "2026-08-20"),
        Hot(0, "2026-08-20", isNewMeter: true),
        Hot(2, "2026-08-31")
    ];

    [Fact]
    public void Calculate_BuildsLinePerServiceAndAdditionalCharges()
    {
        var context = new BillingContext(Apartment(area: 54.3m, residents: 3), August, Readings);

        var statement = _calculator.Calculate(context, Tariffs, [Charge("Roof repair fund", 200, 2026, 8)]);

        Assert.Equal(ApartmentId, statement.ApartmentId);
        Assert.Equal(August, statement.Period);
        Assert.Collection(statement.Lines,
            line => AssertLine(line, "ColdWater", 12.5m, "m³", 33.0968m, 413.71m),
            line => AssertLine(line, "HotWater", 5m, "m³", 110m, 550.00m),
            line => AssertLine(line, "BuildingMaintenance", 54.3m, "m²", 8.5m, 461.55m),
            line => AssertLine(line, "WasteCollection", 3m, "person", 45m, 135.00m),
            line => AssertLine(line, "Roof repair fund", null, null, null, 200.00m));
        Assert.Equal(1760.26m, statement.Total.Amount);
    }

    [Fact]
    public void Calculate_IgnoresAdditionalChargesOfOtherPeriodsAndApartments()
    {
        var context = new BillingContext(Apartment(), August, Readings);
        var charges = new[]
        {
            Charge("July fee", 100, 2026, 7),
            Charge("Other apartment", 100, 2026, 8, apartmentId: Guid.NewGuid())
        };

        var statement = _calculator.Calculate(context, Tariffs, charges);

        Assert.DoesNotContain(statement.Lines, l => l.Quantity is null);
    }

    [Fact]
    public void Calculate_WithNegativeAdditionalCharge_ReducesTotal()
    {
        var context = new BillingContext(Apartment(residents: 2), August, []);
        var tariffs = new[] { Tariff(ServiceType.WasteCollection, ChargeBasis.PerResident, 45, "2026-01-01") };

        var statement = _calculator.Calculate(context, tariffs, [Charge("Discount", -10.5m, 2026, 8)]);

        Assert.Equal(79.50m, statement.Total.Amount);
    }

    [Fact]
    public void Calculate_RoundsEachLineToKopecks()
    {
        var context = new BillingContext(Apartment(area: 33.33m), August, []);
        var tariffs = new[] { Tariff(ServiceType.BuildingMaintenance, ChargeBasis.Area, 7.777m, "2026-01-01") };

        var statement = _calculator.Calculate(context, tariffs, []);

        // 33.33 * 7.777 = 259.20741
        Assert.Equal(259.21m, statement.Lines.Single().Amount.Amount);
    }

    [Fact]
    public void Calculate_WithoutTariffsInPeriod_ReturnsOnlyAdditionalCharges()
    {
        var context = new BillingContext(Apartment(), August, []);
        var oldTariff = Tariff(ServiceType.WasteCollection, ChargeBasis.PerResident, 45, "2025-01-01", "2025-12-31");

        var statement = _calculator.Calculate(context, [oldTariff], [Charge("Fee", 50, 2026, 8)]);

        Assert.Single(statement.Lines);
        Assert.Equal(50m, statement.Total.Amount);
    }

    [Fact]
    public void Calculate_WhenServiceTariffsUseDifferentBases_Throws()
    {
        var context = new BillingContext(Apartment(), August, []);
        var tariffs = new[]
        {
            Tariff(ServiceType.WasteCollection, ChargeBasis.PerResident, 45, "2026-01-01", "2026-08-15"),
            Tariff(ServiceType.WasteCollection, ChargeBasis.Area, 1, "2026-08-16")
        };

        Assert.Throws<DomainException>(() => _calculator.Calculate(context, tariffs, []));
    }

    [Fact]
    public void Calculate_WithoutStrategyForBasis_Throws()
    {
        var calculator = new PaymentCalculator([new AreaChargeStrategy()]);
        var context = new BillingContext(Apartment(), August, []);
        var tariffs = new[] { Tariff(ServiceType.WasteCollection, ChargeBasis.PerResident, 45, "2026-01-01") };

        Assert.Throws<InvalidOperationException>(() => calculator.Calculate(context, tariffs, []));
    }

    private static void AssertLine(PaymentLine line, string description, decimal? quantity, string? unit,
        decimal? unitPrice, decimal amount)
    {
        Assert.Equal(description, line.Description);
        Assert.Equal(quantity, line.Quantity);
        Assert.Equal(unit, line.Unit);
        Assert.Equal(unitPrice, line.UnitPrice);
        Assert.Equal(amount, line.Amount.Amount);
    }
}
