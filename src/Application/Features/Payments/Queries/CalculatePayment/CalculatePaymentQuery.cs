namespace Application.Features.Payments.Queries.CalculatePayment;

public record CalculatePaymentQuery(Guid ApartmentId, int Year, int Month) : IRequest<PaymentStatementDto>;
