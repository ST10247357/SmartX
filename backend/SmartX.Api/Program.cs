using SmartX.Api.Models;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<SensorStore>();

// Allow the React dev server to call this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

// Register a new sensor.
app.MapPost("/api/sensors/register", (SensorRegistrationRequest request, SensorStore store) =>
{
    var record = store.Register(request);
    return Results.Created($"/api/sensors/{record.DeviceMacAddress}", record);
});

// Push a telemetry reading for an existing sensor.
app.MapPost("/api/telemetry", (TelemetryIngestRequest request, SensorStore store) =>
{
    var updated = store.Ingest(request);
    return updated is null
        ? Results.NotFound(new { message = "Sensor not registered." })
        : Results.Ok(updated);
});

// Get all registered sensors (used to populate the dashboard grid).
app.MapGet("/api/sensors", (SensorStore store) => Results.Ok(store.GetAll()));

// Get a single sensor by MAC address.
app.MapGet("/api/sensors/{mac}", (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    return sensor is null ? Results.NotFound() : Results.Ok(sensor);
});

// Upload a config file, deployment photo, or hardware log for a sensor.
app.MapPost("/api/sensors/{mac}/upload", async (string mac, IFormFile file, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    if (sensor is null) return Results.NotFound(new { message = "Sensor not registered." });

    var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "Uploads", mac);
    Directory.CreateDirectory(uploadsDir);

    var filePath = Path.Combine(uploadsDir, file.FileName);
    await using var stream = File.Create(filePath);
    await file.CopyToAsync(stream);

    return Results.Ok(new { message = "File uploaded.", fileName = file.FileName });
}).DisableAntiforgery();

app.Run();