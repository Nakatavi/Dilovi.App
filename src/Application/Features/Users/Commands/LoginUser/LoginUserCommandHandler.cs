using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // TODO: Replace with a proper password hashing and verification library (e.g., BCrypt.Net)
        if (user.PasswordHash != request.Password)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = new List<string> { user.Role.ToString() };
        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles);

        return new LoginUserResponse(token);
    }
}
