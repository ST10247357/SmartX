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

var encryptionKey = builder.Configuration["Encryption:Key"] ?? "SmartX-Fallback-Dev-Key-Do-Not-Use-In-Production";
FileEncryptionHelper.Initialise(encryptionKey);

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

app.MapPost("/api/sensors/register", async (SensorRegistrationRequest request, SensorStore store) =>
{
    var (record, error) = store.Register(request);
    return await Task.FromResult(error is not null
        ? Results.BadRequest(new { message = error })
        : Results.Created($"/api/sensors/{record!.DeviceMacAddress}", record));
});

app.MapPost("/api/telemetry", async (TelemetryIngestRequest request, SensorStore store) =>
{
    var updated = store.Ingest(request);
    return await Task.FromResult(updated is null
        ? Results.BadRequest(new { message = "Sensor not registered, or reading was negative." })
        : Results.Ok(updated));
});

app.MapGet("/api/sensors", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetAll())));

app.MapGet("/api/sensors/{mac}", async (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    return await Task.FromResult(sensor is null ? Results.NotFound() : Results.Ok(sensor));
});

app.MapPost("/api/sensors/{mac}/upload", async (string mac, IFormFile file, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    if (sensor is null) return Results.NotFound(new { message = "Sensor not registered." });

    var allowedExtensions = new[] { ".txt", ".log", ".json", ".jpg", ".jpeg", ".png" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(extension))
        return Results.BadRequest(new { message = $"File type {extension} not allowed." });

    const long maxSizeBytes = 5 * 1024 * 1024;
    if (file.Length > maxSizeBytes)
        return Results.BadRequest(new { message = "File exceeds 5MB limit." });

    // MAC addresses contain colons, which Windows treats as a drive-letter
    // separator in file paths - replace with a safe character for the folder name.
    var safeMacFolder = mac.Replace(":", "-");
    var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "Uploads", safeMacFolder);
    Directory.CreateDirectory(uploadsDir);

    await using var inputStream = file.OpenReadStream();
    var encryptedBytes = await FileEncryptionHelper.EncryptAsync(inputStream);

    var filePath = Path.Combine(uploadsDir, file.FileName + ".enc");
    await File.WriteAllBytesAsync(filePath, encryptedBytes);

    return Results.Ok(new { message = "File uploaded and encrypted.", fileName = file.FileName });
}).DisableAntiforgery();

// --- Gamification: star ratings + alert resolution ---

app.MapGet("/api/gamification/summary", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetSummary())));

app.MapGet("/api/gamification/alerts", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetAlerts())));

app.MapPost("/api/gamification/resolve/{mac}", async (string mac, SensorStore store) =>
{
    var result = store.ResolveAlert(mac);
    return await Task.FromResult(result is null
        ? Results.NotFound(new { message = "Sensor not found." })
        : Results.Ok(new { message = result }));
});

// --- Batch telemetry: demonstrates jagged array -> List<T> transfer ---

app.MapPost("/api/telemetry/batch", async (List<DeviceBatch> batches, SensorStore store) =>
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

    return await Task.FromResult(Results.Ok(new { message = $"Processed {processed} batches, {flattened.Count} readings.", totalReadings = flattened.Count }));
});

// --- Zone power totals (operator overloading) and deployment validation (recursion) ---

app.MapGet("/api/zones/{zone}/total-power", async (string zone, SensorStore store) =>
    await Task.FromResult(Results.Ok(store.GetZoneTotalPower(zone))));

app.MapGet("/api/deployment/zone-status", async (SensorStore store) =>
{
    var tree = store.BuildZoneTree();
    var isValid = tree.ValidateHierarchy();
    var invalidPath = tree.FindFirstInvalidPath();

    return await Task.FromResult(Results.Ok(new
    {
        isValid,
        invalidPath = invalidPath != null ? string.Join(" -> ", invalidPath) : null
    }));
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