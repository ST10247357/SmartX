namespace SmartX.Api.Models;

public class AlertRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public SeverityLevel Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public float ValueAtDetection { get; set; }
    public float? ValueAtResolution { get; set; }
    public string? ResolutionAction { get; set; } // e.g., "Pump turned on"
    
    public bool IsResolved => ResolvedAt.HasValue;
    public bool IsQuickResolution => IsResolved && 
        (ResolvedAt.Value - DetectedAt).TotalMinutes <= 5;
}