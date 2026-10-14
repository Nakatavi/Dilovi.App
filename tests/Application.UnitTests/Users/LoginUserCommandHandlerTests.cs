using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Users.Commands.LoginUser;
using Domain.Enums;
using Domain.Models;
using Moq;

namespace Application.UnitTests.Users;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly LoginUserCommandHandler _handler;

    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "admin@dilovi.test",
        PasswordHash = "$2a$11$hash",
        Role = Role.Admin
    };

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(_users.Object, _tokenGenerator.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokenWithUserRole()
    {
        _users.Setup(r => r.GetUserByEmailAsync(_user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _passwordHasher.Setup(h => h.Verify("secret", _user.PasswordHash)).Returns(true);
        _tokenGenerator
            .Setup(g => g.GenerateToken(_user.Id, _user.Email, It.Is<IList<string>>(r => r.Single() == "Admin")))
            .Returns("jwt-token");

        var response = await _handler.Handle(new LoginUserCommand(_user.Email, "secret"), CancellationToken.None);

        Assert.Equal("jwt-token", response.Token);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsUnauthorized()
    {
        _users.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand("nobody@dilovi.test", "secret"), CancellationToken.None));

        _passwordHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorizedAndDoesNotIssueToken()
    {
        _users.Setup(r => r.GetUserByEmailAsync(_user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _passwordHasher.Setup(h => h.Verify("wrong", _user.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand(_user.Email, "wrong"), CancellationToken.None));

        _tokenGenerator.Verify(
            g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComparesPasswordThroughHasherNotPlainText()
    {
        // Stored hash equals the submitted password: must still be rejected if the hasher says no
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.c", PasswordHash = "secret", Role = Role.User };
        _users.Setup(r => r.GetUserByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("secret", "secret")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand(user.Email, "secret"), CancellationToken.None));
    }
}
