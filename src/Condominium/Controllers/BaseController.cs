

namespace Condominium.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator _mediator;
    protected BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }
}