using SmartX.Api.Models;

namespace SmartX.Api.Services;
public class SensorStore
{
    private readonly Dictionary<string, SensorRecord> _sensors = new();
    private readonly List<AlertRecord> _alerts = new();
    private readonly object _lock = new();

    // Adapted from Thread-Safe Dictionary Mutation (Albahari & Albahari, 2021)
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

    // Adapted from C# Switch Expressions & Out Parameters for Safe Lookups (Microsoft, 2024b; GeeksforGeeks, 2023b)
    public SensorRecord? Ingest(TelemetryIngestRequest request)
    {
        lock (_lock)
        {
            if (!_sensors.TryGetValue(request.DeviceMacAddress, out var sensor))
                return null;

            var previousReading = sensor.LastReading; // capture before overwrite
            sensor.LastReading = request.Value;
            sensor.LastSeen = DateTime.UtcNow;

            bool isCritical = sensor.Category switch
            {
                SensorCategory.Environmental =>
                    SeverityClassifier.ClassifyMoisture(request.Value) == SeverityLevel.Critical,
                SensorCategory.PowerConsumption =>
                    ClassifyPowerCritical(sensor, previousReading, request.Value),
                _ => false
            };

            if (isCritical)
            {
                sensor.StarRating = 1;
                _alerts.Add(new AlertRecord
                {
                    DeviceMacAddress = sensor.DeviceMacAddress,
                    ValueAtDetection = request.Value
                });
            }

            return sensor;
        }
    }

    private bool ClassifyPowerCritical(SensorRecord sensor, float? previousReading, float newValue)
    {
        var baseline = new PowerReading(sensor.DeviceMacAddress, previousReading ?? newValue);
        var current = new PowerReading(sensor.DeviceMacAddress, newValue);
        return SeverityClassifier.ClassifyPower(current, baseline) == SeverityLevel.Critical;
    }

    // Operator acknowledges the most recent open alert for a sensor.
    // Restores one star (capped at 5) and logs the resolution.
    public string? ResolveAlert(string mac)
    {
        lock (_lock)
        {
            var sensor = GetByMac(mac);
            if (sensor is null) return null;

            var alert = _alerts.FirstOrDefault(a => a.DeviceMacAddress == mac && !a.IsResolved);
            if (alert is null) return "No active alert for this sensor.";

            alert.ResolvedAt = DateTime.UtcNow;
            sensor.StarRating = Math.Min(5, sensor.StarRating + 1);
            sensor.ResolutionCount++;

            return $"Resolved. {sensor.DeviceMacAddress} is now {sensor.StarRating} stars.";
        }
    }

    // Adapted from LINQ Dynamic Projections and Aggregate Calculations (GeeksforGeeks, 2023c; Microsoft, 2024c)
    // Simple counts for the dashboard summary panel - no stored state to drift out of sync.
    public object GetSummary()
    {
        lock (_lock)
        {
            return new
            {
                totalSensors = _sensors.Count,
                openAlerts = _alerts.Count(a => !a.IsResolved),
                totalResolutions = _sensors.Values.Sum(s => s.ResolutionCount),
                averageStarRating = _sensors.Count == 0 ? 5 : _sensors.Values.Average(s => s.StarRating)
            };
        }
    }

    public List<AlertRecord> GetAlerts()
    {
        lock (_lock)
        {
            return _alerts.OrderByDescending(a => a.DetectedAt).ToList();
        }
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

    // Adapted from Custom Operator Overloading Accumulation (GeeksforGeeks, 2023d)
    // Adds together every power sensor's last reading in a zone using overloaded + operators.
    public PowerReading GetZoneTotalPower(string zone)
    {
        var total = new PowerReading(zone, 0);
        var powerSensors = _sensors.Values.Where(s => s.Zone == zone && s.Category == SensorCategory.PowerConsumption);

        foreach (var sensor in powerSensors)
            total += new PowerReading(sensor.DeviceMacAddress, sensor.LastReading ?? 0);

        return total;
    }

    // Adapted from Hierarchical Data Grouping & Composite Node Building (Fowler, 2002; GeeksforGeeks, 2022)
    // Builds a 3-level tree: Smart-X facility -> one node per zone -> one leaf per sensor.
    public DeploymentNode BuildZoneTree()
    {
        var root = new DeploymentNode { Name = "Smart-X Facility", IsConfigured = true };

        var zoneGroups = _sensors.Values.GroupBy(s => s.Zone);
        foreach (var group in zoneGroups)
        {
            var zoneNode = new DeploymentNode { Name = group.Key, IsConfigured = true };
            foreach (var sensor in group)
            {
                zoneNode.Children.Add(new DeploymentNode
                {
                    Name = sensor.DeviceMacAddress,
                    IsConfigured = sensor.LastReading.HasValue
                });
            }
            root.Children.Add(zoneNode);
        }

        return root;
    }
}

/*
References:
Fowler, M., 2002. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley.
Albahari, J. and Albahari, B., 2021. C# 10 in a Nutshell: The Definitive Reference. Sebastopol: O'Reilly Media.
Martin, R.C., 2008. Clean Code: A Handbook of Agile Software Craftsmanship. Upper Saddle River: Prentice Hall.
GeeksforGeeks, 2023b. Pattern Matching in C#. GeeksforGeeks. Available at: https://www.geeksforgeeks.org/pattern-matching-in-c-sharp/ [Accessed 10 September 2026].
*/