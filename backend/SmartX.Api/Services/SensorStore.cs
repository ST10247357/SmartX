using SmartX.Api.Models;

namespace SmartX.Api.Services;

public class SensorStore
{
    private readonly Dictionary<string, SensorRecord> _sensors = new();
    private readonly List<AlertRecord> _alertHistory = new();
    private readonly GamificationStats _stats = new();
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

            var previousSeverity = sensor.CurrentSeverity;
            var previousReading = sensor.LastReading;

            sensor.LastReading = request.Value;
            sensor.LastSeen = DateTime.UtcNow;

            sensor.CurrentSeverity = sensor.Category switch
            {
                SensorCategory.Environmental => SeverityClassifier.ClassifyMoisture(request.Value),
                SensorCategory.PowerConsumption => ClassifyPowerFromBaseline(sensor, previousReading, request.Value),
                SensorCategory.Actuator => SeverityClassifier.ClassifyValveState(request.Value != 0, true),
                _ => SeverityLevel.Normal
            };

            // Track critical alerts
            if (sensor.CurrentSeverity == SeverityLevel.Critical && previousSeverity != SeverityLevel.Critical)
            {
                var alert = new AlertRecord
                {
                    DeviceMacAddress = sensor.DeviceMacAddress,
                    Severity = SeverityLevel.Critical,
                    DetectedAt = DateTime.UtcNow,
                    ValueAtDetection = request.Value
                };
                _alertHistory.Add(alert);
                _stats.TotalCriticalAlerts++;
                _stats.LastCriticalAlertTime = DateTime.UtcNow;
                
                // Reset stability streak
                _stats.StabilityStreakHours = 0;
            }
            
            // Track resolution
            if (sensor.CurrentSeverity != SeverityLevel.Critical && previousSeverity == SeverityLevel.Critical)
            {
                var activeAlert = _alertHistory
                    .Where(a => a.DeviceMacAddress == sensor.DeviceMacAddress && !a.IsResolved)
                    .OrderByDescending(a => a.DetectedAt)
                    .FirstOrDefault();
                    
                if (activeAlert != null)
                {
                    activeAlert.ResolvedAt = DateTime.UtcNow;
                    activeAlert.ValueAtResolution = request.Value;
                    _stats.ResolvedAlerts++;
                    
                    if (activeAlert.IsQuickResolution)
                        _stats.QuickResolutions++;
                    
                    if (!_stats.SensorResolutionCounts.ContainsKey(sensor.DeviceMacAddress))
                        _stats.SensorResolutionCounts[sensor.DeviceMacAddress] = 0;
                    _stats.SensorResolutionCounts[sensor.DeviceMacAddress]++;
                }
            }

            // Update stability streak (if no critical alerts are active)
            if (!_alertHistory.Any(a => !a.IsResolved && a.Severity == SeverityLevel.Critical))
            {
                var lastAlert = _alertHistory
                    .Where(a => a.Severity == SeverityLevel.Critical)
                    .OrderByDescending(a => a.DetectedAt)
                    .FirstOrDefault();
                    
                if (lastAlert != null && lastAlert.IsResolved)
                {
                    var hoursSinceResolution = (DateTime.UtcNow - lastAlert.ResolvedAt.Value).TotalHours;
                    _stats.StabilityStreakHours = (int)hoursSinceResolution;
                }
                else if (!_alertHistory.Any(a => a.Severity == SeverityLevel.Critical))
                {
                    // No critical alerts ever - system is perfectly stable
                    _stats.StabilityStreakHours = 24; // Start with 24 hours
                }
            }

            return sensor;
        }
    }

    private SeverityLevel ClassifyPowerFromBaseline(SensorRecord sensor, float? previousReading, float newValue)
    {
        var baseline = new PowerReading(sensor.DeviceMacAddress, previousReading ?? newValue);
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

    public GamificationStats GetGamificationStats()
    {
        lock (_lock)
        {
            return _stats;
        }
    }

    public List<AlertRecord> GetAlertHistory()
    {
        lock (_lock)
        {
            return _alertHistory.OrderByDescending(a => a.DetectedAt).ToList();
        }
    }
}