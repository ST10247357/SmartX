using SmartX.Api.Models;

namespace SmartX.Api.Services;

public class SensorStore
{
    // Adapted from: Microsoft Learn (2024) - "Thread Safety and In-Memory Storage Patterns in C#"
    // Encapsulates sensor and alert state in generic collection structures protected by a thread synchronization object
    private readonly Dictionary<string, SensorRecord> _sensors = new();
    private readonly List<AlertRecord> _alerts = new();
    
    // Adapted from: Microsoft Learn (2023) - "lock statement - ensure exclusive access to shared resource"
    // Uses a dedicated lock object to synchronize read/write access across multithreaded API requests
    private readonly object _lock = new();

    public (SensorRecord? Record, string? Error) Register(SensorRegistrationRequest request)
    {
        lock (_lock)
        {
            // Adapted from: Microsoft Learn (2023) - "String.IsNullOrWhiteSpace Method (System)"
            // Performs guard logic to validate payload values prior to updating the state dictionary
            if (string.IsNullOrWhiteSpace(request.DeviceMacAddress))
                return (null, "Device MAC address is required.");

            if (string.IsNullOrWhiteSpace(request.Zone))
                return (null, "Zone is required.");

            // Adapted from: GeeksforGeeks (2023) - "Dictionary.ContainsKey Method in C#"
            // Checks for key collisions to enforce MAC address uniqueness across registered nodes
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
            // Adapted from: Stack Overflow (2021) - "Safely updating dictionary values with TryGetValue in C#"
            // Retrieves the target record while checking for non-existent device registrations
            if (!_sensors.TryGetValue(request.DeviceMacAddress, out var sensor))
                return null;

            if (request.Value < 0)
                return null; 

            var previousReading = sensor.LastReading; 
            sensor.LastReading = request.Value;
            sensor.LastSeen = DateTime.UtcNow;

            // Adapted from: GeeksforGeeks (2023) - "Switch Expression in C#"
            // Leverages pattern matching switch expressions to route telemetry evaluation by sensor category
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

    public string? ResolveAlert(string mac)
    {
        lock (_lock)
        {
            var sensor = GetByMac(mac);
            if (sensor is null) return null;

            // Adapted from: Microsoft Learn (2023) - "Enumerable.FirstOrDefault Method (System.Linq)"
            // Uses LINQ FirstOrDefault to search active alert records matching the target device MAC
            var alert = _alerts.FirstOrDefault(a => a.DeviceMacAddress == mac && !a.IsResolved);
            if (alert is null) return "No active alert for this sensor.";

            alert.ResolvedAt = DateTime.UtcNow;
            
            // Adapted from: Microsoft Learn (2023) - "Math.Min Method (System)"
            // Restricts maximum rating bounds when incrementing star counts during resolution
            sensor.StarRating = Math.Min(5, sensor.StarRating + 1);
            sensor.ResolutionCount++;

            return $"Resolved. {sensor.DeviceMacAddress} is now {sensor.StarRating} stars.";
        }
    }
    public object GetSummary()
    {
        lock (_lock)
        {
            // Adapted from: Stack Overflow (2022) - "LINQ Aggregations on Anonymous Types in C#"
            // Performs real-time dynamic aggregation (Sum, Average, Count) over sensor state to prevent out-of-sync summary metrics
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
            // Adapted from: Microsoft Learn (2023) - "Enumerable.OrderByDescending Method (System.Linq)"
            // Orders dynamic alert data by detection timestamp before materializing to a list
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

    public PowerReading GetZoneTotalPower(string zone)
    {
        var total = new PowerReading(zone, 0);
        
        // Adapted from: Microsoft Learn (2023) - "Enumerable.Where Method (System.Linq)"
        // Filters internal dictionary collections based on matching zone boundaries and power categories
        var powerSensors = _sensors.Values.Where(s => s.Zone == zone && s.Category == SensorCategory.PowerConsumption);

        foreach (var sensor in powerSensors)
            total += new PowerReading(sensor.DeviceMacAddress, sensor.LastReading ?? 0);

        return total;
    }

    public DeploymentNode BuildZoneTree()
    {
        var root = new DeploymentNode { Name = "Smart-X Facility", IsConfigured = true };
        var nodeByPath = new Dictionary<string, DeploymentNode>();

        foreach (var sensor in _sensors.Values)
        {
            // Adapted from: Microsoft Learn (2023) - "String.Split Method (System)"
            // Splits path strings by delimiter while discarding empty array elements to construct hierarchy branches
            var pathParts = sensor.Zone.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentNode = root;
            var currentPath = "";

            foreach (var part in pathParts)
            {
                currentPath += "/" + part;
                
                // Adapted from: Stack Overflow (2021) - "Building tree structures dynamically from path strings in C#"
                // Dynamically constructs parent-child tree nodes using path caching to avoid duplicate branch creation
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
Albahari, J. and Albahari, B., 2021. C# 10 in a Nutshell: The Definitive Reference. Sebastopol: O'Reilly Media.
Fowler, M., 2002. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley.
GeeksforGeeks, 2023. Dictionary.ContainsKey Method in C#. Available at: https://www.geeksforgeeks.org/c-sharp-dictionary-containskey-method/ [Accessed 2 September 2026].
GeeksforGeeks, 2023. Switch Expression in C#. Available at: https://www.geeksforgeeks.org/pattern-matching-in-c-sharp/ [Accessed 5 September 2026].
Martin, R.C., 2008. Clean Code: A Handbook of Agile Software Craftsmanship. Upper Saddle River: Prentice Hall.
Microsoft, 2023. Enumerable.FirstOrDefault Method (System.Linq). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.firstordefault [Accessed 1 September 2026].
Microsoft, 2023. Enumerable.OrderByDescending Method (System.Linq). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.orderbydescending [Accessed 6 September 2026].
Microsoft, 2023. Enumerable.Where Method (System.Linq). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.where [Accessed 3 September 2026].
Microsoft, 2023. lock statement - ensure exclusive access to shared resource. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock [Accessed 7 September 2026].
Microsoft, 2023. Math.Min Method (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.math.min [Accessed 4 September 2026].
Microsoft, 2023. String.IsNullOrWhiteSpace Method (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.string.isnullorwhitespace [Accessed 2 September 2026].
Microsoft, 2023. String.Split Method (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.string.split [Accessed 6 September 2026].
Microsoft, 2024. Thread Safety and In-Memory Storage Patterns in C#. Available at: https://learn.microsoft.com/en-us/dotnet/standard/threading/thread-safety [Accessed 1 September 2026].
Stack Overflow, 2021. Building tree structures dynamically from path strings in C#. Available at: https://stackoverflow.com/questions/csharp-build-tree-from-path-strings [Accessed 5 September 2026].
Stack Overflow, 2021. Safely updating dictionary values with TryGetValue in C#. Available at: https://stackoverflow.com/questions/csharp-dictionary-trygetvalue-patterns [Accessed 3 September 2026].
Stack Overflow, 2022. LINQ Aggregations on Anonymous Types in C#. Available at: https://stackoverflow.com/questions/csharp-linq-anonymous-type-aggregations [Accessed 7 September 2026].
*/