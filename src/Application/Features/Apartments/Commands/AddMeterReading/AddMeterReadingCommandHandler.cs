using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Responses;
using Domain.Billing;
using Domain.Common;
using Domain.Models;

namespace Application.Features.Apartments.Commands.AddMeterReading;

public class AddMeterReadingCommandHandler : IRequestHandler<AddMeterReadingCommand, EntityCreatedResponse>
{
    private readonly IRepository<Apartment> _apartments;
    private readonly IRepository<MeterReading> _meterReadings;
    private readonly IUnitOfWork _unitOfWork;

    public AddMeterReadingCommandHandler(
        IRepository<Apartment> apartments,
        IRepository<MeterReading> meterReadings,
        IUnitOfWork unitOfWork)
    {
        _apartments = apartments;
        _meterReadings = meterReadings;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityCreatedResponse> Handle(AddMeterReadingCommand request, CancellationToken cancellationToken)
    {
        if (!await _apartments.AnyAsync(a => a.Id == request.ApartmentId, cancellationToken))
        {
            throw new KeyNotFoundException($"Apartment {request.ApartmentId} was not found.");
        }

        var reading = new MeterReading
        {
            Id = Guid.NewGuid(),
            ApartmentId = request.ApartmentId,
            MeterType = request.MeterType,
            Value = request.Value,
            ReadingDate = request.ReadingDate,
            IsNewMeter = request.IsNewMeter
        };

        var existing = await _meterReadings.FindAsync(
            r => r.ApartmentId == request.ApartmentId && r.MeterType == request.MeterType,
            cancellationToken);

        if (existing.Any(r => r.ReadingDate == reading.ReadingDate && r.IsNewMeter == reading.IsNewMeter))
        {
            throw new DomainException(
                $"{reading.MeterType} reading for {reading.ReadingDate:yyyy-MM-dd} already exists.");
        }

        var latest = existing
            .OrderBy(r => r.ReadingDate)
            .ThenBy(r => r.IsNewMeter)
            .LastOrDefault();

        ConsumptionCalculator.EnsureCanFollow(latest, reading);

        await _meterReadings.AddAsync(reading, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new EntityCreatedResponse(reading.Id);
    }
}
