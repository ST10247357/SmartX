namespace SmartX.Api.Models;

public struct PowerReading
{
    public string NodeId { get; set; }
    public double Watts { get; set; }
    public DateTime Timestamp { get; set; }

    // Adapted from: Stack Overflow (2022) - "Best practices for struct constructors in modern C#"
    // Initializes value type properties inline and assigns UTC timestamp at creation
    public PowerReading(string nodeId, double watts)
    {
        NodeId = nodeId;
        Watts = watts;
        Timestamp = DateTime.UtcNow;
    }

    // Adapted from: GeeksforGeeks (2024) - "C# Operator Overloading"
    // Overloads arithmetic additions to aggregate wattage readings between nodes into a combined result
    public static PowerReading operator +(PowerReading a, PowerReading b) =>
        new($"{a.NodeId}+{b.NodeId}", a.Watts + b.Watts);

    // Adapted from: GeeksforGeeks (2024) - "C# Operator Overloading"
    // Overloads subtraction to compute telemetry wattage deltas between reading instances
    public static PowerReading operator -(PowerReading a, PowerReading b) =>
        new($"{a.NodeId}-delta", a.Watts - b.Watts);

    // Adapted from: Microsoft Learn (2023) - "Operator overloading - predefined and user-defined operators"
    // Implements relational operators (< and >) in matching pairs to enable direct power magnitude comparisons
    public static bool operator >(PowerReading a, PowerReading b) => a.Watts > b.Watts;
    public static bool operator <(PowerReading a, PowerReading b) => a.Watts < b.Watts;

    // Adapted from: Microsoft Learn (2023) - "Standard numeric format strings - F format specifier"
    // Formats numerical power values to two fixed decimal places alongside formatted timestamps for logging output
    public override string ToString() => $"{NodeId}: {Watts:F2}W @ {Timestamp:HH:mm:ss}";
}

/*
References:
GeeksforGeeks, 2024. C# | Operator Overloading. Available at: https://www.geeksforgeeks.org/c-sharp-operator-overloading/ [Accessed 3 September 2026].
Microsoft, 2023. Operator overloading - predefined and user-defined operators. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading [Accessed 1 September 2026].
Microsoft, 2023. Standard numeric format strings. Available at: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings [Accessed 7 September 2026].
Stack Overflow, 2022. Best practices for struct constructors in modern C#. Available at: https://stackoverflow.com/questions/csharp-struct-constructor-patterns [Accessed 5 September 2026].
*/