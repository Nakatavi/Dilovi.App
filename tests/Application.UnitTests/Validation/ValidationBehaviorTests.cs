using Application.Common.Behaviors;
using Application.Features.Users.Commands.LoginUser;
using FluentValidation;

namespace Application.UnitTests.Validation;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsAndDoesNotCallHandler()
    {
        var behavior = new ValidationBehavior<LoginUserCommand, LoginUserResponse>([new LoginUserCommandValidator()]);
        var handlerCalled = false;

        var ex = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new LoginUserCommand("not-an-email", ""),
            _ =>
            {
                handlerCalled = true;
                return Task.FromResult(new LoginUserResponse("token"));
            },
            CancellationToken.None));

        Assert.False(handlerCalled);
        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(LoginUserCommand.Email));
        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(LoginUserCommand.Password));
    }

    [Fact]
    public async Task Handle_WithValidRequest_CallsHandler()
    {
        var behavior = new ValidationBehavior<LoginUserCommand, LoginUserResponse>([new LoginUserCommandValidator()]);

        var response = await behavior.Handle(
            new LoginUserCommand("admin@dilovi.test", "secret"),
            _ => Task.FromResult(new LoginUserResponse("token")),
            CancellationToken.None);

        Assert.Equal("token", response.Token);
    }
}
