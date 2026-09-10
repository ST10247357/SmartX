namespace SmartX.Api.Models;

public class DeviceBatch
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public float[] Readings { get; set; } = Array.Empty<float>();
}