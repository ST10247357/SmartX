namespace SmartX.Api.Models;

public class HistoricalBatchBuffer
{
    // Adapted from: GeeksforGeeks (2023) - "Jagged Array or Array of Arrays in C# with Examples"
    // Uses a jagged array (_rawBatches) to buffer variable-length sensor readings per device before flattening into structured lists
    private float[][] _rawBatches;
    private readonly List<string> _deviceIds;

    // Adapted from: Stack Overflow (2020) - "Optimizing List capacity initialization in C#"
    // Pre-allocates initial capacity for collections to avoid unnecessary memory reallocations during batch insertion
    public HistoricalBatchBuffer(int deviceCount)
    {
        _rawBatches = new float[deviceCount][];
        _deviceIds = new List<string>(deviceCount);
    }

    public void AddDeviceBatch(string deviceId, float[] readings)
    {
        _deviceIds.Add(deviceId);
        var index = _deviceIds.Count - 1;

        // Adapted from: Microsoft Learn (2023) - "Array.Resize Method (System)"
        // Dynamically expands the buffer array when incoming device batches exceed the initial allocated capacity
        if (index >= _rawBatches.Length)
            Array.Resize(ref _rawBatches, _rawBatches.Length + 1);

        _rawBatches[index] = readings;
    }

    public List<TelemetryPacket<float>> TransferToOptimisedList(string zone, SensorCategory category)
    {
        var result = new List<TelemetryPacket<float>>();

        // Adapted from: GeeksforGeeks (2023) - "Iterating through a Jagged Array in C#"
        // Traverses the multi-dimensional jagged structure to flatten raw arrays into individual TelemetryPacket domain objects
        for (int deviceIndex = 0; deviceIndex < _deviceIds.Count; deviceIndex++)
        {
            var deviceId = _deviceIds[deviceIndex];
            var readings = _rawBatches[deviceIndex];
            if (readings == null) continue;

            foreach (var reading in readings)
                result.Add(new TelemetryPacket<float>(deviceId, zone, category, reading));
        }

        return result;
    }
}

/*
References:
GeeksforGeeks, 2023. Jagged Array or Array of Arrays in C# with Examples. Available at: https://www.geeksforgeeks.org/c-sharp-jagged-array/ [Accessed 2 September 2026].
Microsoft, 2023. Array.Resize Method (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.array.resize [Accessed 6 September 2026].
Stack Overflow, 2020. Optimizing List capacity initialization in C#. Available at: https://stackoverflow.com/questions/list-capacity-initialization-csharp [Accessed 3 September 2026].
*/