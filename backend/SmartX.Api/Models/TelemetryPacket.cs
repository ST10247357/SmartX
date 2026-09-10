namespace SmartX.Api.Models;

// Adapted from generic type constraints and class template design patterns (Microsoft, 2024)
// Implements a generic wrapper to encapsulate strongly-typed telemetry readings across different sensor categories
public class TelemetryPacket<T> where T : struct
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public T Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public TelemetryPacket() { }

    public TelemetryPacket(string macaddress, string zone, SensorCategory category, T value)
    {
        DeviceMacAddress = macaddress;
        Zone = zone;
        Category = category;
        Value = value;
    }

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss}] {DeviceMacAddress} ({Zone}) - {Category}: {Value}";
}

public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator
}

/*
References:
Microsoft, 2024. Generic classes and methods (C# Programming Guide). Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-classes [Accessed 10 September 2026].
*/