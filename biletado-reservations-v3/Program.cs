using biletado_reservations_v3.Data;
using biletado_reservations_v3.Endpoints;
using biletado_reservations_v3.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// logging
var levelSwitch = new LoggingLevelSwitch();
var configuredLevel = builder.Configuration["LOG_LEVEL"];
if (!string.IsNullOrWhiteSpace(configuredLevel) &&
    Enum.TryParse<LogEventLevel>(configuredLevel, true, out var parsedLevel))
{
    levelSwitch.MinimumLevel = parsedLevel;
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.ControlledBy(levelSwitch)
    .CreateLogger();
Log.Information("application.starting env={Environment} argsCount={ArgsCount} minLevel={MinLevel}",
    builder.Environment.EnvironmentName,
    args.Length,
    levelSwitch.MinimumLevel);
builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddSingleton(levelSwitch);


builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReservationConnection"))
);

builder.Services.AddHttpClient("assets", client =>
{
    client.BaseAddress = new Uri("http://localhost:9090");
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{ 
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reservations API",
        Version = "v3",
        Description = "Reservations API for Biletado application"
    });
    
    options.AddSecurityDefinition("OAuth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
        {
            AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    "http://localhost:9090/auth/realms/biletado/protocol/openid-connect/auth"),
                TokenUrl = new Uri(
                    "http://localhost:9090/auth/realms/biletado/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>()
            }
        }
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "biletado"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddScoped<IReservationValidator, ReservationValidator>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:9090/auth/realms/biletado";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = c =>
            {
                Console.WriteLine($"Authentication Failure: {c.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = c =>
            {
                Console.WriteLine($"Token valid for: {c.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

Log.Information("application.built env={Environment}", app.Environment.EnvironmentName);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("angular");
        options.OAuthUsePkce();
    });

    app.MapGet("/oauth-receiver.html", async context =>
    {
        var html = @"<!doctype html>
<head>
  <script type=""module"" src=""https://unpkg.com/rapidoc/dist/rapidoc-min.js""></script>
</head>

<body>
  <oauth-receiver> </oauth-receiver>
</body>";
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    });

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
      oauth2-redirect-url=""http://localhost:7033/oauth-receiver.html""
      render-style=""view""
      theme=""dark""
    ></rapi-doc>
  </body>
</html>";
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    });
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReservationEndpointsStatus();
app.MapReservationEndpointsReservations();

Log.Information("application.startup_complete env={Environment}", app.Environment.EnvironmentName);

app.Run();
