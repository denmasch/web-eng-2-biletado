using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Status;

public class Health
{
    /// <summary>
    /// if the service is in full operation mode
    /// </summary>
    [JsonPropertyName("live")]
    public bool Live { get; set; }
    
    /// <summary>
    /// if the service is healthy and can serve requests
    /// </summary>
    [JsonPropertyName("ready")]
    public bool Ready { get; set; }
}