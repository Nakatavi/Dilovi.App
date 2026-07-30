

namespace Condominium.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }
}