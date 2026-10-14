using Application.Features.Responses;

namespace Application.Features.Apartments.Commands.CreateApartment;

public record CreateApartmentCommand(
    string Number,
    decimal Area,
    int ResidentsCount) : IRequest<EntityCreatedResponse>;
