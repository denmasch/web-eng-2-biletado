using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Error;

/// <summary>
/// An error response entry.
/// </summary>
public class Error
{
    /// <summary>
    /// The error code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code { get; set; }
    
    /// <summary>
    /// The error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    /// <summary>
    /// Additional information about the error.
    /// </summary>
    [JsonPropertyName("more_info")]
    public string MoreInfo { get; set; }
}