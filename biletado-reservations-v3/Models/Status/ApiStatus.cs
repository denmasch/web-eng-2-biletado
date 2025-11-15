using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Status;

public class ApiStatus
{
    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; }
    
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; }
    
}