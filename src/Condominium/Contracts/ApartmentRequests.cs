using Domain.Enums;

namespace Condominium.Contracts;

public record AddMeterReadingRequest(
    MeterType MeterType,
    decimal Value,
    DateOnly ReadingDate,
    bool IsNewMeter = false);

public record AddAdditionalChargeRequest(
    string Description,
    decimal Amount,
    int Year,
    int Month);
