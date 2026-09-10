# Smart-X IoT Mesh Ecosystem

Part 1: Sensor Data Ingestion and Validation Gateway, for a simulated hybrid IoT mesh covering hydroponic farm monitoring, real estate utility tracking, and smart grid installations.

## Architecture

- **Backend:** ASP.NET Core Minimal API (.NET 10)
- **Frontend:** React (Vite)
- **Data storage:** In-memory (`Dictionary<string, SensorRecord>`) - resets when the API restarts; this is expected for a coursework-scale simulation, not a bug.
- **Data seeding:** `MockDataSeeder` pre-loads 7 sensors across all three scenarios on startup, plus a background simulator that nudges every sensor's reading every 10 seconds to prove the data structures hold up under continuous load.

```
React Dashboard  <-- HTTP (JSON) -->  ASP.NET Core Minimal API  <-- method calls -->  SensorStore
```

## Features implemented (Part 1)

- Startup landing page with three module tiles; only "Sensor Data Ingestion and Telemetry" is active (the other two are scoped for Part 2 and the final PoE).
- Sensor registration form (MAC address, zone, category).
- Telemetry ingestion endpoint, with severity classification per sensor category.
- File/log/photo upload, attached to a specific sensor.
- Sortable, filterable sensor table (by category, by zone).
- **Dynamic engagement feature - proactive alert gamification:** every sensor has a star rating (5 = healthy). A critical reading drops it to 1 star and logs an alert. The operator clicks **Resolve** to acknowledge it, restoring one star and incrementing that sensor's resolution count. A summary panel shows total sensors, open alerts, total resolutions, and average star rating across all sensors.
- Power spikes (rising above threshold) are classified as more severe (Critical) than equivalent drops (Warning), using the overloaded comparison operators on `PowerReading`.

## Technical requirements covered

| Requirement | Where |
|---|---|
| Generics | `TelemetryPacket<T>` |
| Operator overloading (`+`, `-`, `>`, `<`) | `PowerReading` struct, used in `SeverityClassifier` and `SensorStore.GetZoneTotalPower()` |
| Advanced arrays (jagged → `List<T>`) | `HistoricalBatchBuffer`, exercised via `POST /api/telemetry/batch` |
| Recursion | `DeploymentNode.ValidateHierarchy()` / `FindFirstInvalidPath()`, exercised via `GET /api/deployment/zone-status` |
| Collections (`Dictionary`, `List`) | `SensorStore` throughout |

## Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) - confirm with `dotnet --list-sdks`
- [Node.js](https://nodejs.org) (LTS) — confirm with `node -v`
- **Note:** Visual Studio 2022 cannot target `.NET 10` projects. This project was built and should be run using **VS Code** with the C# Dev Kit extension, or any editor plus the `dotnet` CLI directly.

### Backend
```bash
cd backend/SmartX.Api
dotnet restore
dotnet run
```
Note the URL it prints (e.g. `http://localhost:5275`) - the frontend expects the API at this address (configured in `frontend/src/api/client.js`).

### Frontend
In a second terminal:
```bash
cd frontend
npm install
npm run dev
```
Open the printed URL (usually `http://localhost:5173`).

**Both servers must be running at the same time** for the dashboard to load data.

## Testing the engagement feature

1. Open the dashboard and click into "Sensor Data Ingestion and Telemetry."
2. The seeded sensors load automatically - note `AA:BB:02` (dry moisture sensor) already shows 1 star.
3. Open the browser console (F12) and send a critical reading manually:
   ```js
   fetch("http://localhost:5275/api/telemetry", {
     method: "POST",
     headers: { "Content-Type": "application/json" },
     body: JSON.stringify({ deviceMacAddress: "BB:CC:01", value: 9999 })
   }).then(r => r.json()).then(console.log)
   ```
4. Refresh the table (or wait 5 seconds for the automatic poll) — `BB:CC:01` should now show 1 star.
5. Click **Resolve** next to it — the star count increases and the resolution count updates in the summary panel.

## API endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/sensors/register` | Register a new sensor |
| POST | `/api/telemetry` | Push a single reading |
| POST | `/api/telemetry/batch` | Push multiple devices' batched readings (jagged array demo) |
| GET | `/api/sensors` | List all sensors |
| GET | `/api/sensors/{mac}` | Get one sensor |
| POST | `/api/sensors/{mac}/upload` | Attach a file to a sensor |
| GET | `/api/gamification/summary` | Dashboard summary counts |
| GET | `/api/gamification/alerts` | Alert history |
| POST | `/api/gamification/resolve/{mac}` | Resolve a sensor's open alert |
| GET | `/api/zones/{zone}/total-power` | Sum power readings in a zone (operator overloading demo) |
| GET | `/api/deployment/zone-status` | Validate the facility/zone/sensor hierarchy (recursion demo) |

## Known limitations

- Data is in-memory only; restarting the backend clears all registered sensors and alert history back to the seeded defaults.
- The background simulator has no bounds on how far a reading can drift over a long-running session - acceptable for a short demo, not intended for production use.

## Project structure

```
SmartX/
├── backend/
│   └── SmartX.Api/
│       ├── Models/       (TelemetryPacket, PowerReading, DeploymentNode, SeverityClassifier, etc.)
│       ├── Services/     (SensorStore, MockDataSeeder)
│       └── Program.cs
└── frontend/
    └── src/
        ├── api/client.js
        └── components/   (LandingPage, Dashboard, SensorTable, RegisterSensorForm, FileUploadForm, SummaryPanel)
```
