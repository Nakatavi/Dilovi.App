namespace Domain.Billing.Strategies;

public sealed class PerResidentChargeStrategy : IChargeStrategy
{
    public ChargeBasis Basis => ChargeBasis.PerResident;

    public ChargeQuantity GetQuantity(ServiceType service, BillingContext context) =>
        new(context.Apartment.ResidentsCount, "person");
}
