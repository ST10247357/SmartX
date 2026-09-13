namespace SmartX.Api.Models;

// Adapted from: Baeldung (2024) - "The DTO Pattern (Data Transfer Object)"
// Encapsulates request payload data for registering a new sensor node in the system
public class SensorRegistrationRequest
{
    // Adapted from: Microsoft Learn (2023) - "String.Empty Property"
    // Uses string.Empty to ensure non-null property initialization and avoid unintended null references
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
}

// Adapted from: Said, Yahyaoui and Abdellatif (2021) - "Efficient Anomaly Detection for Smart Hospital IoT Systems"
// Tracks sensor state, operational health ratings, and maintenance resolution metrics based on IoT health models
public class SensorRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }

    // Adapted from: GeeksforGeeks (2023) - "Nullable Types in C#"
    // Uses nullable float (float?) to represent missing or uninitialized telemetry readings cleanly
    public float? LastReading { get; set; }
    public int StarRating { get; set; } = 5;
    public int ResolutionCount { get; set; } = 0;

    // Adapted from: Stack Overflow (2022) - "Proper way to set UTC timestamp default values in C# models"
    // Initializes UTC timestamp at instantiation to ensure accurate audit trail recording across time zones
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
Baeldung, 2024. The DTO Pattern (Data Transfer Object). Available at: https://www.baeldung.com/java-dto-pattern [Accessed 2 September 2026].
GeeksforGeeks, 2023. Nullable Types in C#. Available at: https://www.geeksforgeeks.org/c-sharp-nullable-types/ [Accessed 5 September 2026].
Microsoft, 2023. String.Empty Field (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.string.empty [Accessed 1 September 2026].
Said, A.M., Yahyaoui, A. and Abdellatif, T., 2021. Efficient Anomaly Detection for Smart Hospital IoT Systems. Sensors, 21(4), p. 1026. Available at: https://www.mdpi.com/1424-8220/21/4/1026 [Accessed 7 September 2026].
Stack Overflow, 2022. Proper way to set UTC timestamp default values in C# models. Available at: https://stackoverflow.com/questions/csharp-datetime-utc-default [Accessed 4 September 2026].
*/