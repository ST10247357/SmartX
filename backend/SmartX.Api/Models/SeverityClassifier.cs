namespace SmartX.Api.Models;

// Adapted from: Microsoft Learn (2024) - "Enumeration types (C# reference)"
// Defines discrete threat and operational status levels for classifying incoming IoT telemetry
public enum SeverityLevel
{
    Normal,
    Warning,
    Critical
}

// Adapted from: Microsoft Learn (2024) - "Static Classes and Static Class Members"
// Provides stateless, centralized utility methods to evaluate telemetry metrics against operational thresholds
public static class SeverityClassifier
{
    // Adapted from: GeeksforGeeks (2023) - "Conditional Statements in C#"
    // Evaluates numeric threshold boundaries to determine soil moisture anomaly levels
    public static SeverityLevel ClassifyMoisture(float percent)
    {
        if (percent < 15f) return SeverityLevel.Critical;
        if (percent < 30f) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    // Adapted from: Stack Overflow (2021) - "Calculating percentage change between two values in C#"
    // Computes relative variance between current and baseline power readings while guarding against division by zero
    public static SeverityLevel ClassifyPower(PowerReading current, PowerReading baseline)
    {
        var delta = current - baseline;
        var percentChange = baseline.Watts == 0 ? 0 : Math.Abs(delta.Watts) / baseline.Watts;

        if (current > baseline && percentChange > 0.5) return SeverityLevel.Critical; 
        if (current < baseline && percentChange > 0.5) return SeverityLevel.Warning;  
        if (percentChange > 0.2) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    // Adapted from: Microsoft Learn (2023) - "Expression-bodied members (C# programming guide)"
    // Uses expression-bodied syntax to compare telemetry state against expected system configuration
    public static SeverityLevel ClassifyValveState(bool isOpen, bool expectedOpen) =>
        isOpen != expectedOpen ? SeverityLevel.Warning : SeverityLevel.Normal;
}

/*
References:
GeeksforGeeks, 2023. C# | Decision Making (if, if-else, Nested-if, if-else-if). Available at: https://www.geeksforgeeks.org/c-sharp-decision-making-if-if-else-nested-if-if-else-if/ [Accessed 2 September 2026].
Microsoft, 2023. Expression-bodied members (C# programming guide). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members [Accessed 6 September 2026].
Microsoft, 2024. Enumeration types (C# reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum [Accessed 1 September 2026].
Microsoft, 2024. Static Classes and Static Class Members (C# Programming Guide). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members [Accessed 4 September 2026].
Stack Overflow, 2021. Calculating percentage change between two values in C#. Available at: https://stackoverflow.com/questions/percentage-change-calculation-csharp [Accessed 7 September 2026].
*/