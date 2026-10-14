using Application.Features.Responses;

namespace Condominium.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected const string AdminRoles = nameof(Domain.Enums.Role.SuperAdmin) + "," + nameof(Domain.Enums.Role.Admin);

    protected readonly IMediator _mediator;

    protected BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Maps the transport-agnostic application response to an HTTP result
    protected IActionResult ToActionResult<T>(BaseResponse<T> response) =>
        response.IsSuccess ? Ok(response) : BadRequest(response);
}
