using SmartX.Api.Models;
 
namespace SmartX.Api.Services;

public static class MockDataSeeder
{
    public static void Seed(SensorStore store)
    {
        var sensors = new (string Mac, string Zone, SensorCategory Category, float InitialReading)[]
        {
            ("AA:BB:01", "Greenhouse-A", SensorCategory.Environmental, 42f),
            ("AA:BB:02", "Greenhouse-A", SensorCategory.Environmental, 12f), // critical - dry
            ("AA:BB:03", "Greenhouse-B", SensorCategory.Actuator, 1f),      // valve open
 
            ("BB:CC:01", "Unit-12B", SensorCategory.PowerConsumption, 850f),
            ("BB:CC:02", "Unit-14A", SensorCategory.PowerConsumption, 620f),
 
            ("CC:DD:01", "Substation-3", SensorCategory.PowerConsumption, 4200f),
            ("CC:DD:02", "Substation-3", SensorCategory.PowerConsumption, 4150f),
        };
 
        foreach (var s in sensors)
        {
            store.Register(new SensorRegistrationRequest
            {
                DeviceMacAddress = s.Mac,
                Zone = s.Zone,
                Category = s.Category
            });
 
            store.Ingest(new TelemetryIngestRequest
            {
                DeviceMacAddress = s.Mac,
                Value = s.InitialReading
            });
        }
 
       store.Ingest(new TelemetryIngestRequest { DeviceMacAddress = "CC:DD:02", Value = 7800f });
    }
}
 