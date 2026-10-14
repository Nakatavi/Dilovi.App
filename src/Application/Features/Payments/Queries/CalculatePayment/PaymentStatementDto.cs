using Domain.Billing;

namespace Application.Features.Payments.Queries.CalculatePayment;

public record PaymentStatementDto(
    Guid ApartmentId,
    string Period,
    IReadOnlyList<PaymentLineDto> Lines,
    decimal Total)
{
    public static PaymentStatementDto From(PaymentStatement statement) => new(
        statement.ApartmentId,
        statement.Period.ToString(),
        statement.Lines
            .Select(l => new PaymentLineDto(l.Description, l.Quantity, l.Unit, l.UnitPrice, l.Amount.Amount))
            .ToList(),
        statement.Total.Amount);
}

public record PaymentLineDto(
    string Description,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    decimal Amount);
