using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Status;

public class ApiStatus
{
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; }
    
    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; }
    
}