using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Reservation;

public class ReservationCollection
{
    [JsonPropertyName("reservations")]
    public List<Reservation>? Reservations { get; set; }
}