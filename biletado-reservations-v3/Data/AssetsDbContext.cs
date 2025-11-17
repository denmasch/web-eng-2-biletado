using Microsoft.EntityFrameworkCore;
using biletado_reservations_v3.Models.Assets;

namespace biletado_reservations_v3.Data;

public class AssetsDbContext : DbContext
{
    public AssetsDbContext(DbContextOptions<AssetsDbContext> options)
        : base(options) { }
        
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Storey> Storeys => Set<Storey>();
    public DbSet<Room> Rooms => Set<Room>();
}