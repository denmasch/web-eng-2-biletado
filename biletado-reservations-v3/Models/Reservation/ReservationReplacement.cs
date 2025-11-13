using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Reservation;

public class ReservationReplacement
{
    [JsonPropertyName("from")]
    public DateOnly From { get; set; }
    
    [JsonPropertyName("code")]
    public DateOnly To { get; set; }
    
    [JsonPropertyName("room_id")]
    public Guid RoomId { get; set; }
    
    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}