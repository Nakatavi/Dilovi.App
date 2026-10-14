using Application.Common.Interfaces.Repositories;
using Domain.Billing;
using Domain.Models;
using Domain.ValueObjects;

namespace Application.Features.Payments.Queries.CalculatePayment;

public class CalculatePaymentQueryHandler : IRequestHandler<CalculatePaymentQuery, PaymentStatementDto>
{
    private readonly IRepository<Apartment> _apartments;
    private readonly IRepository<MeterReading> _meterReadings;
    private readonly IRepository<Tariff> _tariffs;
    private readonly IRepository<AdditionalCharge> _additionalCharges;
    private readonly PaymentCalculator _paymentCalculator;

    public CalculatePaymentQueryHandler(
        IRepository<Apartment> apartments,
        IRepository<MeterReading> meterReadings,
        IRepository<Tariff> tariffs,
        IRepository<AdditionalCharge> additionalCharges,
        PaymentCalculator paymentCalculator)
    {
        _apartments = apartments;
        _meterReadings = meterReadings;
        _tariffs = tariffs;
        _additionalCharges = additionalCharges;
        _paymentCalculator = paymentCalculator;
    }

    public async Task<PaymentStatementDto> Handle(CalculatePaymentQuery request, CancellationToken cancellationToken)
    {
        var period = new BillingPeriod(request.Year, request.Month);
        var (start, end) = (period.Start, period.End);

        var apartment = await _apartments.GetByIdAsync(request.ApartmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Apartment {request.ApartmentId} was not found.");

        var meterReadings = await _meterReadings.FindAsync(
            r => r.ApartmentId == apartment.Id && r.ReadingDate <= end,
            cancellationToken);

        var tariffs = await _tariffs.FindAsync(
            t => t.ValidFrom <= end && (t.ValidTo == null || t.ValidTo >= start),
            cancellationToken);

        var additionalCharges = await _additionalCharges.FindAsync(
            c => c.ApartmentId == apartment.Id && c.Year == period.Year && c.Month == period.Month,
            cancellationToken);

        var statement = _paymentCalculator.Calculate(
            new BillingContext(apartment, period, meterReadings),
            tariffs,
            additionalCharges);

        return PaymentStatementDto.From(statement);
    }
}
