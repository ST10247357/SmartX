import { useEffect, useState } from "react";
import { getSensors } from "../api/client";
import SensorTable from "./SensorTable";
import RegisterSensorForm from "./RegisterSensorForm";
import FileUploadForm from "./FileUploadForm";
import SummaryPanel from "./SummaryPanel";

export default function Dashboard() {
  // Adapted from: React Docs (2024a) - "useState"
  // Initializes state hooks for sensor records, network error messages, and trigger keys for forced re-renders
  const [sensors, setSensors] = useState([]);
  const [error, setError] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  // Adapted from: Developer Mozilla (2024) - "async function"
  // Handles asynchronous data fetching from the API service and updates state while catching network failures
  async function loadSensors() {
    try {
      setSensors(await getSensors());
      setError(null);
    } catch (err) {
      setError(err.message);
    }
  }

  // Adapted from: React Docs (2024a) - "Updating state based on previous state"
  // Increments a numeric counter key to trigger manual refresh passes across child components
  function refreshAll() {
    loadSensors();
    setRefreshKey((k) => k + 1);
  }

  // Adapted from: React Docs (2024b) - "Synchronizing with Effects & Cleaning up an Effect"
  // Establishes a 5-second polling interval on mount and clears the timer on unmount to prevent memory leaks
  useEffect(() => {
    refreshAll();
    const interval = setInterval(refreshAll, 5000);
    return () => clearInterval(interval);
  }, []);

  // Adapted from: W3Schools (2024) - "React CSS Styling"
  // Structure dashboard components using CSS Grid and inline styles
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
Developer Mozilla, 2024. async function. MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function [Accessed 2 September 2026].
React Docs, 2024a. useState. React Documentation. Available at: https://react.dev/reference/react/useState [Accessed 5 September 2026].
React Docs, 2024b. Synchronizing with Effects. React Documentation. Available at: https://react.dev/learn/synchronizing-with-effects [Accessed 1 September 2026].
W3Schools, 2024. React CSS Styling. W3Schools. Available at: https://www.w3schools.com/react/react_css.asp [Accessed 6 September 2026].
*/