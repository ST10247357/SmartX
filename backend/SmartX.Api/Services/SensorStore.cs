using SmartX.Api.Models;

namespace SmartX.Api.Services;

// In-memory store keyed by MAC address for instant lookups when telemetry arrives.
public class SensorStore
{
    private readonly Dictionary<string, SensorRecord> _sensors = new();
    private readonly List<AlertRecord> _alerts = new();
    private readonly object _lock = new();

    // Returns (record, error). If error is non-null, registration was rejected
    // and record is null - the caller (Program.cs) should return 400 Bad Request.
    public (SensorRecord? Record, string? Error) Register(SensorRegistrationRequest request)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceMacAddress))
                return (null, "Device MAC address is required.");

            if (string.IsNullOrWhiteSpace(request.Zone))
                return (null, "Zone is required.");

            if (_sensors.ContainsKey(request.DeviceMacAddress))
                return (null, $"Sensor {request.DeviceMacAddress} is already registered.");

            var record = new SensorRecord
            {
                DeviceMacAddress = request.DeviceMacAddress,
                Zone = request.Zone,
                Category = request.Category
            };
            _sensors[request.DeviceMacAddress] = record;
            return (record, null);
        }
    }

    public SensorRecord? Ingest(TelemetryIngestRequest request)
    {
        lock (_lock)
        {
            if (!_sensors.TryGetValue(request.DeviceMacAddress, out var sensor))
                return null;

            if (request.Value < 0)
                return null; // negative readings are never valid for moisture, power, or valve state

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

    // Adds together every power sensor's last reading in a zone, using the
    // overloaded + operator instead of manually summing numbers.
    public PowerReading GetZoneTotalPower(string zone)
    {
        var total = new PowerReading(zone, 0);
        var powerSensors = _sensors.Values.Where(s => s.Zone == zone && s.Category == SensorCategory.PowerConsumption);

        foreach (var sensor in powerSensors)
            total += new PowerReading(sensor.DeviceMacAddress, sensor.LastReading ?? 0);

        return total;
    }

    // Builds a variable-depth tree from each sensor's zone path. A zone can be a
    // single name ("Greenhouse-A") for a flat hierarchy, or a "/"-separated path
    // ("Facility-A/Zone-1/Sub-Zone-B") for arbitrarily deep nesting - matching the
    // brief's own example of Sub-Zone B -> Zone 1 -> Facility A. Shared path
    // segments across sensors reuse the same node instead of duplicating it.
    // A sensor leaf is "configured" if it has reported a reading at least once.
    public DeploymentNode BuildZoneTree()
    {
        var root = new DeploymentNode { Name = "Smart-X Facility", IsConfigured = true };
        var nodeByPath = new Dictionary<string, DeploymentNode>();

        foreach (var sensor in _sensors.Values)
        {
            var pathParts = sensor.Zone.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentNode = root;
            var currentPath = "";

            foreach (var part in pathParts)
            {
                currentPath += "/" + part;
                if (!nodeByPath.TryGetValue(currentPath, out var zoneNode))
                {
                    zoneNode = new DeploymentNode { Name = part, IsConfigured = true };
                    nodeByPath[currentPath] = zoneNode;
                    currentNode.Children.Add(zoneNode);
                }
                currentNode = zoneNode;
            }

            currentNode.Children.Add(new DeploymentNode
            {
                Name = sensor.DeviceMacAddress,
                IsConfigured = sensor.LastReading.HasValue
            });
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