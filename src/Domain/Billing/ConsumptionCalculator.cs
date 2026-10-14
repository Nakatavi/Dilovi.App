using System.Globalization;
using Domain.Common;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Billing;

/// <summary>
/// Calculates metered consumption for a billing period from a meter's reading history.
/// </summary>
public static class ConsumptionCalculator
{
    /// <summary>
    /// Returns consumption (m³) for <paramref name="period"/>.
    /// </summary>
    /// <remarks>
    /// The baseline is the last reading before the period. When there is none (first month of a meter),
    /// the first reading in the period is the baseline. Readings marked <see cref="MeterReading.IsNewMeter"/>
    /// start a new meter, so no consumption is counted between the old meter's last value and the new one.
    /// </remarks>
    public static decimal Calculate(IEnumerable<MeterReading> readings, MeterType meterType, BillingPeriod period)
    {
        var ordered = Order(readings.Where(r => r.MeterType == meterType && r.ReadingDate <= period.End));

        var inPeriod = ordered.Where(r => period.Contains(r.ReadingDate)).ToList();
        if (inPeriod.Count == 0)
        {
            throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                $"No {meterType} meter reading for period {period}."));
        }

        var baseline = ordered.LastOrDefault(r => r.ReadingDate < period.Start);
        var sequence = baseline is null ? inPeriod : [baseline, .. inPeriod];

        var consumption = 0m;
        for (var i = 1; i < sequence.Count; i++)
        {
            consumption += GetDelta(sequence[i - 1], sequence[i]);
        }

        return consumption;
    }

    /// <summary>
    /// Validates that <paramref name="next"/> can follow the latest existing reading of the same meter.
    /// </summary>
    public static void EnsureCanFollow(MeterReading? previous, MeterReading next)
    {
        if (previous is null)
        {
            return;
        }

        if (next.ReadingDate < previous.ReadingDate)
        {
            throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                $"Reading date {next.ReadingDate:yyyy-MM-dd} is earlier than the last {next.MeterType} reading ({previous.ReadingDate:yyyy-MM-dd})."));
        }

        GetDelta(previous, next);
    }

    private static List<MeterReading> Order(IEnumerable<MeterReading> readings) =>
        readings
            .OrderBy(r => r.ReadingDate)
            // On a replacement day the old meter's final reading goes before the new meter's initial one
            .ThenBy(r => r.IsNewMeter)
            .ToList();

    private static decimal GetDelta(MeterReading previous, MeterReading current)
    {
        if (current.IsNewMeter)
        {
            return 0m;
        }

        var delta = current.Value - previous.Value;
        if (delta < 0)
        {
            throw new DomainException(string.Create(CultureInfo.InvariantCulture,
                $"{current.MeterType} reading {current.Value} on {current.ReadingDate:yyyy-MM-dd} is lower than the previous " +
                $"reading {previous.Value} on {previous.ReadingDate:yyyy-MM-dd}. If the meter was replaced, mark the reading as a new meter."));
        }

        return delta;
    }
}
