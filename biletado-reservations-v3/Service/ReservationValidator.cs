using System.Text.Json;
using biletado_reservations_v3.Data;
using biletado_reservations_v3.Models.Room;
using biletado_reservations_v3.Models.Validation;
using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Service;

public class ReservationValidator : IReservationValidator
{
    public async Task<ValidationResult> ValidateNewAsync(
        DateOnly from,
        DateOnly to,
        Guid roomId,
        ReservationDbContext db,
        HttpClient assetsClient,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        // Date validation
        if (from > to)
        {
            errors.Add(new ValidationError(
                "bad_request",
                "Invalid reservation dates.",
                "The 'from' date must be before the 'to' date."));
        }

        //Room exists
        if (!await RoomExistsAsync(assetsClient, roomId, cancellationToken))
        {
            errors.Add(new ValidationError(
                "not_found",
                "Room not found.",
                "No room with the given ID exists in the assets service."));
        }

        // Room is booked
        if (await RoomIsBookedAsync(db, roomId, from, to))
        {
            errors.Add(new ValidationError(
                "conflict",
                "The room is already booked.",
                "Please choose different dates or a different room."));
        }

        return errors.Count == 0
            ? ValidationResult.Ok()
            : ValidationResult.Fail(errors);
    }
    
    public async Task<ValidationResult> ValidateExistingAsync(
        DateOnly from,
        DateOnly to,
        Guid roomId,
        Guid currentReservationId,
        ReservationDbContext db,
        HttpClient assetsClient,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        // Date validation
        if (from > to)
        {
            errors.Add(new ValidationError(
                "bad_request",
                "Invalid reservation dates.",
                "The 'from' date must be before the 'to' date."));
        }

        //Room exists
        if (!await RoomExistsAsync(assetsClient, roomId, cancellationToken))
        {
            errors.Add(new ValidationError(
                "not_found",
                "Room not found.",
                "No room with the given ID exists in the assets service."));
        }

        // Room is booked
        if (await RoomIsBookedAsync(db, roomId, from, to, currentReservationId))
        {
            errors.Add(new ValidationError(
                "conflict",
                "The room is already booked.",
                "Please choose different dates or a different room."));
        }

        return errors.Count == 0
            ? ValidationResult.Ok()
            : ValidationResult.Fail(errors);
    }

    private static async Task<bool> RoomExistsAsync(HttpClient client, Guid room_id, CancellationToken ct)
    {
        try
        {
            using var resp = await client.GetAsync($"/api/v3/assets/rooms/{room_id}", ct);
            if (!resp.IsSuccessStatusCode)
                return false;

            var stream = await resp.Content.ReadAsStreamAsync(ct);

            var room = await JsonSerializer.DeserializeAsync<Room>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct
            );
            bool existingRoom = room != null && room.DeletedAt == null;
            return existingRoom;
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
    
    private static async Task<bool> RoomIsBookedAsync(ReservationDbContext db, Guid room_id, DateOnly from, DateOnly to, Guid excludeId)
    {
        var query = db.Reservations.AsQueryable();
        return await query.AnyAsync(r =>
            r.RoomId == room_id &&
            r.DeletedAt == null &&
            (r.Id != excludeId) &&
            (
                (from >= r.From && from < r.To) ||
                (to > r.From && to <= r.To) ||
                (from <= r.From && to >= r.To)
            )
        );
    }
}