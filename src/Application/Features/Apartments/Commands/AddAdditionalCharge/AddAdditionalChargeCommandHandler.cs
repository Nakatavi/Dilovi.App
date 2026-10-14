using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Responses;
using Domain.Models;

namespace Application.Features.Apartments.Commands.AddAdditionalCharge;

public class AddAdditionalChargeCommandHandler : IRequestHandler<AddAdditionalChargeCommand, EntityCreatedResponse>
{
    private readonly IRepository<Apartment> _apartments;
    private readonly IRepository<AdditionalCharge> _additionalCharges;
    private readonly IUnitOfWork _unitOfWork;

    public AddAdditionalChargeCommandHandler(
        IRepository<Apartment> apartments,
        IRepository<AdditionalCharge> additionalCharges,
        IUnitOfWork unitOfWork)
    {
        _apartments = apartments;
        _additionalCharges = additionalCharges;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityCreatedResponse> Handle(AddAdditionalChargeCommand request, CancellationToken cancellationToken)
    {
        if (!await _apartments.AnyAsync(a => a.Id == request.ApartmentId, cancellationToken))
        {
            throw new KeyNotFoundException($"Apartment {request.ApartmentId} was not found.");
        }

        var charge = new AdditionalCharge
        {
            Id = Guid.NewGuid(),
            ApartmentId = request.ApartmentId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Year = request.Year,
            Month = request.Month
        };

        await _additionalCharges.AddAsync(charge, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new EntityCreatedResponse(charge.Id);
    }
}
