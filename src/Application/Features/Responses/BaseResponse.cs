using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Application.Features.Responses;

public class BaseResponse<T> : IActionResult
{
    protected bool IsSuccess;

    public BaseResponse(T data, bool isSuccess = true)
    {
        Data = data;
        IsSuccess = isSuccess;
        Result = isSuccess ? Constants.Success : Constants.Error;
    }

    protected BaseResponse(T data, List<ApiError> errors, ErrorMessageLevel level, bool isSuccess = false)
    {
        Data = data;
        Errors = errors == null ? null : new ErrorContainer() { Errors = errors, Level = level };
        IsSuccess = isSuccess;
        Result = isSuccess ? Constants.Success : Constants.Error;
    }



    [JsonProperty(PropertyName = "result")]
    public string Result { get; private set; }

    [JsonProperty(PropertyName = "data")]
    public T Data { get; set; }

    [JsonProperty(PropertyName = "errors")]
    public ErrorContainer Errors { get; set; }
    public Task ExecuteResultAsync(ActionContext context)
    {
        if (IsSuccess)
        {
            return new OkObjectResult(this).ExecuteResultAsync(context);
        }
        else
        {
            return new BadRequestObjectResult(this).ExecuteResultAsync(context);
        }
    }
}
