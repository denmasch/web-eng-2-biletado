using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Reservation;

public class Reservation
{
    
    [JsonPropertyName("id")]
    public Guid Id { get; set; } 
    
    [JsonPropertyName("from")]
    public DateOnly From { get; set; }
    
    [JsonPropertyName("to")]
    public DateOnly To { get; set; }
    
    [JsonPropertyName("room_id")]
    public Guid RoomId { get; set; }
    
    [property: JsonPropertyName("deleted_at"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DeletedAt { get; set; }
}