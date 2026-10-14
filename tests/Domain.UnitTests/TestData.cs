using Domain.Enums;
using Domain.Models;

namespace Domain.UnitTests;

internal static class TestData
{
    public static readonly Guid ApartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static Apartment Apartment(decimal area = 54.3m, int residents = 3) => new()
    {
        Id = ApartmentId,
        Number = "12",
        Area = area,
        ResidentsCount = residents
    };

    public static MeterReading Reading(MeterType type, decimal value, string date, bool isNewMeter = false) => new()
    {
        Id = Guid.NewGuid(),
        ApartmentId = ApartmentId,
        MeterType = type,
        Value = value,
        ReadingDate = DateOnly.Parse(date),
        IsNewMeter = isNewMeter
    };

    public static MeterReading Cold(decimal value, string date, bool isNewMeter = false) =>
        Reading(MeterType.ColdWater, value, date, isNewMeter);

    public static MeterReading Hot(decimal value, string date, bool isNewMeter = false) =>
        Reading(MeterType.HotWater, value, date, isNewMeter);

    public static Tariff Tariff(ServiceType service, ChargeBasis basis, decimal price, string from, string? to = null) => new()
    {
        Id = Guid.NewGuid(),
        ServiceType = service,
        ChargeBasis = basis,
        PricePerUnit = price,
        ValidFrom = DateOnly.Parse(from),
        ValidTo = to is null ? null : DateOnly.Parse(to)
    };

    public static AdditionalCharge Charge(string description, decimal amount, int year, int month, Guid? apartmentId = null) => new()
    {
        Id = Guid.NewGuid(),
        ApartmentId = apartmentId ?? ApartmentId,
        Description = description,
        Amount = amount,
        Year = year,
        Month = month
    };
}
