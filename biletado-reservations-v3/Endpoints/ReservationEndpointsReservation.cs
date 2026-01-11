using System.Diagnostics;
using biletado_reservations_v3.Models.Reservation;
using biletado_reservations_v3.Data;
using biletado_reservations_v3.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace biletado_reservations_v3.Endpoints;

public static class ReservationEndpointsReservation
{
    public static void MapReservationEndpointsReservations(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v3/reservations/reservations").WithTags("Reservations");
        
        group.MapGet("", async (
            ReservationDbContext db,
            bool? includeDeleted,
            Guid? roomId,
            DateOnly? before,
            DateOnly? after
        ) =>
        {
            Log.Information("Getting reservations");
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
            IReservationValidator validator,
            CancellationToken cancellationToken
        ) =>
        {
            Log.Information("Creating reservation");
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            var client = httpClientFactory.CreateClient("assets");
            
            // validate reservation data
            var validationResult = await validator.ValidateNewAsync(from, to, room_id, db, client, cancellationToken);

            if (!validationResult.IsValid)
            {
                Log.Warning("Validation Failed");
                
                return Results.BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => new {
                        code = e.Code,
                        message = e.Message,
                        more_info = e.MoreInfo
                    }),
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

            Log.Information("Reservation created with ID {ReservationId}", reservation.Id);
            
            return Results.Created(
                location,
                reservation
            );
        });

        
        group.MapGet("{id}", async (ReservationDbContext db, Guid id) =>
        {
            Log.Information("Getting reservation with ID {ReservationId}", id);
            
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            var query = db.Reservations.AsQueryable();
            
            var reservation = await query.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null)
            {
                Log.Warning("Reservation with ID {ReservationId} not found", id);
                
                return Results.NotFound(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "bad_request",
                            message = "Reservation not found.",
                            more_info = "No reservation with the given id exists."
                        }
                    },
                    trace = traceId
                });
            }
            
            Log.Information("Reservation with ID {ReservationId} retrieved", id);
            
            return Results.Ok(reservation);
            
        });
        
        group.MapPut("{id}", async (ReservationDbContext db, 
            Guid id,
            ReservationReplacement body,
            IHttpClientFactory httpClientFactory,
            IReservationValidator validator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            Log.Information("Updating reservation with ID {ReservationId}", id);
            
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            var client = httpClientFactory.CreateClient("assets");
            
            var reservation = await db.Reservations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);
            
            bool isExisting = reservation != null;
            bool isDeleted = reservation?.DeletedAt != null;
            
            if (!isExisting)
            {
                Log.Information("Reservation with ID {ReservationId} not found", id);
                
                // validate new reservation data
                var validationNewResult = await validator.ValidateNewAsync(body.From, body.To, body.RoomId, db, client, cancellationToken);

                if (!validationNewResult.IsValid)
                {
                    Log.Warning("Validation Failed for new reservation");
                    
                    return Results.BadRequest(new
                    {
                        errors = validationNewResult.Errors.Select(e => new {
                            code = e.Code,
                            message = e.Message,
                            more_info = e.MoreInfo
                        }),
                        trace = traceId
                    });
                }
                
                Reservation newReservation = new Reservation
                {
                    Id = id,
                    From = body.From,
                    To = body.To,
                    RoomId = body.RoomId,
                    DeletedAt = body.DeletedAt
                };
                
                db.Reservations.Add(newReservation);
                await db.SaveChangesAsync();
                
                var location = $"/api/v3/reservations/reservations/{newReservation.Id}";

                Log.Information("Reservation created with ID {ReservationId}", newReservation.Id);
                
                return Results.Created(location, newReservation);
            }
            
            if (isExisting)
            {
                if (httpContext.User?.Identity?.IsAuthenticated != true)
                {
                    Log.Warning("Unauthorized attempt to update reservation with ID {ReservationId}", id);
                    return Results.Unauthorized();
                }
            }

            if (isDeleted && body.DeletedAt != null)
            {
                Log.Warning("Attempt to update a soft deleted reservation but deleted_at is not null", id);
                
                return Results.BadRequest(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "bad_request",
                            message = "Cannot update a deleted reservation.",
                            more_info = "To restore a deleted reservation, set 'deleted_at' to null."
                        }
                    },
                    trace = traceId
                });
            }
            
            // validate reservation data
            Debug.Assert(reservation != null, nameof(reservation) + " != null");
            var validationExistingResult = await validator.ValidateExistingAsync(body.From, body.To, body.RoomId, reservation.Id ,db, client, cancellationToken);

            if (!validationExistingResult.IsValid)
            {
                Log.Warning("Validation Failed for new reservation");
                
                return Results.BadRequest(new
                {
                    errors = validationExistingResult.Errors.Select(e => new {
                        code = e.Code,
                        message = e.Message,
                        more_info = e.MoreInfo
                    }),
                    trace = traceId
                });
            }
            
            reservation.From = body.From;
            reservation.To = body.To;
            reservation.RoomId = body.RoomId;
            reservation.DeletedAt = body.DeletedAt;

            await db.SaveChangesAsync();

            Log.Information("Reservation updated with ID {ReservationId}", reservation.Id);
            
            return Results.Ok(reservation);
            
        }).WithOpenApi(operation =>
        {
            operation.Security = new List<Microsoft.OpenApi.Models.OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "OAuth2"
                            }
                        },
                        new List<string>()
                    }
                }
            };
            return operation;
        });
        
        group.MapDelete("{id}", async (Guid id, ReservationDbContext db, [FromQuery] bool permanent = false) =>
        {
            Log.Information("Deleting reservation with ID {ReservationId}", id);
            
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            var reservation = await db.Reservations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);

            // reservation not found
            if (reservation == null)
            {
                Log.Warning("Reservation with ID {ReservationId} not found", id);
                
                return Results.NotFound(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "bad_request",
                            message = "Reservation not found.",
                            more_info = "No reservation with the given id exists."
                        }
                    },
                    trace = traceId
                });
            }
            
            // reservation is already soft-deleted and permanent delete is not requested
            if (reservation.DeletedAt != null && !permanent)
            {
                Log.Warning("Reservation with ID {ReservationId} is already soft deleted", id);
                
                return Results.NotFound(new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "bad_request",
                            message = "Reservation is already soft deleted.",
                            more_info = "Reservation is already soft deleted and permanent delete is not requested."
                        }
                    },
                    trace = traceId
                });
            }
            
            if (permanent)
            {
                db.Reservations.Remove(reservation);
            }
            else
            {
                reservation.DeletedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
            
            Log.Information("Reservation with ID {ReservationId} deleted", id);
            
            return Results.NoContent();
        }).RequireAuthorization()
        .WithOpenApi(operation =>
        {
            operation.Security = new List<Microsoft.OpenApi.Models.OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "OAuth2"
                            }
                        },
                        new List<string>()
                    }
                }
            };
            return operation;
        });
    }
}