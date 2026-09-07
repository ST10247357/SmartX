using System.Security.Cryptography.X509Certificates;

namespace SmartX.Api.Models;

public enum SeverityLevel
{
    Normal,
    Warning,
    Critical
}

public static class SeverityClassifier
{
    public static SeverityLevel ClassifyMoisture(float percent)
    {
        if (percent < 15f) return SeverityLevel.Critical;
        if (percent < 30f) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    public static SeverityLevel ClassifyPower(PowerReading current, PowerReading baseline)
    {
        var delta = current - baseline; 
        var percentChange = baseline.Watts == 0 ? 0 : Math.Abs(delta.Watts) / baseline.Watts;

        if (percentChange > 0.5) return SeverityLevel.Critical;
        if (percentChange > 0.2) return SeverityLevel.Warning;
        return SeverityLevel.Normal;
    }

    public static SeverityLevel ClassifyValveState(bool isOpen, bool expectedOpen) =>
        isOpen != expectedOpen ? SeverityLevel.Warning : SeverityLevel.Normal;
}

