using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Responses;
using Domain.Common;
using Domain.Models;

namespace Application.Features.Apartments.Commands.CreateApartment;

public class CreateApartmentCommandHandler : IRequestHandler<CreateApartmentCommand, EntityCreatedResponse>
{
    private readonly IRepository<Apartment> _apartments;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApartmentCommandHandler(IRepository<Apartment> apartments, IUnitOfWork unitOfWork)
    {
        _apartments = apartments;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityCreatedResponse> Handle(CreateApartmentCommand request, CancellationToken cancellationToken)
    {
        var number = request.Number.Trim();

        if (await _apartments.AnyAsync(a => a.Number == number, cancellationToken))
        {
            throw new DomainException($"Apartment {number} already exists.");
        }

        var apartment = new Apartment
        {
            Id = Guid.NewGuid(),
            Number = number,
            Area = request.Area,
            ResidentsCount = request.ResidentsCount
        };

        await _apartments.AddAsync(apartment, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new EntityCreatedResponse(apartment.Id);
    }
}
