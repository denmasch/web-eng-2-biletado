using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Status;

public class ApiStatus
{
    [JsonPropertyName("authors")]
    public required List<string> Authors { get; set; }
    
    [JsonPropertyName("api_version")]
    public required string ApiVersion { get; set; }
    
}