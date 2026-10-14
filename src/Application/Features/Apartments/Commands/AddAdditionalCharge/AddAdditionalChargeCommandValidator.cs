using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Apartments.Commands.AddAdditionalCharge;

public class AddAdditionalChargeCommandValidator : AbstractValidator<AddAdditionalChargeCommand>
{
    public AddAdditionalChargeCommandValidator()
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount must not be zero (use a negative amount for a discount).")
            .PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Year).InclusiveBetween(BillingPeriod.MinYear, BillingPeriod.MaxYear);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
