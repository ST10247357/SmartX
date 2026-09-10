namespace SmartX.Api.Models;

// Adapted from: Baeldung (2024) - "The DTO Pattern (Data Transfer Object)"
// Encapsulates request payload data for registering a new sensor node in the system
public class SensorRegistrationRequest
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
}

// References gamification mechanisms from IoT anomaly detection models (Said, Yahyaoui and Abdellatif, 2021)
// Tracks sensor state, operational health ratings, and maintenance resolution metrics
public class SensorRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public float? LastReading { get; set; }
    public int StarRating { get; set; } = 5;
    public int ResolutionCount { get; set; } = 0;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeen { get; set; }
}

// Adapted from: Baeldung (2024) - "The DTO Pattern (Data Transfer Object)"
// Transport model for ingesting single-value telemetry data from edge devices
public class TelemetryIngestRequest
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float Value { get; set; }
}

/*
References:
Baeldung, 2024. The DTO Pattern (Data Transfer Object). Available at: https://www.baeldung.com/java-dto-pattern [Accessed 10 September 2026].
Said, A.M., Yahyaoui, A. and Abdellatif, T., 2021. Efficient Anomaly Detection for Smart Hospital IoT Systems. Sensors, 21(4), p. 1026. Available at: https://www.mdpi.com/1424-8220/21/4/1026 [Accessed 10 September 2026].
*/