namespace SmartX.Api.Models;

// What the client sends when registering a new sensor.
public class SensorRegistrationRequest
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
}

// A registered sensor and its current gamified state.
public class SensorRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public float? LastReading { get; set; }

    // Gamification: 5 = healthy, drops to 1 on a critical reading.
    public int StarRating { get; set; } = 5;
    public int ResolutionCount { get; set; } = 0;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeen { get; set; }
}

// What the client sends when pushing a telemetry reading.
public class TelemetryIngestRequest
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float Value { get; set; }
}