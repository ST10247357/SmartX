import { useState } from "react";
import { getCategoryLabel, resolveAlert } from "../api/client";

export default function SensorTable({ sensors, onResolved, colors }) {
  // Adapted from React useState Hook for Sorting & Filtering State (React Docs, 2024a)
  const [sortField, setSortField] = useState("deviceMacAddress");
  const [sortDirection, setSortDirection] = useState("asc");
  const [filterCategory, setFilterCategory] = useState("all");
  const [filterZone, setFilterZone] = useState("all");
  const [resolving, setResolving] = useState(null);

  const darkColors = colors || {
    surface: '#161b22',
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
    dangerBg: 'rgba(248, 81, 73, 0.15)',
    dangerText: '#f85149',
    successText: '#3fb950',
    star: '#e3b341',
  };

  const zones = ["all", ...new Set((sensors || []).map((s) => s.zone))];

  // Adapted from Array Filtering & Sorting Logic in React (React Docs, 2024a)
  const filtered = (sensors || [])
    .filter((s) => filterCategory === "all" || s.category === parseInt(filterCategory))
    .filter((s) => filterZone === "all" || s.zone === filterZone);

  const sorted = [...filtered].sort((a, b) => {
    let aVal = a[sortField] ?? "";
    let bVal = b[sortField] ?? "";
    if (typeof aVal === "string") {
      return sortDirection === "asc" ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
    }
    return sortDirection === "asc" ? aVal - bVal : bVal - aVal;
  });

  function handleSort(field) {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  }

  // Adapted from Asynchronous Handler Pattern (React Docs, 2024b)
  async function handleResolve(mac) {
    setResolving(mac);
    try {
      await resolveAlert(mac);
      onResolved?.();
    } finally {
      setResolving(null);
    }
  }

  const sortIcon = (field) => (sortField !== field ? "↕" : sortDirection === "asc" ? "↑" : "↓");
  const stars = (rating) => "★".repeat(rating) + "☆".repeat(5 - rating);

  if (!sensors || sensors.length === 0) {
    return (
      <p style={{ color: darkColors.textSecondary, fontSize: 13, fontStyle: 'italic', padding: 12 }}>
        No sensors registered yet.
      </p>
    );
  }

  return (
    <div style={{ border: `1px solid ${darkColors.border}`, borderRadius: 8, overflow: "hidden", backgroundColor: darkColors.surface }}>
      <div style={{ padding: 10, display: "flex", gap: 10, borderBottom: `1px solid ${darkColors.border}`, alignItems: 'center' }}>
        <select 
          value={filterCategory} 
          onChange={(e) => setFilterCategory(e.target.value)}
          style={{
            padding: '6px 10px',
            borderRadius: 6,
            border: `1px solid ${darkColors.border}`,
            backgroundColor: darkColors.surface2,
            color: darkColors.text,
            fontSize: 12,
            outline: 'none',
            cursor: 'pointer'
          }}
        >
          <option value="all">All Categories</option>
          <option value="0">Environmental</option>
          <option value="1">Power</option>
          <option value="2">Actuator</option>
        </select>

        <select 
          value={filterZone} 
          onChange={(e) => setFilterZone(e.target.value)}
          style={{
            padding: '6px 10px',
            borderRadius: 6,
            border: `1px solid ${darkColors.border}`,
            backgroundColor: darkColors.surface2,
            color: darkColors.text,
            fontSize: 12,
            outline: 'none',
            cursor: 'pointer'
          }}
        >
          {zones.map((z) => (
            <option key={z} value={z}>{z === "all" ? "All Zones" : z}</option>
          ))}
        </select>

        <span style={{ marginLeft: "auto", fontSize: 12, color: darkColors.textSecondary }}>
          {sorted.length} of {sensors.length} sensors
        </span>
      </div>

      <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13, color: darkColors.text }}>
        <thead>
          <tr style={{ backgroundColor: darkColors.surface2, borderBottom: `1px solid ${darkColors.border}` }}>
            <th onClick={() => handleSort("deviceMacAddress")} style={{ padding: 10, cursor: "pointer", textAlign: "left", color: darkColors.textSecondary, fontWeight: 600 }}>
              MAC {sortIcon("deviceMacAddress")}
            </th>
            <th onClick={() => handleSort("zone")} style={{ padding: 10, cursor: "pointer", textAlign: "left", color: darkColors.textSecondary, fontWeight: 600 }}>
              Zone {sortIcon("zone")}
            </th>
            <th style={{ padding: 10, textAlign: "left", color: darkColors.textSecondary, fontWeight: 600 }}>Category</th>
            <th onClick={() => handleSort("lastReading")} style={{ padding: 10, cursor: "pointer", textAlign: "right", color: darkColors.textSecondary, fontWeight: 600 }}>
              Reading {sortIcon("lastReading")}
            </th>
            <th onClick={() => handleSort("starRating")} style={{ padding: 10, cursor: "pointer", textAlign: "center", color: darkColors.textSecondary, fontWeight: 600 }}>
              Rating {sortIcon("starRating")}
            </th>
            <th style={{ padding: 10, textAlign: "center", color: darkColors.textSecondary, fontWeight: 600 }}>Resolutions</th>
            <th style={{ padding: 10, textAlign: "center", color: darkColors.textSecondary, fontWeight: 600 }}>Action</th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((sensor) => (
            <tr
              key={sensor.deviceMacAddress}
              style={{
                borderBottom: `1px solid ${darkColors.border}`,
                backgroundColor: sensor.starRating === 1 ? darkColors.dangerBg : "transparent",
                transition: 'background-color 0.2s'
              }}
            >
              <td style={{ padding: 10, fontFamily: 'monospace' }}>{sensor.deviceMacAddress}</td>
              <td style={{ padding: 10 }}>{sensor.zone}</td>
              <td style={{ padding: 10 }}>{getCategoryLabel(sensor.category)}</td>
              <td style={{ padding: 10, textAlign: "right", fontWeight: 500 }}>{sensor.lastReading ?? "—"}</td>
              <td style={{ padding: 10, textAlign: "center", color: darkColors.star, letterSpacing: 1 }}>
                {stars(sensor.starRating)}
              </td>
              <td style={{ padding: 10, textAlign: "center" }}>{sensor.resolutionCount}</td>
              <td style={{ padding: 10, textAlign: "center" }}>
                {sensor.starRating === 1 ? (
                  <button 
                    onClick={() => handleResolve(sensor.deviceMacAddress)} 
                    disabled={resolving === sensor.deviceMacAddress}
                    style={{
                      padding: '4px 12px',
                      borderRadius: 4,
                      border: 'none',
                      backgroundColor: darkColors.dangerText,
                      color: 'white',
                      fontSize: 12,
                      fontWeight: 600,
                      cursor: resolving === sensor.deviceMacAddress ? 'not-allowed' : 'pointer',
                      opacity: resolving === sensor.deviceMacAddress ? 0.6 : 1
                    }}
                  >
                    {resolving === sensor.deviceMacAddress ? "..." : "Resolve"}
                  </button>
                ) : (
                  <span style={{ color: darkColors.successText, fontSize: 12, fontWeight: 500 }}>
                    ✓ Healthy
                  </span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/*
References:
MDN Web Docs, 2024. HTML table styling and layout. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Learn/CSS/Building_blocks/Styling_tables [Accessed 10 September 2026].
React Docs, 2024a. Rendering Lists. React Documentation. Available at: https://react.dev/learn/rendering-lists [Accessed 10 September 2026].
React Docs, 2024b. Responding to Events. React Documentation. Available at: https://react.dev/learn/responding-to-events [Accessed 10 September 2026].
*/