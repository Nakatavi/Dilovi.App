using Application.Features.Responses;
using Domain.Enums;

namespace Application.Features.Apartments.Commands.AddMeterReading;

public record AddMeterReadingCommand(
    Guid ApartmentId,
    MeterType MeterType,
    decimal Value,
    DateOnly ReadingDate,
    bool IsNewMeter) : IRequest<EntityCreatedResponse>;
