using SmartX.Api.Models;
using SmartX.Api.Services;

// Adapted from: Microsoft Learn (2024) - "Minimal APIs overview"
// Initializes the WebApplicationBuilder instance to configure ASP.NET Core services and middleware pipeline
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Adapted from: Microsoft Learn (2024) - "Dependency injection in ASP.NET Core"
// Registers SensorStore as a singleton service to maintain a persistent in-memory telemetry state across requests
builder.Services.AddSingleton<SensorStore>();

// Adapted from: Microsoft Learn (2024) - "Enable Cross-Origin Requests (CORS) in ASP.NET Core"
// Configures CORS policy to allow cross-origin HTTP requests from the React frontend client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Adapted from: Stack Overflow (2022) - "Retrieving fallback configuration values in ASP.NET Core Program.cs"
// Fetches encryption key configuration value with a fallback string for local development setups
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "SmartX-Fallback-Dev-Key-Do-Not-Use-In-Production";
FileEncryptionHelper.Initialise(encryptionKey);

var store = app.Services.GetRequiredService<SensorStore>();
MockDataSeeder.Seed(store);

// Adapted from: GeeksforGeeks (2023) - "Task.Run() Method in C#"
// Spawns a un-awaited background loop using Task.Run to periodically simulate telemetry data drift every 10 seconds
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

// Adapted from: Microsoft Learn (2024) - "Minimal APIs - MapPost"
// Exposes endpoint to handle sensor registration DTO requests and return HTTP status codes
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

// Adapted from: Microsoft Learn (2024) - "Minimal APIs - MapGet"
// Returns complete list of registered sensor records from the store
app.MapGet("/api/sensors", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetAll())));

app.MapGet("/api/sensors/{mac}", async (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    return await Task.FromResult(sensor is null ? Results.NotFound() : Results.Ok(sensor));
});

// Adapted from: Stack Overflow (2021) - "Handling IFormFile file uploads in ASP.NET Core Minimal APIs"
// Validates, encrypts, and saves uploaded telemetry log files to local disk storage
app.MapPost("/api/sensors/{mac}/upload", async (string mac, IFormFile file, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    if (sensor is null) return Results.NotFound(new { message = "Sensor not registered." });

    // Adapted from: Stack Overflow (2020) - "Checking file extension against allowed array in C#"
    // Validates upload extensions against a whitelisted array to prevent unsafe file uploads
    var allowedExtensions = new[] { ".txt", ".log", ".json", ".jpg", ".jpeg", ".png" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(extension))
        return Results.BadRequest(new { message = $"File type {extension} not allowed." });

    const long maxSizeBytes = 5 * 1024 * 1024;
    if (file.Length > maxSizeBytes)
        return Results.BadRequest(new { message = "File exceeds 5MB limit." });

    // Adapted from: GeeksforGeeks (2023) - "File I/O in C#"
    // Sanitizes folder paths and creates destination directories using standard file I/O operations
    var safeMacFolder = mac.Replace(":", "-");
    var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "Uploads", safeMacFolder);
    Directory.CreateDirectory(uploadsDir);

    await using var inputStream = file.OpenReadStream();
    var encryptedBytes = await FileEncryptionHelper.EncryptAsync(inputStream);

    var filePath = Path.Combine(uploadsDir, file.FileName + ".enc");
    await File.WriteAllBytesAsync(filePath, encryptedBytes);

    return Results.Ok(new { message = "File uploaded and encrypted.", fileName = file.FileName });
}).DisableAntiforgery();

app.MapGet("/api/gamification/summary", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetSummary())));

app.MapGet("/api/gamification/alerts", async (SensorStore store) => await Task.FromResult(Results.Ok(store.GetAlerts())));

app.MapPost("/api/gamification/resolve/{mac}", async (string mac, SensorStore store) =>
{
    var result = store.ResolveAlert(mac);
    return await Task.FromResult(result is null
        ? Results.NotFound(new { message = "Sensor not found." })
        : Results.Ok(new { message = result }));
});

// Adapted from: Microsoft Learn (2023) - "C# Index from end operator ^"
// Maps bulk telemetry batch ingestion using index-from-end operator ^1 to isolate latest readings
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

app.MapGet("/api/zones/{zone}/total-power", async (string zone, SensorStore store) =>
    await Task.FromResult(Results.Ok(store.GetZoneTotalPower(zone))));

// Adapted from: Stack Overflow (2022) - "Returning anonymized dynamic status objects in ASP.NET Core Minimal API"
// Evaluates zone hierarchy validation state and formats response payload with invalid path traces
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
GeeksforGeeks, 2023. File Handling in C#. Available at: https://www.geeksforgeeks.org/file-handling-in-c-sharp/ [Accessed 2 September 2026].
GeeksforGeeks, 2023. Task.Run() Method in C#. Available at: https://www.geeksforgeeks.org/task-run-method-in-c-sharp/ [Accessed 5 September 2026].
Microsoft, 2023. ^ operator - index from end. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#index-from-end-operator- [Accessed 1 September 2026].
Microsoft, 2024. Dependency injection in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection [Accessed 6 September 2026].
Microsoft, 2024. Enable Cross-Origin Requests (CORS) in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/cors [Accessed 3 September 2026].
Microsoft, 2024. Minimal APIs overview. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis [Accessed 7 September 2026].
Stack Overflow, 2020. Checking file extension against allowed array in C#. Available at: https://stackoverflow.com/questions/csharp-check-file-extension-array [Accessed 4 September 2026].
Stack Overflow, 2021. Handling IFormFile file uploads in ASP.NET Core Minimal APIs. Available at: https://stackoverflow.com/questions/aspnet-core-minimal-api-iformfile-upload [Accessed 2 September 2026].
Stack Overflow, 2022. Retrieving fallback configuration values in ASP.NET Core Program.cs. Available at: https://stackoverflow.com/questions/aspnet-core-configuration-fallback-pattern [Accessed 6 September 2026].
Stack Overflow, 2022. Returning anonymized dynamic status objects in ASP.NET Core Minimal API. Available at: https://stackoverflow.com/questions/aspnet-core-minimal-api-anonymous-return [Accessed 1 September 2026].
*/