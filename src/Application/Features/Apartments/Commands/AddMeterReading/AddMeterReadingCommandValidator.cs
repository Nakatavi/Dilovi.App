using FluentValidation;

namespace Application.Features.Apartments.Commands.AddMeterReading;

public class AddMeterReadingCommandValidator : AbstractValidator<AddMeterReadingCommand>
{
    public AddMeterReadingCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.ApartmentId).NotEmpty();
        RuleFor(x => x.MeterType).IsInEnum();
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(12, 3, ignoreTrailingZeros: true);
        RuleFor(x => x.ReadingDate)
            .Must(date => date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("Reading date cannot be in the future.");
    }
}
