const PILLARS = [
  {
    key: "ingestion",
    label: "Sensor Data Ingestion",
    description: "Monitor and manage all connected sensors in real-time",
    enabled: true,
  },
  {
    key: "commands",
    label: "Real-Time Command Stream",
    description: "Send and track device commands across the network",
    enabled: false,
  },
  {
    key: "topology",
    label: "Network Topology & Mesh Routing",
    description: "Visualise and optimise your mesh network performance",
    enabled: false,
  },
];

const colors = {
  background: "#0d1117",
  surface: "#161b22",
  surface3: "#21262d",
  border: "#30363d",
  text: "#e6edf3",
  textSecondary: "#8b949e",
  primary: "#58a6ff",
  success: "#3fb950",
};

export default function LandingPage({ onSelect }) {
  return (
    <div
      style={{
        minHeight: "100vh",
        backgroundColor: colors.background,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: "system-ui, -apple-system, sans-serif",
        padding: 20,
      }}
    >
      {/* Adapted from Conditional Inline Styling (React Docs, 2024b) */}
      <style>{`
        .pillar-btn:not(:disabled):hover {
          transform: translateX(6px);
          border-color: ${colors.primary};
          background-color: ${colors.surface3};
        }
      `}</style>

      <div style={{ maxWidth: 640, width: "100%", padding: "40px 32px" }}>
        <div style={{ textAlign: "center", marginBottom: 40 }}>
          <h1 style={{ fontSize: 32, fontWeight: 700, color: colors.text, margin: "0 0 8px" }}>
            Smart-X IoT Ecosystem
          </h1>
          <p style={{ fontSize: 15, color: colors.textSecondary, margin: 0 }}>
            Select a module to start monitoring your IoT network
          </p>
        </div>

        {/* Pillar Cards - Adapted from Array Mapping & Dynamic List Rendering (React Docs, 2024a) */}
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {PILLARS.map((pillar) => (
            <button
              key={pillar.key}
              className="pillar-btn"
              disabled={!pillar.enabled}
              onClick={() => onSelect(pillar.key)}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: 16,
                padding: "18px 24px",
                borderRadius: 10,
                border: `1px solid ${colors.border}`,
                backgroundColor: colors.surface,
                cursor: pillar.enabled ? "pointer" : "not-allowed",
                width: "100%",
                transition: "all 0.2s ease",
                opacity: pillar.enabled ? 1 : 0.45,
                textAlign: "left",
              }}
            >
              <div>
                <div style={{ fontSize: 16, fontWeight: 600, color: colors.text }}>
                  {pillar.label}
                </div>
                <div style={{ fontSize: 13, color: colors.textSecondary, marginTop: 2 }}>
                  {pillar.description}
                </div>
              </div>

              <span
                style={{
                  fontSize: 12,
                  fontWeight: 600,
                  padding: "4px 14px",
                  borderRadius: 20,
                  whiteSpace: "nowrap",
                  color: pillar.enabled ? colors.success : colors.textSecondary,
                  backgroundColor: pillar.enabled ? "rgba(63, 185, 80, 0.15)" : colors.surface3,
                }}
              >
                {pillar.enabled ? "Available" : "Coming Soon"}
              </span>
            </button>
          ))}
        </div>

        <p style={{ textAlign: "center", marginTop: 32, fontSize: 12, color: colors.textSecondary, opacity: 0.5 }}>
          Smart-X IoT Ecosystem v1.0
        </p>
      </div>
    </div>
  );
}

/*
References:
MDN Web Docs, 2024. Element: mouseenter event. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/API/Element/mouseenter_event [Accessed 10 September 2026].
React Docs, 2024a. Rendering Lists. React Documentation. Available at: https://react.dev/learn/rendering-lists [Accessed 10 September 2026].
React Docs, 2024b. DOM Elements: style. React Documentation. Available at: https://react.dev/reference/react-dom/components/common#style [Accessed 10 September 2026].
*/