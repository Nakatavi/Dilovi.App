using Domain.Billing.Strategies;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Tariffs.Commands.CreateTariff;

public class CreateTariffCommandValidator : AbstractValidator<CreateTariffCommand>
{
    public CreateTariffCommandValidator()
    {
        RuleFor(x => x.ServiceType).IsInEnum();
        RuleFor(x => x.ChargeBasis).IsInEnum();
        RuleFor(x => x.PricePerUnit).GreaterThan(0).PrecisionScale(12, 4, ignoreTrailingZeros: true);

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .When(x => x.ValidTo.HasValue)
            .WithMessage("ValidTo must not be earlier than ValidFrom.");

        // Metered services are charged by consumption, and only they can be
        RuleFor(x => x.ChargeBasis)
            .Must((command, basis) =>
                MeterConsumptionChargeStrategy.IsMetered(command.ServiceType) == (basis == ChargeBasis.MeterConsumption))
            .WithMessage(command => $"{command.ChargeBasis} charge basis is not allowed for {command.ServiceType}.");
    }
}
