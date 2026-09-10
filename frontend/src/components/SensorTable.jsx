import { useState } from "react";
import { getCategoryLabel, resolveAlert } from "../api/client";

export default function SensorTable({ sensors, onResolved }) {
  const [sortField, setSortField] = useState("deviceMacAddress");
  const [sortDirection, setSortDirection] = useState("asc");
  const [filterCategory, setFilterCategory] = useState("all");
  const [filterZone, setFilterZone] = useState("all");
  const [resolving, setResolving] = useState(null);

  const zones = ["all", ...new Set((sensors || []).map((s) => s.zone))];

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
    return <p>No sensors registered yet.</p>;
  }

  return (
    <div style={{ border: "1px solid #ddd", borderRadius: 8, overflow: "hidden" }}>
      <div style={{ padding: 10, display: "flex", gap: 10, borderBottom: "1px solid #ddd" }}>
        <select value={filterCategory} onChange={(e) => setFilterCategory(e.target.value)}>
          <option value="all">All Categories</option>
          <option value="0">Environmental</option>
          <option value="1">Power</option>
          <option value="2">Actuator</option>
        </select>

        <select value={filterZone} onChange={(e) => setFilterZone(e.target.value)}>
          {zones.map((z) => (
            <option key={z} value={z}>{z === "all" ? "All Zones" : z}</option>
          ))}
        </select>

        <span style={{ marginLeft: "auto", fontSize: 12, color: "#666" }}>
          {sorted.length} of {sensors.length} sensors
        </span>
      </div>

      <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
        <thead>
          <tr style={{ backgroundColor: "#f5f5f5" }}>
            <th onClick={() => handleSort("deviceMacAddress")} style={{ padding: 10, cursor: "pointer", textAlign: "left" }}>
              MAC {sortIcon("deviceMacAddress")}
            </th>
            <th onClick={() => handleSort("zone")} style={{ padding: 10, cursor: "pointer", textAlign: "left" }}>
              Zone {sortIcon("zone")}
            </th>
            <th style={{ padding: 10, textAlign: "left" }}>Category</th>
            <th onClick={() => handleSort("lastReading")} style={{ padding: 10, cursor: "pointer", textAlign: "right" }}>
              Reading {sortIcon("lastReading")}
            </th>
            <th onClick={() => handleSort("starRating")} style={{ padding: 10, cursor: "pointer", textAlign: "center" }}>
              Rating {sortIcon("starRating")}
            </th>
            <th style={{ padding: 10, textAlign: "center" }}>Resolutions</th>
            <th style={{ padding: 10, textAlign: "center" }}>Action</th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((sensor) => (
            <tr
              key={sensor.deviceMacAddress}
              style={{
                borderBottom: "1px solid #eee",
                backgroundColor: sensor.starRating === 1 ? "#ffebee" : "transparent",
              }}
            >
              <td style={{ padding: 10 }}>{sensor.deviceMacAddress}</td>
              <td style={{ padding: 10 }}>{sensor.zone}</td>
              <td style={{ padding: 10 }}>{getCategoryLabel(sensor.category)}</td>
              <td style={{ padding: 10, textAlign: "right" }}>{sensor.lastReading ?? "—"}</td>
              <td style={{ padding: 10, textAlign: "center" }}>{stars(sensor.starRating)}</td>
              <td style={{ padding: 10, textAlign: "center" }}>{sensor.resolutionCount}</td>
              <td style={{ padding: 10, textAlign: "center" }}>
                {sensor.starRating === 1 ? (
                  <button onClick={() => handleResolve(sensor.deviceMacAddress)} disabled={resolving === sensor.deviceMacAddress}>
                    {resolving === sensor.deviceMacAddress ? "..." : "Resolve"}
                  </button>
                ) : (
                  "Healthy"
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
