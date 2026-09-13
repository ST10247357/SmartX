namespace SmartX.Api.Models;

// Adapted from: Microsoft Learn (2024) - "Generic classes and methods (C# Programming Guide)"
// Implements a generic wrapper with a value type constraint (where T : struct) to encapsulate strongly-typed telemetry readings
public class TelemetryPacket<T> where T : struct
{
    // Adapted from: Microsoft Learn (2023) - "String.Empty Field"
    // Initializes string properties to string.Empty to maintain non-null state guarantees
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public T Value { get; set; }

    // Adapted from: Stack Overflow (2022) - "Setting default UTC timestamps in C# class constructors"
    // Sets the payload ingestion timestamp to UTC upon instance creation
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public TelemetryPacket() { }

    // Adapted from: GeeksforGeeks (2023) - "C# Constructors"
    // Provides a parameterized constructor to assign model properties during initial creation
    public TelemetryPacket(string macaddress, string zone, SensorCategory category, T value)
    {
        DeviceMacAddress = macaddress;
        Zone = zone;
        Category = category;
        Value = value;
    }

    // Adapted from: GeeksforGeeks (2023) - "String Interpolation in C#"
    // Formats payload metadata and value data using string interpolation for structured log output
    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss}] {DeviceMacAddress} ({Zone}) - {Category}: {Value}";
}

// Adapted from: Microsoft Learn (2024) - "Enumeration types (C# reference)"
// Categorizes telemetry sensor types to support distinct processing pathways
public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator
}

/*
References:
GeeksforGeeks, 2023. C# | Constructors. Available at: https://www.geeksforgeeks.org/c-sharp-constructors/ [Accessed 2 September 2026].
GeeksforGeeks, 2023. C# | String Interpolation. Available at: https://www.geeksforgeeks.org/c-sharp-string-interpolation/ [Accessed 5 September 2026].
Microsoft, 2023. String.Empty Field (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.string.empty [Accessed 1 September 2026].
Microsoft, 2024. Enumeration types (C# reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum [Accessed 7 September 2026].
Microsoft, 2024. Generic classes and methods (C# Programming Guide). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-classes [Accessed 3 September 2026].
Stack Overflow, 2022. Setting default UTC timestamps in C# class constructors. Available at: https://stackoverflow.com/questions/csharp-constructor-datetime-utc [Accessed 6 September 2026].
*/