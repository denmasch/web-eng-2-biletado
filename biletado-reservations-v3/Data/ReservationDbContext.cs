using biletado_reservations_v3.Models.Reservation;
using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Data;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options) { }

    public DbSet<Reservation> Reservations => Set<Reservation>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Building mapping
        modelBuilder.Entity<Reservation>(r =>
        {
            r.ToTable("reservations");
            r.HasKey(x => x.Id);
            r.Property(x => x.Id).HasColumnName("id");
            r.Property(x => x.From).HasColumnName("from");
            r.Property(x => x.To).HasColumnName("to");
            r.Property(x => x.RoomId).HasColumnName("room_id");
            r.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });
    }
}