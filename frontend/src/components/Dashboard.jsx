import { useEffect, useState } from "react";
import { getSensors } from "../api/client";
import SensorTile from "./SensorTile";
import RegisterSensorForm from "./RegisterSensorForm";
import FileUploadForm from "./FileUploadForm";

export default function Dashboard() {
  const [sensors, setSensors] = useState([]);
  const [error, setError] = useState(null);

  async function loadSensors() {
    try {
      const data = await getSensors();
      setSensors(data);
      setError(null);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => {
    loadSensors();
    // Poll every 5 seconds so the grid reflects new telemetry without a manual refresh.
    const interval = setInterval(loadSensors, 5000);
    return () => clearInterval(interval);
  }, []);

  const criticalCount = sensors.filter((s) => s.currentSeverity === 2).length;

  return (
    <div style={{ padding: 24 }}>
      <h2>Sensor Data Ingestion and Telemetry</h2>

      <RegisterSensorForm onRegistered={loadSensors} />
      <FileUploadForm sensors={sensors} />

      {error && <p style={{ color: "red" }}>Error: {error}</p>}

      <p style={{ marginBottom: 16 }}>
        {sensors.length} sensors registered — {criticalCount} critical
      </p>

      <div style={{ display: "flex", flexWrap: "wrap", gap: 16 }}>
        {sensors.map((sensor) => (
          <SensorTile key={sensor.deviceMacAddress} sensor={sensor} />
        ))}
      </div>
    </div>
  );
}
