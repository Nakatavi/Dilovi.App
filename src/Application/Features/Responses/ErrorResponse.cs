
namespace Application.Features.Responses;

public class ErrorResponse : BaseResponse<string>
{
    public ErrorResponse(ErrorMessageLevel level, List<ApiError> errors) : base(null, errors, level)
    {
    }

    public ErrorResponse(ErrorMessageLevel level, IEnumerable<FluentValidation.Results.ValidationFailure> errors) : base(null, false)
    {
        var err = errors.Select(x => new ApiError()
        {
            Message = x.ErrorMessage,
            Code = 400,
            Item = x.PropertyName
        }).ToList();

        Errors = new ErrorContainer()
        {
            Errors = err,
            Level = level,
        };
    }
}
