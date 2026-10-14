using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Responses;
using Domain.Billing;
using Domain.Common;
using Domain.Models;

namespace Application.Features.Tariffs.Commands.CreateTariff;

public class CreateTariffCommandHandler : IRequestHandler<CreateTariffCommand, EntityCreatedResponse>
{
    private readonly IRepository<Tariff> _tariffs;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTariffCommandHandler(IRepository<Tariff> tariffs, IUnitOfWork unitOfWork)
    {
        _tariffs = tariffs;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityCreatedResponse> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        var tariff = new Tariff
        {
            Id = Guid.NewGuid(),
            ServiceType = request.ServiceType,
            ChargeBasis = request.ChargeBasis,
            PricePerUnit = request.PricePerUnit,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        };

        var existing = await _tariffs.FindAsync(t => t.ServiceType == request.ServiceType, cancellationToken);

        if (existing.Any(t => t.ChargeBasis != tariff.ChargeBasis))
        {
            throw new DomainException($"{tariff.ServiceType} is already charged by a different basis.");
        }

        TariffSchedule.EnsureNoOverlaps([.. existing, tariff]);

        await _tariffs.AddAsync(tariff, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new EntityCreatedResponse(tariff.Id);
    }
}
