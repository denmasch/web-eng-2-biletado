using biletado_reservations_v3.Data;
using biletado_reservations_v3.Endpoints;
using biletado_reservations_v3.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationConnection"))
);

// Read assets base URL from configuration (allows overriding via env var "Assets__BaseUrl")
var assetsBase = builder.Configuration["Assets:BaseUrl"] ?? "http://localhost:9090";

builder.Services.AddHttpClient("assets", client =>
{
    client.BaseAddress = new Uri(assetsBase);
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IReservationValidator, ReservationValidator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/rapidoc", async context =>
{
    var html = @"
<!DOCTYPE html>
<html>
  <head>
    <script type=""module""
      src=""https://unpkg.com/rapidoc/dist/rapidoc-min.js"">
    </script>
  </head>
  <body>
    <rapi-doc 
      spec-url=""/swagger/v1/swagger.json""
      render-style=""view""
      theme=""dark""
    ></rapi-doc>
  </body>
</html>";
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});


app.UseHttpsRedirection();

app.MapReservationEndpointsStatus();
app.MapReservationEndpointsReservations();

app.Run();
