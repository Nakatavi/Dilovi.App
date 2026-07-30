
namespace Application.Features.Responses;

public class MultiRequestResponse<T> : BaseResponse<IEnumerable<T>>
{
    public MultiRequestResponse(IEnumerable<T> data, List<ApiError> errors, ErrorMessageLevel level) : base(data, errors, level, data != null)
    {
    }
}