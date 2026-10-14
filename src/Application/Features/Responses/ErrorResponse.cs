using FluentValidation.Results;

namespace Application.Features.Responses;

public class ErrorResponse : BaseResponse<string>
{
    public ErrorResponse(ErrorMessageLevel level, List<ApiError> errors)
        : base(null, errors, level)
    {
    }

    public ErrorResponse(ErrorMessageLevel level, IEnumerable<ValidationFailure> errors)
        : base(null, errors.Select(ToApiError).ToList(), level)
    {
    }

    private static ApiError ToApiError(ValidationFailure failure) => new()
    {
        Message = failure.ErrorMessage,
        Code = 400,
        Item = failure.PropertyName
    };
}
