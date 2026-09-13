import { useState } from "react";
import { registerSensor } from "../api/client";

// Adapted from: Developer Mozilla (2024b) - "Array.prototype.map()"
// Static metadata array defining available sensor category dropdown options
const CATEGORY_OPTIONS = [
  { value: 0, label: "Environmental", description: "Temperature, moisture, humidity" },
  { value: 1, label: "Power Consumption", description: "Energy usage monitoring" },
  { value: 2, label: "Actuator", description: "Valves, switches, relays" },
];

export default function RegisterSensorForm({ onRegistered, colors }) {
  // Adapted from: React Docs (2024a) - "Sharing State Between Components"
  // Initializes component state variables to manage controlled input fields and submission lifecycle states
  const [mac, setMac] = useState("");
  const [zone, setZone] = useState("");
  const [category, setCategory] = useState(0);
  const [status, setStatus] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Default color palette fallback configuration for form styling
  const darkColors = colors || {
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
  };

  // Adapted from: React Docs (2024b) - "Form Components"
  // Handles form submission, performs MAC address regex validation, and dispatches API payload
  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);
    setIsSubmitting(true);

    if (!mac.trim() || !zone.trim()) {
      setStatus({ type: "error", message: "MAC address and zone are required." });
      setIsSubmitting(false);
      return;
    }

    // Adapted from: Developer Mozilla (2024a) - "RegExp"
    // Validates MAC address structure against colon-separated hex byte sequences
    const macPattern = /^([0-9A-Fa-f]{2}:){2,}[0-9A-Fa-f]{2}$/;
    if (!macPattern.test(mac.trim())) {
      setStatus({ type: "error", message: "MAC address must be colon-separated hex pairs, e.g. AA:BB:01." });
      setIsSubmitting(false);
      return;
    }

    try {
      await registerSensor({
        deviceMacAddress: mac.trim().toUpperCase(),
        zone: zone.trim(),
        category: Number(category),
      });
      setStatus({ type: "success", message: `Sensor ${mac} registered successfully.` });
      setMac("");
      setZone("");
      onRegistered?.();
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    } finally {
      setIsSubmitting(false);
    }
  }

  // Adapted from: W3Schools (2024) - "React Forms"
  // Renders registration form components with event handlers and dynamic focus styling
  return (
    <form onSubmit={handleSubmit}>
      <h3 style={{ marginTop: 0, marginBottom: 14, fontSize: 15, color: darkColors.text }}>
        Add New Sensor
      </h3>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: 14, marginBottom: 14 }}>
        <div>
          <label style={{ fontSize: 12, fontWeight: 600, color: darkColors.textSecondary, display: 'block', marginBottom: 4 }}>
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
              boxSizing: 'border-box',
              transition: 'border-color 0.2s',
            }}
            onFocus={(e) => e.target.style.borderColor = darkColors.primary}
            onBlur={(e) => e.target.style.borderColor = darkColors.border}
          />
        </div>

        <div>
          <label style={{ fontSize: 12, fontWeight: 600, color: darkColors.textSecondary, display: 'block', marginBottom: 4 }}>
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
              boxSizing: 'border-box',
              transition: 'border-color 0.2s',
            }}
            onFocus={(e) => e.target.style.borderColor = darkColors.primary}
            onBlur={(e) => e.target.style.borderColor = darkColors.border}
          />
        </div>

        <div>
          <label style={{ fontSize: 12, fontWeight: 600, color: darkColors.textSecondary, display: 'block', marginBottom: 4 }}>
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
              boxSizing: 'border-box',
              cursor: 'pointer',
            }}
          >
            {CATEGORY_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label} - {opt.description}</option>
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
          width: '100%',
        }}
      >
        {isSubmitting ? 'Registering...' : 'Register Sensor'}
      </button>

      {status && (
        <p style={{ marginTop: 10, color: status.type === "error" ? '#f85149' : '#3fb950', fontSize: 13, fontWeight: 500 }}>
          {status.message}
        </p>
      )}
    </form>
  );
}

/*
References:
Developer Mozilla, 2024a. RegExp. MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/RegExp [Accessed 3 September 2026].
Developer Mozilla, 2024b. Array.prototype.map(). MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/map [Accessed 1 September 2026].
React Docs, 2024a. Sharing State Between Components: Controlled and Uncontrolled Components. React Documentation. Available at: https://react.dev/learn/sharing-state-between-components#controlled-and-uncontrolled-components [Accessed 7 September 2026].
React Docs, 2024b. Form Components. React Documentation. Available at: https://react.dev/reference/react-dom/components/input [Accessed 5 September 2026].
W3Schools, 2024. React Forms. W3Schools. Available at: https://www.w3schools.com/react/react_forms.asp [Accessed 2 September 2026].
*/