using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(IAppDbContext dbContext, IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

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
