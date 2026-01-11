using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Room;

public class Room
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } 
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    
    [JsonPropertyName("storey_id")]
    public Guid StoreyId { get; set; }
    
    [property: JsonPropertyName("deleted_at"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DeletedAt { get; set; }
}