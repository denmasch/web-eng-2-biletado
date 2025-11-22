using biletado_reservations_v3.Data;
using biletado_reservations_v3.Models.Validation;

namespace biletado_reservations_v3.Service;

public interface IReservationValidator
{
    Task<ValidationResult> ValidateNewAsync(
        DateOnly from,
        DateOnly to,
        Guid roomId,
        ReservationDbContext db,
        HttpClient assetsClient,
        CancellationToken cancellationToken);
    
    Task<ValidationResult> ValidateExistingAsync(
        DateOnly from,
        DateOnly to,
        Guid roomId,
        Guid currentReservationId,
        ReservationDbContext db,
        HttpClient assetsClient,
        CancellationToken cancellationToken);
}