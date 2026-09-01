namespace SmartX.Api.Models;

public class TelemetryPacket<T> where T : struct
{
    public string DeviceMacAddress {get; set;} = string.Empty;
    public string Zone {get; set;} = string .Empty;
    public SensorCategory Category {get; set;}
    public T Value {get; set;}
    public DateTime Timestamp {get; set;} = DateTime.UtcNow;

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
    Enviromental,
    PowerConsumption,
    Actuator
}