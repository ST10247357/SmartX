import { useEffect, useState } from "react";
import { getSensors } from "../api/client";
import SensorTile from "./SensorTile";
import RegisterSensorForm from "./RegisterSensorForm";
import FileUploadForm from "./FileUploadForm";
import GamificationStats from "./GamificationStats"; // Import the new component

export default function Dashboard() {
  const [sensors, setSensors] = useState([]);
  const [error, setError] = useState(null);
  const [showGamification, setShowGamification] = useState(true); // Toggle

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
    const interval = setInterval(loadSensors, 5000);
    return () => clearInterval(interval);
  }, []);

  const criticalCount = sensors.filter((s) => s.currentSeverity === 2).length;

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Sensor Data Ingestion and Telemetry</h2>
        <button
          onClick={() => setShowGamification(!showGamification)}
          style={{
            padding: '8px 16px',
            borderRadius: 4,
            border: '1px solid #ddd',
            cursor: 'pointer',
            backgroundColor: showGamification ? '#e8f5e9' : '#f5f5f5',
          }}
        >
          {showGamification ? 'Hide Gamification' : 'Show Gamification'}
        </button>
      </div>

      <RegisterSensorForm onRegistered={loadSensors} />
      <FileUploadForm sensors={sensors} />

      {error && <p style={{ color: "red" }}>Error: {error}</p>}

      <p style={{ marginBottom: 16 }}>
        {sensors.length} sensors registered — {criticalCount} critical
      </p>

      {/* GAMIFICATION STATS - The core gamification feature */}
      {showGamification && <GamificationStats />}

      {/* Achievement banner - Professional version */}
      {criticalCount === 0 && sensors.length > 0 && (
        <div style={{
          backgroundColor: '#e8f5e9',
          borderLeft: '4px solid #2e7d32',
          padding: '8px 16px',
          marginBottom: 16,
          borderRadius: 4,
        }}>
          <span style={{ color: '#2e7d32', fontWeight: 600 }}>
            ✅ All systems normal. System health is stable.
          </span>
        </div>
      )}

      {criticalCount > 0 && (
        <div style={{
          backgroundColor: '#ffebee',
          borderLeft: '4px solid #c62828',
          padding: '8px 16px',
          marginBottom: 16,
          borderRadius: 4,
        }}>
          <span style={{ color: '#c62828', fontWeight: 600 }}>
            ⚠️ {criticalCount} critical alert{criticalCount > 1 ? 's' : ''} require attention!
          </span>
        </div>
      )}

      <div style={{ display: "flex", flexWrap: "wrap", gap: 16 }}>
        {sensors.map((sensor) => (
          <SensorTile key={sensor.deviceMacAddress} sensor={sensor} />
        ))}
      </div>
    </div>
  );
}