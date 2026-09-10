import { useEffect, useState } from "react";
import { getSensors } from "../api/client";
import SensorTable from "./SensorTable";
import RegisterSensorForm from "./RegisterSensorForm";
import FileUploadForm from "./FileUploadForm";
import SummaryPanel from "./SummaryPanel";

export default function Dashboard() {
  // Adapted from React useState Hook for Local State Management (React Docs, 2024a)
  const [sensors, setSensors] = useState([]);
  const [error, setError] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  async function loadSensors() {
    try {
      setSensors(await getSensors());
      setError(null);
    } catch (err) {
      setError(err.message);
    }
  }

  // Adapted from Triggering Child Re-renders via State Counters (React Docs, 2024b)
  function refreshAll() {
    loadSensors();
    setRefreshKey((k) => k + 1);
  }

  // Adapted from Effect Hook Polling & Interval Cleanup (Meta Open Source, 2024; W3Schools, 2024)
  useEffect(() => {
    refreshAll();
    const interval = setInterval(refreshAll, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div style={{ padding: 24 }}>
      <h2>Sensor Data Ingestion and Telemetry</h2>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gap: 16,
          marginBottom: 16,
        }}
      >
        <div style={{ border: "1px solid #30363d", borderRadius: 10, padding: 20, backgroundColor: "#161b22" }}>
          <RegisterSensorForm onRegistered={refreshAll} />
        </div>
        <div style={{ border: "1px solid #30363d", borderRadius: 10, padding: 20, backgroundColor: "#161b22" }}>
          <FileUploadForm sensors={sensors} />
        </div>
      </div>

      <SummaryPanel refreshKey={refreshKey} />

      {error && <p style={{ color: "red" }}>Error: {error}</p>}

      <SensorTable sensors={sensors} onResolved={refreshAll} />
    </div>
  );
}

/*
References:
Meta Open Source, 2024. Synchronizing with Effects. React Documentation. Available at: https://react.dev/learn/synchronizing-with-effects [Accessed 10 September 2026].
React Docs, 2024a. useState. React Documentation. Available at: https://react.dev/reference/react/useState [Accessed 10 September 2026].
React Docs, 2024b. useEffect. React Documentation. Available at: https://react.dev/reference/react/useEffect [Accessed 10 September 2026].
W3Schools, 2024. React useEffect Hooks. W3Schools. Available at: https://www.w3schools.com/react/react_useeffect.asp [Accessed 10 September 2026].
*/