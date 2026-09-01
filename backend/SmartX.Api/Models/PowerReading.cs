using System.Diagnostics.Eventing.Reader;

namespace SmartX.Api.Models;

public struct PowerReading
{
    public string NodeId {get;set;}
    public double Watts {get;set;}
    public DateTime Timestamp {get;set;}

    public PowerReading(string nodeId, double watts)
    {
        NodeId = nodeId;
        Watts = watts;
        Timestamp = DateTime.UtcNow;
    }

    public static PowerReading operator +(PowerReading a, PowerReading b) =>
        new($"{a.NodeId}+{b.NodeId}", a.Watts + b.Watts);

    public static PowerReading operator -(PowerReading a, PowerReading b) =>
        new($"{a.NodeId}-delta", a.Watts - b.Watts);

    public static bool operator >(PowerReading a, PowerReading b) => a.Watts > b.Watts;
    public static bool operator <(PowerReading a, PowerReading b) => a.Watts < b.Watts;

    public override string ToString() => $"{NodeId}: {Watts:F2}W @ {Timestamp:HH:mm:ss}";

}