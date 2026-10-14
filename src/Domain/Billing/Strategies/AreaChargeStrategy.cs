namespace Domain.Billing.Strategies;

public sealed class AreaChargeStrategy : IChargeStrategy
{
    public ChargeBasis Basis => ChargeBasis.Area;

    public ChargeQuantity GetQuantity(ServiceType service, BillingContext context) =>
        new(context.Apartment.Area, "m²");
}
