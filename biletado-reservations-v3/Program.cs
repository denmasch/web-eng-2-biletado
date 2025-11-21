using biletado_reservations_v3.Data;
using biletado_reservations_v3.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationConnection"))
);

builder.Services.AddHttpClient("assets", client =>
{
    client.BaseAddress = new Uri("http://localhost:9090");
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
