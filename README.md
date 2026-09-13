# PROG7312-POE - PART 1 - Smart-X IoT Mesh Ecosystem

A hybrid IoT mesh gateway built to ingest, validate, and monitor telemetry from distributed sensors across hydroponic farms, real estate utility tracking, and smart grid installations.

## Overview

Smart-X replaces manual, ad-hoc device monitoring with a modernised ASP.NET Core Minimal API and React dashboard. It ingests high-throughput telemetry from simulated ESP32-style devices, validates readings against category-specific thresholds, and keeps operators engaged through a gamified alert system rather than a passive log.

## Features

- 📡 **Sensor Registration & Telemetry Ingestion** — register devices by MAC address, zone, and category; push live readings via API
- ⭐ **Proactive Alert Gamification** — every sensor holds a star rating (5 = healthy); a critical reading drops it to 1 star and logs an alert; operators click **Resolve** to restore a star and log the resolution
- ⚡ **Severity Classification** — power spikes are treated as more critical than equivalent drops, using overloaded comparison operators
- 📎 **Encrypted File Uploads** — config files, deployment photos, and hardware logs are AES-256 encrypted at rest, restricted by file type and size
- 🌳 **Deployment Hierarchy Validation** — recursively validates arbitrarily deep facility/zone/sensor trees
- 🔢 **Zone Power Totals** — aggregates power readings across a zone using overloaded operators
- 🧪 **Mock Data Seeding** — seeds sensors across all three IoT scenarios, plus a background simulator that continuously drifts readings to prove the system holds up under load

## Key Requirements Covered

| Requirement | Implementation |
|---|---|
| Generics | `TelemetryPacket<T>` |
| Operator Overloading | `PowerReading` (`+`, `-`, `>`, `<`) |
| Advanced Arrays (jagged → List) | `HistoricalBatchBuffer` |
| Recursion (variable depth) | `DeploymentNode` |
| Collections | `Dictionary<string, SensorRecord>` in `SensorStore` |

## Tech Stack

- ASP.NET Core Minimal API (.NET 10)
- React (Vite)
- In-memory data store (no external database — coursework scale)
- AES-256 encryption via `System.Security.Cryptography`

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (LTS)

> **Note:** Visual Studio 2022 cannot target .NET 10 projects. Use **VS Code** with the C# Dev Kit extension, or the `dotnet` CLI directly.

### Configuration

Set the file encryption key via .NET User Secrets (kept out of source control):
```bash
cd backend/SmartX.Api
dotnet user-secrets init
dotnet user-secrets set "Encryption:Key" "your-own-secret-string"
```
If not set, a fallback development key is used automatically.

### Run the project

**Backend:**
```bash
cd backend/SmartX.Api
dotnet restore
dotnet run
```

**Frontend** (new terminal):
```bash
cd frontend
npm install
npm run dev
```

Both servers must run simultaneously. Open the frontend URL (usually `http://localhost:5173`).

## Quick Test

1. Open the dashboard → "Sensor Data Ingestion and Telemetry."
2. Trigger a critical reading via the browser console:
   ```js
   fetch("http://localhost:5275/api/telemetry", {
     method: "POST",
     headers: { "Content-Type": "application/json" },
     body: JSON.stringify({ deviceMacAddress: "AA:BB:01", value: 5 })
   }).then(r => r.json()).then(console.log)
   ```
3. Refresh the table — the sensor drops to 1 star. Click **Resolve** to restore it.

## API Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/sensors/register` | Register a sensor |
| POST | `/api/telemetry` | Push a reading |
| POST | `/api/telemetry/batch` | Push multiple devices' batched readings |
| GET | `/api/sensors` | List all sensors |
| POST | `/api/sensors/{mac}/upload` | Upload & encrypt a file for a sensor |
| GET | `/api/gamification/summary` | Dashboard summary counts |
| POST | `/api/gamification/resolve/{mac}` | Resolve a sensor's alert |
| GET | `/api/zones/{zone}/total-power` | Sum power readings in a zone |
| GET | `/api/deployment/zone-status` | Validate the facility/zone/sensor tree |

## Project Structure

```
SmartX/
├── backend/
│   └── SmartX.Api/
│       ├── Models/          # TelemetryPacket, PowerReading, DeploymentNode, SeverityClassifier, AlertRecord, Dtos, DeviceBatch, HistoricalBatchBuffer
│       ├── Services/        # SensorStore, MockDataSeeder, FileEncryptionHelper
│       └── Program.cs
└── frontend/
    └── src/
        ├── api/client.js
        └── components/      # LandingPage, Dashboard, SensorTable, RegisterSensorForm, FileUploadForm, SummaryPanel
```

## Known Limitations

- In-memory storage only — restarting the backend resets all data to the seeded defaults.
- The background telemetry simulator has no bounds on long-running drift; acceptable for a short demo, not production use.

## YouTube Link

[Insert YouTube link here]

## References

See [REFERENCES.md](./REFERENCES.md) for a full list of sources used.

## Author

[Insert student number] — PROG7312 / AAPD7112 POE Part 1
IIE Varsity College
