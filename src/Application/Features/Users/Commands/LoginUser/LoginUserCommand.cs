namespace Application.Features.Users.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password) : IRequest<LoginUserResponse>;
