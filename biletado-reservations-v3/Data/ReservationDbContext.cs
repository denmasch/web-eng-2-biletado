using biletado_reservations_v3.Models.Reservation;
using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Data;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options) { }

    public DbSet<Reservation> Reservations => Set<Reservation>();
}