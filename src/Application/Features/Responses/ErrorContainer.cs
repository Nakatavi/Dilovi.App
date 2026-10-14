using System.Text.Json.Serialization;

namespace Application.Features.Responses;

public class ErrorContainer
{
    [JsonPropertyName("level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorMessageLevel Level { get; set; }

    [JsonPropertyName("errors")]
    public List<ApiError> Errors { get; set; } = [];
}
