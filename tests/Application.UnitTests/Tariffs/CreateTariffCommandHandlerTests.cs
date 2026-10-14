using System.Linq.Expressions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Tariffs.Commands.CreateTariff;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using Moq;

namespace Application.UnitTests.Tariffs;

public class CreateTariffCommandHandlerTests
{
    private readonly Mock<IRepository<Tariff>> _tariffs = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateTariffCommandHandler _handler;

    public CreateTariffCommandHandlerTests()
    {
        _handler = new CreateTariffCommandHandler(_tariffs.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithoutConflicts_AddsTariffAndCommits()
    {
        SetupExisting(new Tariff
        {
            ServiceType = ServiceType.HotWater, ChargeBasis = ChargeBasis.MeterConsumption,
            PricePerUnit = 100, ValidFrom = new DateOnly(2026, 1, 1), ValidTo = new DateOnly(2026, 6, 30)
        });
        var command = new CreateTariffCommand(
            ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, new DateOnly(2026, 7, 1), null);

        var response = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        _tariffs.Verify(r => r.AddAsync(
            It.Is<Tariff>(t => t.Id == response.Id && t.PricePerUnit == 110 && t.ValidTo == null),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOverlappingExistingTariff_ThrowsAndDoesNotSave()
    {
        SetupExisting(new Tariff
        {
            ServiceType = ServiceType.HotWater, ChargeBasis = ChargeBasis.MeterConsumption,
            PricePerUnit = 100, ValidFrom = new DateOnly(2026, 1, 1)
        });
        var command = new CreateTariffCommand(
            ServiceType.HotWater, ChargeBasis.MeterConsumption, 110, new DateOnly(2026, 7, 1), null);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));

        _tariffs.Verify(r => r.AddAsync(It.IsAny<Tariff>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceUsesDifferentChargeBasis_Throws()
    {
        SetupExisting(new Tariff
        {
            ServiceType = ServiceType.WasteCollection, ChargeBasis = ChargeBasis.PerResident,
            PricePerUnit = 45, ValidFrom = new DateOnly(2026, 1, 1), ValidTo = new DateOnly(2026, 12, 31)
        });
        var command = new CreateTariffCommand(
            ServiceType.WasteCollection, ChargeBasis.Area, 1, new DateOnly(2027, 1, 1), null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("different basis", ex.Message);
    }

    private void SetupExisting(params Tariff[] existing) =>
        _tariffs
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tariff, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
}
