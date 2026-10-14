using Application.Features.Apartments.Commands.AddAdditionalCharge;
using Application.Features.Apartments.Commands.AddMeterReading;
using Application.Features.Apartments.Commands.CreateApartment;
using Application.Features.Tariffs.Commands.CreateTariff;
using Domain.Enums;

namespace Application.UnitTests.Validation;

public class ValidatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 14);

    [Theory]
    [InlineData(ServiceType.ColdWater, ChargeBasis.MeterConsumption, true)]
    [InlineData(ServiceType.HotWater, ChargeBasis.MeterConsumption, true)]
    [InlineData(ServiceType.BuildingMaintenance, ChargeBasis.Area, true)]
    [InlineData(ServiceType.WasteCollection, ChargeBasis.PerResident, true)]
    [InlineData(ServiceType.ColdWater, ChargeBasis.Area, false)]
    [InlineData(ServiceType.WasteCollection, ChargeBasis.MeterConsumption, false)]
    public void CreateTariff_AllowsMeterBasisOnlyForWater(ServiceType service, ChargeBasis basis, bool isValid)
    {
        var command = new CreateTariffCommand(service, basis, 10, Today, null);

        Assert.Equal(isValid, new CreateTariffCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void CreateTariff_WithValidToBeforeValidFrom_IsInvalid()
    {
        var command = new CreateTariffCommand(
            ServiceType.ColdWater, ChargeBasis.MeterConsumption, 10, Today, Today.AddDays(-1));

        var result = new CreateTariffCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTariffCommand.ValidTo));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CreateTariff_WithNonPositivePrice_IsInvalid(decimal price)
    {
        var command = new CreateTariffCommand(ServiceType.ColdWater, ChargeBasis.MeterConsumption, price, Today, null);

        var result = new CreateTariffCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTariffCommand.PricePerUnit));
    }

    [Theory]
    [InlineData(0, true)]   // today
    [InlineData(-30, true)]
    [InlineData(1, false)]  // tomorrow
    public void AddMeterReading_RejectsFutureDates(int daysFromToday, bool isValid)
    {
        var validator = new AddMeterReadingCommandValidator(new FixedTimeProvider(new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero)));
        var command = new AddMeterReadingCommand(Guid.NewGuid(), MeterType.ColdWater, 10, Today.AddDays(daysFromToday), false);

        Assert.Equal(isValid, validator.Validate(command).IsValid);
    }

    [Fact]
    public void AddMeterReading_WithNegativeValue_IsInvalid()
    {
        var validator = new AddMeterReadingCommandValidator(TimeProvider.System);
        var command = new AddMeterReadingCommand(Guid.NewGuid(), MeterType.ColdWater, -1, new DateOnly(2026, 1, 1), false);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddMeterReadingCommand.Value));
    }

    [Theory]
    [InlineData("", 54.3, 3, "Number")]
    [InlineData("12", 0, 3, "Area")]
    [InlineData("12", 54.3, -1, "ResidentsCount")]
    public void CreateApartment_WithInvalidData_ReportsProperty(string number, decimal area, int residents, string property)
    {
        var result = new CreateApartmentCommandValidator().Validate(new CreateApartmentCommand(number, area, residents));

        Assert.Contains(result.Errors, e => e.PropertyName == property);
    }

    [Theory]
    [InlineData(200, 8, true)]
    [InlineData(-50, 8, true)] // discount
    [InlineData(0, 8, false)]
    [InlineData(200, 13, false)]
    public void AddAdditionalCharge_ValidatesAmountAndMonth(decimal amount, int month, bool isValid)
    {
        var command = new AddAdditionalChargeCommand(Guid.NewGuid(), "Roof repair fund", amount, 2026, month);

        Assert.Equal(isValid, new AddAdditionalChargeCommandValidator().Validate(command).IsValid);
    }
}
