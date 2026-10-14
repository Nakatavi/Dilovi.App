using Application.Features.Users.Commands.LoginUser;
using Microsoft.AspNetCore.Authorization;

namespace Condominium.Controllers;

[Route("api/users")]
public class UserController : BaseController
{
    public UserController(IMediator mediator) : base(mediator)
    {
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
