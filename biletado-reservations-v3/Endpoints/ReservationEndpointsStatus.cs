using System.Diagnostics;
using System.Text.Json;
using biletado_reservations_v3.Models.Reservation;
using biletado_reservations_v3.Data;
using biletado_reservations_v3.Models.Status;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace biletado_reservations_v3.Endpoints;

public static class ReservationEndpointsStatus
{
    public static void MapReservationEndpointsStatus(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v3/reservations").WithTags("Status");

        group.MapGet("/status", async () =>
            {
                Log.Information("Status endpoint called");

                return Results.Ok(new ApiStatus
                    { Authors = new List<string> { "Devin Schnurr", "Jannik Metz" }, ApiVersion = "3.0.0" });
            }
        );

        group.MapGet("/health", async (ReservationDbContext reservationsDb, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        {
            Log.Information("Health endpoint called");
            
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            
            var client = httpClientFactory.CreateClient("assets");
            bool assetsConnected = await CheckExternalApiHealthAsync(client, "/api/v3/assets/health", cancellationToken);
            
            var reservationsConnected = await CheckDatabaseAsync(reservationsDb);
            
            if (!assetsConnected || !reservationsConnected)
            {
                Log.Warning("Database or external API unreachable");
                
                return Results.Json( new
                {
                    errors = new[]
                    {
                        new
                        {
                            code = "database_unreachable",
                            message = "One or more databases are not reachable.",
                            more_info = "Check connection strings or database availability."
                        }
                    },
                    trace = traceId
                }, statusCode: 503);
            }
            
            Log.Information("Health check passed");

            return Results.Ok(new 
                { 
                    live = true,
                    ready = true,
                    databases = new
                    {
                        reservations = new
                        {
                            connected = reservationsConnected,
                        }
                    }
                });
        }
        );
        
        group.MapGet("/health/live", () =>
            Results.Ok(new  { live = true })
        );

        group.MapGet("/health/ready", async (ReservationDbContext reservationsDb, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
            {
                Log.Information("Readiness endpoint called");
                
                var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
                
                var client = httpClientFactory.CreateClient("assets");
                bool assetsConnected = await CheckExternalApiHealthAsync(client, "/api/v3/assets/health", cancellationToken);
                
                var reservationsConnected = await CheckDatabaseAsync(reservationsDb);
                
                if (!assetsConnected || !reservationsConnected)
                {
                    Log.Warning("Database or external API unreachable");
                    return Results.Json( new
                    {
                        errors = new[]
                        {
                            new
                            {
                                code = "database_unreachable",
                                message = "One or more databases are not reachable.",
                                more_info = "Check connection strings or database availability."
                            }
                        },
                        trace = traceId
                    }, statusCode: 503);
                }
                Log.Information("Readiness check passed");
                return Results.Ok(new { ready = true });
            }
        );
    }
    
    private static async Task<bool> CheckDatabaseAsync(DbContext db)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1;");
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    static async Task<bool> CheckExternalApiHealthAsync(HttpClient client, string path, CancellationToken ct)
    {
        try
        {
            using var resp = await client.GetAsync(path, ct);
            if (!resp.IsSuccessStatusCode) return false;

            var content = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(content)) return true;

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("live", out var liveProp) &&
                root.TryGetProperty("ready", out var readyProp))
            {
                bool live = liveProp.ValueKind == JsonValueKind.True;
                bool ready = readyProp.ValueKind == JsonValueKind.True;
                return live && ready;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
