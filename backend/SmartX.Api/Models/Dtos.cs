namespace SmartX.Api.Models;

public class SensorRegistrationRequest
{
    public string DeviceMacAddress {get;set;} = string.Empty;
    public string Zone {get;set;} =string.Empty;
    public SensorCategory Category {get;set;}

}

public class SensorRecord
{
     public string DeviceMacAddress { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public float? LastReading { get; set; }
    public SeverityLevel CurrentSeverity { get; set; } = SeverityLevel.Normal;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeen { get; set; }
}

public class TelemetryIngestRequest
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float Value { get; set; }
}