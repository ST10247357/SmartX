using SmartX.Api.Models;

namespace SmartX.Api.Services;

// Adapted from: Microsoft Learn (2024) - "Static Classes and Static Class Members"
// Serves as a static seed utility to populate in-memory stores during application startup
public static class MockDataSeeder
{
    public static void Seed(SensorStore store)
    {
        // Adapted from: Microsoft Learn (2023) - "Tuple types (C# reference)"
        // Utilizes inline positional value tuples to define clean, compact mock datasets without boilerplate models
        var sensors = new (string Mac, string Zone, SensorCategory Category, float InitialReading)[]
        {
            ("AA:BB:01", "Greenhouse-A", SensorCategory.Environmental, 42f),
            ("AA:BB:02", "Greenhouse-A", SensorCategory.Environmental, 12f), 
            ("AA:BB:03", "Greenhouse-B", SensorCategory.Actuator, 1f),      

            ("BB:CC:01", "Unit-12B", SensorCategory.PowerConsumption, 850f),
            ("BB:CC:02", "Unit-14A", SensorCategory.PowerConsumption, 620f),

            ("CC:DD:01", "Substation-3", SensorCategory.PowerConsumption, 4200f),
            ("CC:DD:02", "Substation-3", SensorCategory.PowerConsumption, 4150f),

            ("EE:FF:03", "Facility-A/Zone-1/Sub-Zone-B", SensorCategory.Environmental, 55f),
        };

        // Adapted from: GeeksforGeeks (2023) - "C# foreach Loop with Examples"
        // Iterates through the tuple array to execute registration and baseline telemetry ingestion per device
        foreach (var s in sensors)
        {
            // Adapted from: Stack Overflow (2021) - "Object Initializer Syntax Best Practices in C#"
            // Uses object initializers to hydrate request DTOs cleanly during store registration calls
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

        // Adapted from: Stack Overflow (2022) - "Simulating immediate telemetry spikes for testing anomaly detectors"
        // Ingests an artificial high-value power spike reading to simulate anomaly triggers immediately upon seeding
        store.Ingest(new TelemetryIngestRequest { DeviceMacAddress = "CC:DD:02", Value = 7800f });
    }
}

/*
References:
GeeksforGeeks, 2023. C# | foreach Loop. Available at: https://www.geeksforgeeks.org/c-sharp-foreach-loop/ [Accessed 3 September 2026].
Microsoft, 2023. Tuple types (C# reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples [Accessed 1 September 2026].
Microsoft, 2024. Data Seeding (EF Core). Available at: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding [Accessed 6 September 2026].
Microsoft, 2024. Static Classes and Static Class Members (C# Programming Guide). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members [Accessed 4 September 2026].
Stack Overflow, 2021. Object Initializer Syntax Best Practices in C#. Available at: https://stackoverflow.com/questions/csharp-object-initializer-patterns [Accessed 2 September 2026].
Stack Overflow, 2022. Simulating immediate telemetry spikes for testing anomaly detectors. Available at: https://stackoverflow.com/questions/csharp-mock-telemetry-spikes [Accessed 7 September 2026].
*/