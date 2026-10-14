using FluentValidation;

namespace Application.Features.Apartments.Commands.CreateApartment;

public class CreateApartmentCommandValidator : AbstractValidator<CreateApartmentCommand>
{
    public CreateApartmentCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(16);
        RuleFor(x => x.Area).GreaterThan(0).LessThan(10_000).PrecisionScale(10, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.ResidentsCount).InclusiveBetween(0, 50);
    }
}
