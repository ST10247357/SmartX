namespace SmartX.Api.Models;

// Adapted from: Baeldung (2024) - "The DTO Pattern (Data Transfer Object)"
// Implements the Data Transfer Object pattern to encapsulate batched sensor telemetry for API transport
public class DeviceBatch
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float[] Readings { get; set; } = Array.Empty<float>();
}

/*
References:
Baeldung, 2024. The DTO Pattern (Data Transfer Object). Available at: https://www.baeldung.com/java-dto-pattern [Accessed 10 September 2026].
*/