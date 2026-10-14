using Domain.Common;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Billing;

/// <summary>
/// Builds a monthly payment statement: one line per service (priced by its tariff and
/// charge strategy) plus additional charges.
/// </summary>
public class PaymentCalculator
{
    private readonly IReadOnlyDictionary<ChargeBasis, IChargeStrategy> _strategies;

    public PaymentCalculator(IEnumerable<IChargeStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Basis);
    }

    public PaymentStatement Calculate(
        BillingContext context,
        IEnumerable<Tariff> tariffs,
        IEnumerable<AdditionalCharge> additionalCharges)
    {
        var period = context.Period;
        var lines = new List<PaymentLine>();

        var tariffsByService = tariffs
            .Where(t => t.Overlaps(period.Start, period.End))
            .GroupBy(t => t.ServiceType)
            .OrderBy(g => g.Key);

        foreach (var serviceTariffs in tariffsByService)
        {
            lines.Add(CalculateServiceLine(serviceTariffs.Key, serviceTariffs.ToList(), context));
        }

        lines.AddRange(additionalCharges
            .Where(c => c.ApartmentId == context.Apartment.Id && c.Year == period.Year && c.Month == period.Month)
            .Select(c => new PaymentLine(c.Description, null, null, null, Money.Of(c.Amount))));

        var total = lines.Aggregate(Money.Zero, (sum, line) => sum + line.Amount);

        return new PaymentStatement(context.Apartment.Id, period, lines, total);
    }

    private PaymentLine CalculateServiceLine(ServiceType service, IReadOnlyList<Tariff> tariffs, BillingContext context)
    {
        var basis = tariffs.Select(t => t.ChargeBasis).Distinct().ToList();
        if (basis.Count > 1)
        {
            throw new DomainException($"{service} tariffs use different charge bases in period {context.Period}.");
        }

        if (!_strategies.TryGetValue(basis[0], out var strategy))
        {
            throw new InvalidOperationException($"No charge strategy is registered for {basis[0]}.");
        }

        var unitPrice = TariffSchedule.GetEffectivePrice(tariffs, context.Period);
        var quantity = strategy.GetQuantity(service, context);

        return new PaymentLine(
            service.ToString(),
            quantity.Value,
            quantity.Unit,
            Math.Round(unitPrice, 4, MidpointRounding.AwayFromZero),
            Money.Of(quantity.Value * unitPrice));
    }
}
