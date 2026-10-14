using System.Linq.Expressions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Apartments.Commands.AddMeterReading;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Moq;

namespace Application.UnitTests.Apartments;

public class AddMeterReadingCommandHandlerTests
{
    private static readonly Guid ApartmentId = Guid.NewGuid();

    private readonly Mock<IRepository<Apartment>> _apartments = new();
    private readonly Mock<IRepository<MeterReading>> _readings = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AddMeterReadingCommandHandler _handler;

    public AddMeterReadingCommandHandlerTests()
    {
        _apartments
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Apartment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new AddMeterReadingCommandHandler(_apartments.Object, _readings.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidReading_AddsItAndCommits()
    {
        SetupExisting(Reading(100, 2026, 7, 31));

        var response = await _handler.Handle(Command(112.5m, 2026, 8, 31), CancellationToken.None);

        _readings.Verify(r => r.AddAsync(
            It.Is<MeterReading>(m => m.Id == response.Id && m.Value == 112.5m && m.ApartmentId == ApartmentId),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApartmentDoesNotExist_ThrowsNotFound()
    {
        _apartments
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Apartment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(Command(10, 2026, 8, 31), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenValueIsLowerThanLatestReading_Throws()
    {
        SetupExisting(Reading(90, 2026, 6, 30), Reading(100, 2026, 7, 31));

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(Command(95, 2026, 8, 31), CancellationToken.None));

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNewMeterHasLowerValue_Succeeds()
    {
        SetupExisting(Reading(100, 2026, 7, 31));

        await _handler.Handle(Command(0, 2026, 8, 20, isNewMeter: true), CancellationToken.None);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReadingForSameDayExists_Throws()
    {
        SetupExisting(Reading(100, 2026, 8, 31));

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(Command(105, 2026, 8, 31), CancellationToken.None));

        Assert.Contains("already exists", ex.Message);
    }

    private void SetupExisting(params MeterReading[] readings) =>
        _readings
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<MeterReading, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(readings);

    private static AddMeterReadingCommand Command(decimal value, int year, int month, int day, bool isNewMeter = false) =>
        new(ApartmentId, MeterType.ColdWater, value, new DateOnly(year, month, day), isNewMeter);

    private static MeterReading Reading(decimal value, int year, int month, int day) => new()
    {
        Id = Guid.NewGuid(),
        ApartmentId = ApartmentId,
        MeterType = MeterType.ColdWater,
        Value = value,
        ReadingDate = new DateOnly(year, month, day)
    };
}
