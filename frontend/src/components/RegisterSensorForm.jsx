import { useState } from "react";
import { registerSensor } from "../api/client";

const CATEGORY_OPTIONS = [
  { value: 0, label: "🌱 Environmental", description: "Temperature, moisture, humidity" },
  { value: 1, label: "⚡ Power Consumption", description: "Energy usage monitoring" },
  { value: 2, label: "🔧 Actuator", description: "Valves, switches, relays" },
];

export default function RegisterSensorForm({ onRegistered, colors }) {
  const [mac, setMac] = useState("");
  const [zone, setZone] = useState("");
  const [category, setCategory] = useState(0);
  const [status, setStatus] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const darkColors = colors || {
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
  };

  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);
    setIsSubmitting(true);

    if (!mac.trim() || !zone.trim()) {
      setStatus({ type: "error", message: "MAC address and zone are required." });
      setIsSubmitting(false);
      return;
    }

    try {
      await registerSensor({
        deviceMacAddress: mac.trim().toUpperCase(),
        zone: zone.trim(),
        category: Number(category),
      });
      setStatus({ type: "success", message: `✅ Sensor ${mac} registered successfully!` });
      setMac("");
      setZone("");
      onRegistered?.();
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <div style={{
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: 12,
        marginBottom: 12
      }}>
        <div>
          <label style={{
            fontSize: 12,
            fontWeight: 600,
            color: darkColors.textSecondary,
            display: 'block',
            marginBottom: 4
          }}>
            MAC / Unique ID
          </label>
          <input
            value={mac}
            onChange={(e) => setMac(e.target.value)}
            placeholder="e.g., AA:BB:CC:DD:EE:FF"
            style={{
              width: '100%',
              padding: '8px 12px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 13,
              outline: 'none',
              transition: 'border-color 0.2s'
            }}
            onFocus={(e) => e.target.style.borderColor = darkColors.primary}
            onBlur={(e) => e.target.style.borderColor = darkColors.border}
          />
        </div>

        <div>
          <label style={{
            fontSize: 12,
            fontWeight: 600,
            color: darkColors.textSecondary,
            display: 'block',
            marginBottom: 4
          }}>
            Zone / Location
          </label>
          <input
            value={zone}
            onChange={(e) => setZone(e.target.value)}
            placeholder="e.g., Greenhouse-A"
            style={{
              width: '100%',
              padding: '8px 12px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 13,
              outline: 'none',
              transition: 'border-color 0.2s'
            }}
            onFocus={(e) => e.target.style.borderColor = darkColors.primary}
            onBlur={(e) => e.target.style.borderColor = darkColors.border}
          />
        </div>

        <div style={{ gridColumn: '1 / -1' }}>
          <label style={{
            fontSize: 12,
            fontWeight: 600,
            color: darkColors.textSecondary,
            display: 'block',
            marginBottom: 4
          }}>
            Category
          </label>
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            style={{
              width: '100%',
              padding: '8px 12px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 13,
              outline: 'none',
              cursor: 'pointer'
            }}
          >
            {CATEGORY_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value} style={{
                backgroundColor: darkColors.surface2,
                color: darkColors.text,
                padding: '6px'
              }}>
                {opt.label} - {opt.description}
              </option>
            ))}
          </select>
        </div>
      </div>

      <button
        type="submit"
        disabled={isSubmitting}
        style={{
          padding: '8px 20px',
          borderRadius: 6,
          border: 'none',
          backgroundColor: isSubmitting ? darkColors.textSecondary : darkColors.primary,
          color: 'white',
          fontWeight: 600,
          fontSize: 13,
          cursor: isSubmitting ? 'not-allowed' : 'pointer',
          transition: 'all 0.2s',
          width: '100%'
        }}
        onMouseEnter={(e) => {
          if (!isSubmitting) e.currentTarget.style.backgroundColor = '#4a8dd6';
        }}
        onMouseLeave={(e) => {
          if (!isSubmitting) e.currentTarget.style.backgroundColor = darkColors.primary;
        }}
      >
        {isSubmitting ? '⏳ Registering...' : '➕ Register Sensor'}
      </button>

      {status && (
        <p style={{
          marginTop: 10,
          color: status.type === "error" ? '#f85149' : '#3fb950',
          fontSize: 13,
          fontWeight: 500
        }}>
          {status.message}
        </p>
      )}
    </form>
  );
}