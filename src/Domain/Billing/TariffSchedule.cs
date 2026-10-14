using System.Globalization;
using Domain.Common;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Billing;

/// <summary>
/// Resolves the price of a service for a billing period from its tariff history.
/// </summary>
public static class TariffSchedule
{
    /// <summary>
    /// Returns the day-weighted price for <paramref name="period"/>.
    /// When the tariff changes mid-month, each tariff applies to the share of days it was valid.
    /// </summary>
    /// <exception cref="DomainException">Tariffs overlap or do not cover the whole period.</exception>
    public static decimal GetEffectivePrice(IEnumerable<Tariff> tariffs, BillingPeriod period)
    {
        var applicable = tariffs
            .Where(t => t.Overlaps(period.Start, period.End))
            .OrderBy(t => t.ValidFrom)
            .ToList();

        if (applicable.Count == 0)
        {
            throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                $"No tariff is defined for period {period}."));
        }

        EnsureNoOverlaps(applicable);

        var coveredDays = 0;
        var weightedPrice = 0m;

        foreach (var tariff in applicable)
        {
            var from = Max(tariff.ValidFrom, period.Start);
            var to = Min(tariff.ValidTo ?? period.End, period.End);
            var days = to.DayNumber - from.DayNumber + 1;

            coveredDays += days;
            weightedPrice += tariff.PricePerUnit * days;
        }

        if (coveredDays != period.Days)
        {
            throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                $"{applicable[0].ServiceType} tariffs cover {coveredDays} of {period.Days} days in period {period}."));
        }

        return weightedPrice / period.Days;
    }

    /// <summary>
    /// Throws when any two tariffs of the same service are valid on the same day.
    /// </summary>
    public static void EnsureNoOverlaps(IEnumerable<Tariff> tariffs)
    {
        var ordered = tariffs.OrderBy(t => t.ValidFrom).ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            var previous = ordered[i - 1];
            var current = ordered[i];

            if (previous.ValidTo is null || previous.ValidTo >= current.ValidFrom)
            {
                throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                    $"{current.ServiceType} tariff starting {current.ValidFrom:yyyy-MM-dd} overlaps the tariff starting {previous.ValidFrom:yyyy-MM-dd}."));
            }
        }
    }

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
}
