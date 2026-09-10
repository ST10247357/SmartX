namespace SmartX.Api.Models;

public enum SeverityLevel
{
    Normal,
    Warning,
    Critical
}

// Adapted from enumeration and static utility class patterns (Microsoft, 2024)
// Classifies telemetry anomalies based on moisture thresholds, power surge deltas, and state mismatches
public static class SeverityClassifier
{
    public static SeverityLevel ClassifyMoisture(float percent)
    {
        if (percent < 15f) return SeverityLevel.Critical;
        if (percent < 30f) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    // Spikes (current > baseline) are treated as more dangerous than drops
    // (current < baseline) - a surge is usually a bigger risk than a dip.
    public static SeverityLevel ClassifyPower(PowerReading current, PowerReading baseline)
    {
        var delta = current - baseline;
        var percentChange = baseline.Watts == 0 ? 0 : Math.Abs(delta.Watts) / baseline.Watts;

        if (current > baseline && percentChange > 0.5) return SeverityLevel.Critical; // spike
        if (current < baseline && percentChange > 0.5) return SeverityLevel.Warning;  // big drop
        if (percentChange > 0.2) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    public static SeverityLevel ClassifyValveState(bool isOpen, bool expectedOpen) =>
        isOpen != expectedOpen ? SeverityLevel.Warning : SeverityLevel.Normal;
}

/*
References:
Microsoft, 2024. Enumeration types (C# reference). Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum [Accessed 10 September 2026].
*/