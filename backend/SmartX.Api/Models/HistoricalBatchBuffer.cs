namespace SmartX.Api.Models;

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