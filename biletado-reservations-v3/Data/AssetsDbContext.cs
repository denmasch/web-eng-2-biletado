using Microsoft.EntityFrameworkCore;

namespace biletado_reservations_v3.Data;

public class AssetsDbContext : DbContext
{
    public AssetsDbContext(DbContextOptions<AssetsDbContext> options)
        : base(options) { }
}