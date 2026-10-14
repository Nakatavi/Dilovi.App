using System.Text.Json.Serialization;

namespace Application.Features.Responses;

public class BaseResponse<T>
{
    public BaseResponse(T? data, bool isSuccess = true)
    {
        Data = data;
        IsSuccess = isSuccess;
        Result = isSuccess ? Constants.Success : Constants.Error;
    }

    protected BaseResponse(T? data, List<ApiError>? errors, ErrorMessageLevel level, bool isSuccess = false)
        : this(data, isSuccess)
    {
        Errors = errors is null ? null : new ErrorContainer { Errors = errors, Level = level };
    }

    [JsonIgnore]
    public bool IsSuccess { get; }

    [JsonPropertyName("result")]
    public string Result { get; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorContainer? Errors { get; set; }
}
