using SmartX.Api.Models;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<SensorStore>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

var store = app.Services.GetRequiredService<SensorStore>();
MockDataSeeder.Seed(store);

// Background simulator: pushes a new reading for every sensor every 10s,
// proving the data structures handle continuous load, not just one-off calls.
_ = Task.Run(async () =>
{
    var random = new Random();
    while (true)
    {
        await Task.Delay(10000);
        foreach (var sensor in store.GetAll())
        {
            var drift = (float)(random.NextDouble() * 10 - 5);
            var newValue = (sensor.LastReading ?? 50f) + drift;
            store.Ingest(new TelemetryIngestRequest
            {
                DeviceMacAddress = sensor.DeviceMacAddress,
                Value = newValue
            });
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

// --- Sensor management ---

app.MapPost("/api/sensors/register", (SensorRegistrationRequest request, SensorStore store) =>
{
    var record = store.Register(request);
    return Results.Created($"/api/sensors/{record.DeviceMacAddress}", record);
});

app.MapPost("/api/telemetry", (TelemetryIngestRequest request, SensorStore store) =>
{
    var updated = store.Ingest(request);
    return updated is null
        ? Results.NotFound(new { message = "Sensor not registered." })
        : Results.Ok(updated);
});

app.MapGet("/api/sensors", (SensorStore store) => Results.Ok(store.GetAll()));

app.MapGet("/api/sensors/{mac}", (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    return sensor is null ? Results.NotFound() : Results.Ok(sensor);
});

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

// --- Gamification: star ratings + alert resolution ---

app.MapGet("/api/gamification/summary", (SensorStore store) => Results.Ok(store.GetSummary()));

app.MapGet("/api/gamification/alerts", (SensorStore store) => Results.Ok(store.GetAlerts()));

app.MapPost("/api/gamification/resolve/{mac}", (string mac, SensorStore store) =>
{
    var result = store.ResolveAlert(mac);
    return result is null
        ? Results.NotFound(new { message = "Sensor not found." })
        : Results.Ok(new { message = result });
});

// --- Batch telemetry: demonstrates jagged array -> List<T> transfer ---

app.MapPost("/api/telemetry/batch", (List<DeviceBatch> batches, SensorStore store) =>
{
    var buffer = new HistoricalBatchBuffer(batches.Count);
    foreach (var batch in batches)
        buffer.AddDeviceBatch(batch.DeviceMacAddress, batch.Readings);

    var flattened = buffer.TransferToOptimisedList("batch-import", SensorCategory.Environmental);

    var processed = 0;
    foreach (var batch in batches)
    {
        if (batch.Readings.Length == 0) continue;
        store.Ingest(new TelemetryIngestRequest
        {
            DeviceMacAddress = batch.DeviceMacAddress,
            Value = batch.Readings[^1]
        });
        processed++;
    }

    return Results.Ok(new { message = $"Processed {processed} batches, {flattened.Count} readings.", totalReadings = flattened.Count });
});

app.MapGet("/api/zones/{zone}/total-power", (string zone, SensorStore store) =>
    Results.Ok(store.GetZoneTotalPower(zone)));

app.MapGet("/api/deployment/zone-status", (SensorStore store) =>
{
    var tree = store.BuildZoneTree();
    var isValid = tree.ValidateHierarchy();
    var invalidPath = tree.FindFirstInvalidPath();

    return Results.Ok(new
    {
        isValid,
        invalidPath = invalidPath != null ? string.Join(" -> ", invalidPath) : null
    });
});

app.Run();