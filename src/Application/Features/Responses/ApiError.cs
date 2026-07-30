using Newtonsoft.Json;

namespace Application.Features.Responses;
public class ApiError
{
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }
    [JsonProperty(PropertyName = "code")]
    public int Code { get; set; }
    [JsonProperty(PropertyName = "item")]
    public string Item { get; set; }
    [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }
}
