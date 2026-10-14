using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Billing;

/// <summary>
/// Apartment data needed to calculate a payment for one period.
/// </summary>
public sealed record BillingContext(
    Apartment Apartment,
    BillingPeriod Period,
    IReadOnlyCollection<MeterReading> MeterReadings);
