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

MockDataSeeder.Seed(app.Services.GetRequiredService<SensorStore>());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

// ============================================
// PART 1: SENSOR MANAGEMENT ENDPOINTS
// ============================================

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

// ============================================
// PART 2: GAMIFICATION ENDPOINTS
// ============================================

// Get gamification statistics (health score, stability streak, quick response rate)
app.MapGet("/api/gamification/stats", (SensorStore store) =>
{
    return Results.Ok(store.GetGamificationStats());
});

// Get alert history (for the gamification dashboard)
app.MapGet("/api/gamification/alerts", (SensorStore store) =>
{
    return Results.Ok(store.GetAlertHistory());
});

// Mark a critical alert as resolved (simulates operator intervention)
app.MapPost("/api/gamification/resolve/{mac}", (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    if (sensor == null)
        return Results.NotFound(new { message = "Sensor not found." });

    // Force a re-ingest with the same value to trigger resolution logic
    // The Ingest() method will detect the severity change and handle resolution tracking
    if (sensor.LastReading.HasValue)
    {
        store.Ingest(new TelemetryIngestRequest
        {
            DeviceMacAddress = mac,
            Value = sensor.LastReading.Value
        });
        return Results.Ok(new
        {
            message = "Alert resolution attempted. Sensor status updated.",
            mac = mac
        });
    }

    return Results.BadRequest(new { message = "Sensor has no reading to resolve." });
});

// ============================================
// PART 3: RECURSIVE DEPLOYMENT VALIDATION ENDPOINT
// ============================================

// Validate a nested device deployment tree using recursion
app.MapPost("/api/deployment/validate", (DeploymentNode root) =>
{
    if (root == null)
        return Results.BadRequest(new { message = "Invalid deployment tree." });

    var isValid = root.ValidateHierarchy();
    var invalidPath = root.FindFirstInvalidPath();

    return Results.Ok(new
    {
        isValid = isValid,
        message = isValid ? "All nodes are properly configured." : "Invalid configuration detected.",
        invalidPath = invalidPath != null ? string.Join(" -> ", invalidPath) : null,
        nodeCount = CountNodes(root)
    });
});

// Helper function to count nodes in the deployment tree
static int CountNodes(DeploymentNode node)
{
    int count = 1;
    foreach (var child in node.Children)
    {
        count += CountNodes(child);
    }
    return count;
}

app.Run();