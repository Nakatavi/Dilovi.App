using Domain.ValueObjects;

namespace Domain.Billing;

public sealed record PaymentStatement(
    Guid ApartmentId,
    BillingPeriod Period,
    IReadOnlyList<PaymentLine> Lines,
    Money Total);

/// <summary>
/// One line of a statement. Quantity, unit and price are empty for additional charges.
/// </summary>
public sealed record PaymentLine(
    string Description,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    Money Amount);
