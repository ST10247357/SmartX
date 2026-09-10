namespace SmartX.Api.Models;

// Adapted from: GeeksforGeeks (2023) - "Jagged Array or Array of Arrays in C# with Examples"
// Uses a jagged array (_rawBatches) to buffer variable-length sensor readings per device before flattening into structured lists
public class HistoricalBatchBuffer
{
    private float[][] _rawBatches;
    private readonly List<string> _deviceIds;

    public HistoricalBatchBuffer(int deviceCount)
    {
        _rawBatches = new float[deviceCount][];
        _deviceIds = new List<string>(deviceCount);
    }

    public void AddDeviceBatch(string deviceId, float[] readings)
    {
        _deviceIds.Add(deviceId);
        var index = _deviceIds.Count - 1;

        if (index >= _rawBatches.Length)
            Array.Resize(ref _rawBatches, _rawBatches.Length + 1);

        _rawBatches[index] = readings;
    }

    // Flattens jagged data into the List<T> structure the rest of the app uses.
    public List<TelemetryPacket<float>> TransferToOptimisedList(string zone, SensorCategory category)
    {
        var result = new List<TelemetryPacket<float>>();

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
GeeksforGeeks, 2023. Jagged Array or Array of Arrays in C# with Examples. Available at: https://www.geeksforgeeks.org/c/jagged-array-or-array-of-arrays-in-c-with-examples/ [Accessed 10 September 2026].
*/