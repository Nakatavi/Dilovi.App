using Microsoft.AspNetCore.Mvc;
// TODO: Please verify the correct namespace for the LoginUserCommand
using Application.Features.Users.Commands.LoginUser;

namespace Condominium.Controllers
{
    public class UserController: BaseController
    {
        public UserController(IMediator mediator): base(mediator)
        {            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
