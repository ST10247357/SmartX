using SmartX.Api.Models;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Adapted from Singleton Service Lifetime Pattern (Fowler, 2002; Microsoft, 2024a)
builder.Services.AddSingleton<SensorStore>();

// Adapted from ASP.NET Core CORS Middleware Configuration (Microsoft, 2024b)
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

// Adapted from Asynchronous Non-blocking Background Worker Tasks (Albahari & Albahari, 2021; GeeksforGeeks, 2023a)
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

// Adapted from Minimal API HTTP Post Route Mapping & Typed Results (Microsoft, 2024a)
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

// Adapted from Asynchronous Multipart Form File Handling & Direct Storage Operations (GeeksforGeeks, 2023b)
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

app.MapGet("/api/gamification/summary", (SensorStore store) => Results.Ok(store.GetSummary()));

app.MapGet("/api/gamification/alerts", (SensorStore store) => Results.Ok(store.GetAlerts()));

app.MapPost("/api/gamification/resolve/{mac}", (string mac, SensorStore store) =>
{
    var result = store.ResolveAlert(mac);
    return result is null
        ? Results.NotFound(new { message = "Sensor not found." })
        : Results.Ok(new { message = result });
});

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

/*
References:
Albahari, J. and Albahari, B., 2021. C# 10 in a Nutshell: The Definitive Reference. Sebastopol: O'Reilly Media.
Fowler, M., 2002. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley.
GeeksforGeeks, 2023a. Task.Run() Method in C#. GeeksforGeeks. Available at: https://www.geeksforgeeks.org/task-run-method-in-c-sharp/ [Accessed 10 September 2026].
GeeksforGeeks, 2023b. File I/O in C#. GeeksforGeeks. Available at: https://www.geeksforgeeks.org/file-handling-in-c-sharp/ [Accessed 10 September 2026].
Microsoft, 2024a. Minimal APIs overview. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis [Accessed 10 September 2026].
Microsoft, 2024b. Enable Cross-Origin Requests (CORS) in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/cors [Accessed 10 September 2026].
*/