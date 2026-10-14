namespace Domain.Billing;

/// <summary>
/// Calculates the billable quantity of a service for one <see cref="ChargeBasis"/>.
/// </summary>
public interface IChargeStrategy
{
    ChargeBasis Basis { get; }

    ChargeQuantity GetQuantity(ServiceType service, BillingContext context);
}

public sealed record ChargeQuantity(decimal Value, string Unit);
