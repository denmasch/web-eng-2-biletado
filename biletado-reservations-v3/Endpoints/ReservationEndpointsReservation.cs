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
            Guid room_id,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken
        ) =>
        {
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            // validate dates
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
            
            // validate room exists
            var client = httpClientFactory.CreateClient("assets");
            if (!await RoomExistsAsync(client, room_id, cancellationToken))
            {
                return Results.NotFound(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "not_found",
                            message = "Room not found.",
                            more_info = "No room with the given id exists in the assets service."
                        }
                    },
                    trace = traceId
                });
            }
            
            // check for conflicting reservations
            if (await RoomIsBookedAsync(db, room_id, from, to))
            {
                return Results.Conflict(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "conflict",
                            message = "The room is already booked for the selected dates.",
                            more_info = "Please choose different dates or a different room."
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
                RoomId = room_id,
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
    
    private static async Task<bool> RoomExistsAsync(HttpClient client, Guid room_id, CancellationToken ct)
    {
        try
        {
            using var resp = await client.GetAsync($"/api/v3/assets/rooms/{room_id}", ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> RoomIsBookedAsync(ReservationDbContext db, Guid room_id, DateOnly from, DateOnly to)
    {
        var query = db.Reservations.AsQueryable();
        return await query.AnyAsync(r =>
            r.RoomId == room_id &&
            r.DeletedAt == null &&
            (
                (from >= r.From && from < r.To) ||
                (to > r.From && to <= r.To) ||
                (from <= r.From && to >= r.To)
            )
        );
    }
}