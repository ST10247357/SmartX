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

Console.WriteLine("🚀 Starting Real-World Telemetry Simulator...");

_ = Task.Run(async () =>
{
    var random = new Random();
    var baselines = new Dictionary<string, float>();
    
    Console.WriteLine("📡 Telemetry Simulator Started - Realistic data every 10 seconds");
    
    while (true)
    {
        try
        {
            await Task.Delay(10000);
            
            var sensors = store.GetAll();
            if (sensors.Count == 0) continue;
            
            foreach (var sensor in sensors)
            {
                if (!baselines.ContainsKey(sensor.DeviceMacAddress))
                {
                    baselines[sensor.DeviceMacAddress] = sensor.LastReading ?? 50f;
                }
            }
            
            foreach (var sensor in sensors)
            {
                float newValue;
                float currentBaseline = baselines[sensor.DeviceMacAddress];
                
                switch (sensor.Category)
                {
                    case SensorCategory.Environmental:
                        float drift = (float)(random.NextDouble() * 6 - 3);
                        newValue = currentBaseline + drift;
                        newValue = Math.Clamp(newValue, 5, 95);
                        break;
                        
                    case SensorCategory.PowerConsumption:
                        float powerDrift = (float)(random.NextDouble() * 100 - 50);
                        newValue = currentBaseline + powerDrift;
                        newValue = Math.Clamp(newValue, 100, 5000);
                        break;
                        
                    case SensorCategory.Actuator:
                        if (random.NextDouble() > 0.9)
                        {
                            newValue = currentBaseline > 0.5 ? 0 : 1;
                        }
                        else
                        {
                            newValue = currentBaseline;
                        }
                        break;
                        
                    default:
                        newValue = currentBaseline + (float)(random.NextDouble() * 10 - 5);
                        break;
                }
                
                baselines[sensor.DeviceMacAddress] = newValue;
                
                store.Ingest(new TelemetryIngestRequest
                {
                    DeviceMacAddress = sensor.DeviceMacAddress,
                    Value = newValue
                });
            }
            
            var criticalCount = sensors.Count(s => s.CurrentSeverity == SeverityLevel.Critical);
            var warningCount = sensors.Count(s => s.CurrentSeverity == SeverityLevel.Warning);
            
            if (criticalCount > 0)
            {
                Console.WriteLine($"⚠️ {DateTime.Now:HH:mm:ss} - {criticalCount} critical, {warningCount} warning");
            }
            else if (warningCount > 0)
            {
                Console.WriteLine($"⚠️ {DateTime.Now:HH:mm:ss} - {warningCount} warnings");
            }
            else
            {
                Console.WriteLine($"✅ {DateTime.Now:HH:mm:ss} - All {sensors.Count} sensors normal");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Simulator error: {ex.Message}");
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");

app.MapPost("/api/sensors/register", (SensorRegistrationRequest request, SensorStore store) =>
{
    var record = store.Register(request);
    Console.WriteLine($"✅ Sensor registered: {record.DeviceMacAddress} in {record.Zone}");
    return Results.Created($"/api/sensors/{record.DeviceMacAddress}", record);
});

app.MapPost("/api/telemetry", (TelemetryIngestRequest request, SensorStore store) =>
{
    var updated = store.Ingest(request);
    if (updated is null)
    {
        return Results.NotFound(new { message = "Sensor not registered." });
    }
    
    Console.WriteLine($"📡 Telemetry: {request.DeviceMacAddress} = {request.Value} → Severity: {updated.CurrentSeverity}");
    return Results.Ok(updated);
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

    Console.WriteLine($"📎 File uploaded: {file.FileName} for {mac}");
    return Results.Ok(new { message = "File uploaded.", fileName = file.FileName });
}).DisableAntiforgery();

app.MapGet("/api/gamification/stats", (SensorStore store) =>
{
    return Results.Ok(store.GetGamificationStats());
});

app.MapGet("/api/gamification/alerts", (SensorStore store) =>
{
    return Results.Ok(store.GetAlertHistory());
});

app.MapPost("/api/gamification/resolve/{mac}", (string mac, SensorStore store) =>
{
    var sensor = store.GetByMac(mac);
    if (sensor == null)
        return Results.NotFound(new { message = "Sensor not found." });

    if (sensor.LastReading.HasValue)
    {
        store.Ingest(new TelemetryIngestRequest
        {
            DeviceMacAddress = mac,
            Value = sensor.LastReading.Value
        });
        
        Console.WriteLine($"✅ Alert resolved for: {mac}");
        return Results.Ok(new
        {
            message = "Alert resolution attempted. Sensor status updated.",
            mac = mac
        });
    }

    return Results.BadRequest(new { message = "Sensor has no reading to resolve." });
});

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

static int CountNodes(DeploymentNode node)
{
    int count = 1;
    foreach (var child in node.Children)
    {
        count += CountNodes(child);
    }
    return count;
}

app.MapGet("/api/deployment/zone-status", (SensorStore store) =>
{
    var root = store.BuildZoneHierarchy();
    var isValid = store.ValidateZoneHierarchy();
    var invalidZone = store.FindMissingDataZone();
    var nodeCount = store.CountZoneNodes();

    return Results.Ok(new
    {
        isValid = isValid,
        invalidZone = invalidZone,
        nodeCount = nodeCount,
        message = isValid 
            ? "All zones have active sensors with data." 
            : $"Zone '{invalidZone}' has sensors with no data.",
        tree = new
        {
            name = root.Name,
            isConfigured = root.IsConfigured,
            childCount = root.Children.Count,
            children = root.Children.Select(c => new
            {
                name = c.Name,
                isConfigured = c.IsConfigured,
                childCount = c.Children.Count
            })
        }
    });
});

app.Run();