using Application.Features.Responses;
using Domain.Enums;

namespace Application.Features.Tariffs.Commands.CreateTariff;

public record CreateTariffCommand(
    ServiceType ServiceType,
    ChargeBasis ChargeBasis,
    decimal PricePerUnit,
    DateOnly ValidFrom,
    DateOnly? ValidTo) : IRequest<EntityCreatedResponse>;
