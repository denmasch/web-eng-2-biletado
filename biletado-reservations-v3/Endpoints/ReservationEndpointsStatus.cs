using biletado_reservations_v3.Models.Reservation;
using biletado_reservations_v3.Data;
using biletado_reservations_v3.Models.Status;
using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Endpoints;

public static class ReservationEndpointsStatus
{
    public static void MapReservationEndpointsStatus(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v3/reservations");

        group.MapGet("/status", async () =>
            Results.Ok(new ApiStatus { Authors = new List<string> {"Devin Schnurr", "Jannik Metz"}, ApiVersion = "3.0.0"})
        );

        group.MapGet("/health", async () =>
            Results.Ok(new Health { Live = true, Ready = true })
        );

    }
}