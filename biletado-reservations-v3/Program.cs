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

app.UseHttpsRedirection();

app.MapReservationEndpointsStatus();
app.MapReservationEndpointsReservations();

app.Run();
