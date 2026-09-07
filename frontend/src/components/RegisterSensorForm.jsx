import { useState } from "react";
import { registerSensor } from "../api/client";

const CATEGORY_OPTIONS = [
  { value: 0, label: "Environmental" },
  { value: 1, label: "Power Consumption" },
  { value: 2, label: "Actuator" },
];

export default function RegisterSensorForm({ onRegistered }) {
  const [mac, setMac] = useState("");
  const [zone, setZone] = useState("");
  const [category, setCategory] = useState(0);
  const [status, setStatus] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);

    if (!mac.trim() || !zone.trim()) {
      setStatus({ type: "error", message: "MAC address and zone are required." });
      return;
    }

    try {
      await registerSensor({
        deviceMacAddress: mac.trim(),
        zone: zone.trim(),
        category: Number(category),
      });
      setStatus({ type: "success", message: `Sensor ${mac} registered.` });
      setMac("");
      setZone("");
      onRegistered?.();
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: 24, padding: 16, border: "1px solid #ddd", borderRadius: 8 }}>
      <h3>Register New Sensor</h3>

      <div style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "flex-end" }}>
        <label>
          Device MAC / Unique ID
          <br />
          <input value={mac} onChange={(e) => setMac(e.target.value)} placeholder="AA:BB:04" />
        </label>

        <label>
          Zone / Room / Node ID
          <br />
          <input value={zone} onChange={(e) => setZone(e.target.value)} placeholder="Greenhouse-C" />
        </label>

        <label>
          Category
          <br />
          <select value={category} onChange={(e) => setCategory(e.target.value)}>
            {CATEGORY_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </label>

        <button type="submit">Register</button>
      </div>

      {status && (
        <p style={{ color: status.type === "error" ? "red" : "green", marginTop: 8 }}>
          {status.message}
        </p>
      )}
    </form>
  );
}
