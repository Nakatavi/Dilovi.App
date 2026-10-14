using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Payments.Queries.CalculatePayment;

public class CalculatePaymentQueryValidator : AbstractValidator<CalculatePaymentQuery>
{
    public CalculatePaymentQueryValidator()
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(BillingPeriod.MinYear, BillingPeriod.MaxYear);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
