using Application.Features.Responses;
using Application.Features.Tariffs.Commands.CreateTariff;
using Microsoft.AspNetCore.Authorization;

namespace Condominium.Controllers;

[Authorize(Roles = AdminRoles)]
[Route("api/tariffs")]
public class TariffsController : BaseController
{
    public TariffsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType<EntityCreatedResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateTariffCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
