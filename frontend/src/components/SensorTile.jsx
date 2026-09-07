const SEVERITY = {
  0: { label: "Normal", color: "#2e7d32", bg: "#e8f5e9" },
  1: { label: "Warning", color: "#f9a825", bg: "#fff8e1" },
  2: { label: "Critical", color: "#c62828", bg: "#ffebee" },
};

const CATEGORY_LABELS = {
  0: "Environmental",
  1: "Power Consumption",
  2: "Actuator",
};

export default function SensorTile({ sensor }) {
  const severity = SEVERITY[sensor.currentSeverity] ?? SEVERITY[0];

  return (
    <div
      style={{
        border: `2px solid ${severity.color}`,
        backgroundColor: severity.bg,
        borderRadius: 8,
        padding: 16,
        minWidth: 220,
      }}
    >
      <div style={{ fontWeight: 600, fontSize: 14 }}>{sensor.deviceMacAddress}</div>
      <div style={{ fontSize: 12, color: "#555" }}>{sensor.zone}</div>
      <div style={{ fontSize: 12, color: "#555", marginBottom: 8 }}>
        {CATEGORY_LABELS[sensor.category]}
      </div>

      <div style={{ fontSize: 24, fontWeight: 700 }}>
        {sensor.lastReading ?? "—"}
      </div>

      <div
        style={{
          display: "inline-block",
          marginTop: 8,
          padding: "2px 10px",
          borderRadius: 12,
          backgroundColor: severity.color,
          color: "#fff",
          fontSize: 12,
          fontWeight: 600,
        }}
      >
        {severity.label}
      </div>
    </div>
  );
}
