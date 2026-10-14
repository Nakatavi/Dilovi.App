using Domain.Common;

namespace Domain.Billing.Strategies;

public sealed class MeterConsumptionChargeStrategy : IChargeStrategy
{
    public ChargeBasis Basis => ChargeBasis.MeterConsumption;

    public ChargeQuantity GetQuantity(ServiceType service, BillingContext context)
    {
        var meterType = ToMeterType(service);
        var consumption = ConsumptionCalculator.Calculate(context.MeterReadings, meterType, context.Period);

        return new ChargeQuantity(consumption, "m³");
    }

    public static bool IsMetered(ServiceType service) => service is ServiceType.ColdWater or ServiceType.HotWater;

    private static MeterType ToMeterType(ServiceType service) => service switch
    {
        ServiceType.ColdWater => MeterType.ColdWater,
        ServiceType.HotWater => MeterType.HotWater,
        _ => throw new DomainException($"Service {service} is not metered.")
    };
}
