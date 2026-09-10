// ============================================
// Academic Reference:
// Alert Lifecycle Tracking for IoT Systems
// Reference: PMC/MDPI (2025) - "Efficient Anomaly Detection for Smart Hospital IoT Systems"
// ============================================

namespace SmartX.Api.Models;

/// <summary>
/// Tracks critical alerts and resolutions for gamification.
/// Reference: PMC/MDPI (2025) - Efficient Anomaly Detection for Smart Hospital IoT Systems
/// </summary>
public class AlertRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float ValueAtDetection { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved => ResolvedAt.HasValue;
}