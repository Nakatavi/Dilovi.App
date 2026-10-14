using Domain.Billing;
using Domain.Billing.Strategies;
using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using static Domain.UnitTests.TestData;

namespace Domain.UnitTests.Billing;

public class ChargeStrategyTests
{
    private static readonly BillingPeriod August = new(2026, 8);

    [Fact]
    public void AreaStrategy_ReturnsApartmentArea()
    {
        var context = new BillingContext(Apartment(area: 54.3m), August, []);

        var quantity = new AreaChargeStrategy().GetQuantity(ServiceType.BuildingMaintenance, context);

        Assert.Equal(new ChargeQuantity(54.3m, "m²"), quantity);
    }

    [Fact]
    public void PerResidentStrategy_ReturnsResidentsCount()
    {
        var context = new BillingContext(Apartment(residents: 4), August, []);

        var quantity = new PerResidentChargeStrategy().GetQuantity(ServiceType.WasteCollection, context);

        Assert.Equal(new ChargeQuantity(4, "person"), quantity);
    }

    [Theory]
    [InlineData(ServiceType.ColdWater, 12.5)]
    [InlineData(ServiceType.HotWater, 3)]
    public void MeterConsumptionStrategy_UsesMatchingMeter(ServiceType service, decimal expected)
    {
        var readings = new[]
        {
            Cold(100, "2026-07-31"), Cold(112.5m, "2026-08-31"),
            Hot(50, "2026-07-31"), Hot(53, "2026-08-31")
        };
        var context = new BillingContext(Apartment(), August, readings);

        var quantity = new MeterConsumptionChargeStrategy().GetQuantity(service, context);

        Assert.Equal(new ChargeQuantity(expected, "m³"), quantity);
    }

    [Fact]
    public void MeterConsumptionStrategy_ForNonMeteredService_Throws()
    {
        var context = new BillingContext(Apartment(), August, []);

        Assert.Throws<DomainException>(
            () => new MeterConsumptionChargeStrategy().GetQuantity(ServiceType.WasteCollection, context));
    }

    [Theory]
    [InlineData(ServiceType.ColdWater, true)]
    [InlineData(ServiceType.HotWater, true)]
    [InlineData(ServiceType.BuildingMaintenance, false)]
    [InlineData(ServiceType.WasteCollection, false)]
    public void IsMetered_ReturnsTrueOnlyForWater(ServiceType service, bool expected)
    {
        Assert.Equal(expected, MeterConsumptionChargeStrategy.IsMetered(service));
    }
}
