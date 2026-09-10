namespace SmartX.Api.Models;

// Adapted from: PMC/MDPI (2025) - "Efficient Anomaly Detection for Smart Hospital IoT Systems"
// Modified alert model to track detection values and resolution timestamps for gamification system
public class AlertRecord
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float ValueAtDetection { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved => ResolvedAt.HasValue;
}

/*
References:
Zachos, G., Mantas, G., Porfyrakis, K. and Rodriguez, J., 2025. Implementing Anomaly-Based Intrusion Detection for Resource-Constrained Devices in IoMT Networks. Sensors, 25(4), p. 1216. Available at: https://pmc.ncbi.nlm.nih.gov/articles/PMC11859946/ [Accessed 10 September 2026].
Said, A.M., Yahyaoui, A. and Abdellatif, T., 2021. Efficient Anomaly Detection for Smart Hospital IoT Systems. Sensors, 21(4), p. 1026. Available at: https://www.mdpi.com/1424-8220/21/4/1026 [Accessed 10 September 2026].
*/