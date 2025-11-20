using System.Diagnostics;
using System.Security.Claims;
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
        
        group.MapGet("", async (
            ReservationDbContext db,
            bool? includeDeleted,
            Guid? roomId,
            DateOnly? before,
            DateOnly? after
        ) =>
        {
            var query = db.Reservations.AsQueryable();

            if (includeDeleted is not true)
            {
                query = query.Where(r => r.DeletedAt == null);
            }

            if (roomId is Guid roomIdValue)
            {
                query = query.Where(r => r.RoomId == roomIdValue);
            }

            if (before is DateOnly beforeValue)
            {
                query = query.Where(r => r.From <= beforeValue);
            }

            if (after is DateOnly afterValue)
            {
                query = query.Where(r => r.To >= afterValue);
            }

            var result = await query.ToListAsync();

            return Results.Ok(new
            {
                reservations = result
            });
        });
        
        group.MapPost("", async (
            ReservationDbContext db,
            DateOnly from,
            DateOnly to,
            Guid roomId
        ) =>
        {
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            //TODO: Check room existence via Assets API and if the room is already booked for the given dates
            
            if (from > to || from < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return Results.BadRequest(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "bad_request",
                            message = "Invalid reservation dates.",
                            more_info = "The 'from' date must be before the 'to' date and cannot be in the past."
                        }
                    },
                    trace = traceId
                });
            }

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                From = from,
                To = to,
                RoomId = roomId,
                DeletedAt = null
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();

            var location = $"/api/v3/reservations/reservations/{reservation.Id}";

            return Results.Created(
                location,
                reservation
            );
        });

        
        group.MapGet("{id}", async (Guid id, ReservationDbContext reservationDb) =>
        {
            
        });
        
        group.MapPut("{id}", async (Guid id, ReservationDbContext reservationDb) =>
        {
            
        });
        
        group.MapDelete("{id}", async (Guid id, ReservationDbContext reservationDb) =>
        {
            
        });
    } 
}