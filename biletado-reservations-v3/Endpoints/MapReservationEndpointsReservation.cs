using biletado_reservations_v3.Models.Reservation;
using biletado_reservations_v3.Data;
using biletado_reservations_v3.Models.Status;
using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Endpoints;

public static class ReservationEndpointsReservation
{
    public static void MapReservationEndpointsReservations(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v3/reservations/reservations");
    }
}