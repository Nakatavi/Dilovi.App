
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Application.Features.Responses;

public class ErrorContainer
{
    [JsonProperty(PropertyName = "level")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorMessageLevel Level { get; set; }

    [JsonProperty(PropertyName = "errors")]
    public List<ApiError> Errors { get; set; }
}
