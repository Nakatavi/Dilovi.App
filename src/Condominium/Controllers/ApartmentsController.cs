using Application.Features.Apartments.Commands.AddAdditionalCharge;
using Application.Features.Apartments.Commands.AddMeterReading;
using Application.Features.Apartments.Commands.CreateApartment;
using Application.Features.Payments.Queries.CalculatePayment;
using Application.Features.Responses;
using Condominium.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Condominium.Controllers;

[Authorize]
[Route("api/apartments")]
public class ApartmentsController : BaseController
{
    public ApartmentsController(IMediator mediator) : base(mediator)
    {
    }

    [Authorize(Roles = AdminRoles)]
    [HttpPost]
    [ProducesResponseType<EntityCreatedResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateApartmentCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = AdminRoles)]
    [HttpPost("{apartmentId:guid}/meter-readings")]
    [ProducesResponseType<EntityCreatedResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMeterReading(Guid apartmentId, [FromBody] AddMeterReadingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddMeterReadingCommand(
            apartmentId, request.MeterType, request.Value, request.ReadingDate, request.IsNewMeter);

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = AdminRoles)]
    [HttpPost("{apartmentId:guid}/additional-charges")]
    [ProducesResponseType<EntityCreatedResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddAdditionalCharge(Guid apartmentId, [FromBody] AddAdditionalChargeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddAdditionalChargeCommand(
            apartmentId, request.Description, request.Amount, request.Year, request.Month);

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("{apartmentId:guid}/payments/{year:int}/{month:int}")]
    [ProducesResponseType<PaymentStatementDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculatePayment(Guid apartmentId, int year, int month,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new CalculatePaymentQuery(apartmentId, year, month), cancellationToken));
    }
}
