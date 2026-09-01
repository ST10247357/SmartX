using SmartX.Api.Models;
 
namespace SmartX.Api.Services;

public class SensorStore
{
    private readonly Dictionary<string, SensorRecord> _sensors = new();
    private readonly object _lock = new();
 
    public SensorRecord Register(SensorRegistrationRequest request)
    {
        lock (_lock)
        {
            var record = new SensorRecord
            {
                DeviceMacAddress = request.DeviceMacAddress,
                Zone = request.Zone,
                Category = request.Category
            };
            _sensors[request.DeviceMacAddress] = record;
            return record;
        }
    }

    public SensorRecord? Ingest(TelemetryIngestRequest request)
    {
        lock (_lock)
        {
            if (!_sensors.TryGetValue(request.DeviceMacAddress, out var sensor))
                return null;
 
            sensor.LastReading = request.Value;
            sensor.LastSeen = DateTime.UtcNow;
            sensor.CurrentSeverity = sensor.Category switch
            {
                SensorCategory.Environmental => SeverityClassifier.ClassifyMoisture(request.Value),
                SensorCategory.PowerConsumption => ClassifyPowerFromBaseline(sensor, request.Value),
                SensorCategory.Actuator => SeverityClassifier.ClassifyValveState(request.Value != 0, true),
                _ => SeverityLevel.Normal
            };
 
            return sensor;
        }
    }
 
    private SeverityLevel ClassifyPowerFromBaseline(SensorRecord sensor, float newValue)
    {
        var baseline = new PowerReading(sensor.DeviceMacAddress, sensor.LastReading ?? newValue);
        var current = new PowerReading(sensor.DeviceMacAddress, newValue);
        return SeverityClassifier.ClassifyPower(current, baseline);
    }
 
    public List<SensorRecord> GetAll()
    {
        lock (_lock)
        {
            return _sensors.Values.ToList();
        }
    }
 
    public SensorRecord? GetByMac(string mac)
    {
        lock (_lock)
        {
            return _sensors.TryGetValue(mac, out var sensor) ? sensor : null;
        }
    }
}            
 