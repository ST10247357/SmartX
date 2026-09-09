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

            // Update stability streak
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
                    _stats.StabilityStreakHours = 24;
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

    // ============================================
    // ZONE HIERARCHY METHODS (USING RECURSION)
    // ============================================

    /// <summary>
    /// Builds a hierarchical tree of zones from registered sensors.
    /// </summary>
    public DeploymentNode BuildZoneHierarchy()
    {
        var root = new DeploymentNode { Name = "Smart-X Facility", IsConfigured = true };
        var zoneMap = new Dictionary<string, DeploymentNode>();

        lock (_lock)
        {
            foreach (var sensor in _sensors.Values)
            {
                // Split zone by '-' (e.g., "Greenhouse-A" -> ["Greenhouse", "A"])
                var zoneParts = sensor.Zone.Split('-');
                var parent = root;

                foreach (var part in zoneParts)
                {
                    var key = parent.Name + "->" + part;
                    if (!zoneMap.ContainsKey(key))
                    {
                        var node = new DeploymentNode
                        {
                            Name = part,
                            IsConfigured = sensor.LastReading.HasValue // Has data = configured
                        };
                        zoneMap[key] = node;
                        parent.Children.Add(node);
                    }
                    parent = zoneMap[key];
                }
            }
        }

        return root;
    }

    /// <summary>
    /// Recursively validates the entire zone hierarchy.
    /// Returns true if all zones have sensors with data.
    /// </summary>
    public bool ValidateZoneHierarchy()
    {
        var root = BuildZoneHierarchy();
        return root.ValidateHierarchy();
    }

    /// <summary>
    /// Recursively finds the first invalid zone in the hierarchy.
    /// Returns the path to the invalid zone as a string.
    /// </summary>
    public string? FindMissingDataZone()
    {
        var root = BuildZoneHierarchy();
        var invalidPath = root.FindFirstInvalidPath();
        return invalidPath != null ? string.Join(" -> ", invalidPath) : null;
    }

    /// <summary>
    /// Recursively counts all nodes in the zone hierarchy.
    /// </summary>
    public int CountZoneNodes()
    {
        var root = BuildZoneHierarchy();
        return CountNodesRecursive(root);
    }

    private int CountNodesRecursive(DeploymentNode node)
    {
        int count = 1;
        foreach (var child in node.Children)
        {
            count += CountNodesRecursive(child);
        }
        return count;
    }
}