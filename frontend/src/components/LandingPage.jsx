const PILLARS = [
  { key: "ingestion", label: "Sensor Data Ingestion and Telemetry", enabled: true },
  { key: "commands", label: "Real-Time Command Stream and History", enabled: false },
  { key: "topology", label: "Network Topology and Mesh Routing", enabled: false },
];

export default function LandingPage({ onSelect }) {
  return (
    <div style={{ padding: 24 }}>
      <h1>Smart-X IoT Mesh Ecosystem</h1>
      <p>Select a module to continue.</p>

      <div style={{ display: "flex", gap: 16, marginTop: 16 }}>
        {PILLARS.map((pillar) => (
          <button
            key={pillar.key}
            disabled={!pillar.enabled}
            onClick={() => onSelect(pillar.key)}
            style={{
              padding: "16px 24px",
              borderRadius: 8,
              border: "1px solid #ccc",
              cursor: pillar.enabled ? "pointer" : "not-allowed",
              backgroundColor: pillar.enabled ? "#fff" : "#f0f0f0",
              color: pillar.enabled ? "#000" : "#999",
              minWidth: 200,
            }}
          >
            {pillar.label}
            {!pillar.enabled && <div style={{ fontSize: 11, marginTop: 4 }}>Coming in later parts</div>}
          </button>
        ))}
      </div>
    </div>
  );
}
