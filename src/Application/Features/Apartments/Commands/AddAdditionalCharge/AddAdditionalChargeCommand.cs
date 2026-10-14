using Application.Features.Responses;

namespace Application.Features.Apartments.Commands.AddAdditionalCharge;

public record AddAdditionalChargeCommand(
    Guid ApartmentId,
    string Description,
    decimal Amount,
    int Year,
    int Month) : IRequest<EntityCreatedResponse>;
