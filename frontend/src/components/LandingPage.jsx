const PILLARS = [
  { 
    key: "ingestion", 
    label: "Sensor Data Ingestion", 
    description: "Monitor and manage all connected sensors in real-time",
    icon: "📡",
    enabled: true 
  },
  { 
    key: "commands", 
    label: "Real-Time Command Stream", 
    description: "Send and track device commands across the network",
    icon: "⚡",
    enabled: false 
  },
  { 
    key: "topology", 
    label: "Network Topology & Mesh Routing", 
    description: "Visualize and optimize your mesh network performance",
    icon: "🌐",
    enabled: false 
  },
];

export default function LandingPage({ onSelect }) {
  const colors = {
    background: '#0d1117',
    surface: '#161b22',
    surface2: '#1c2333',
    surface3: '#21262d',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
    success: '#3fb950',
  };

  return (
    <div style={{
      minHeight: '100vh',
      backgroundColor: colors.background,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontFamily: 'system-ui, -apple-system, sans-serif',
      padding: 20
    }}>
      <div style={{
        maxWidth: 800,
        width: '100%',
        padding: '40px 32px'
      }}>
        {/* Header */}
        <div style={{ textAlign: 'center', marginBottom: 40 }}>
          <div style={{
            fontSize: 64,
            marginBottom: 8,
            display: 'inline-block',
            background: 'linear-gradient(135deg, #58a6ff, #3fb950)',
            borderRadius: '50%',
            padding: '20px',
            width: 100,
            height: 100,
            lineHeight: '60px'
          }}>
            🌿
          </div>
          <h1 style={{
            fontSize: 36,
            fontWeight: 700,
            color: colors.text,
            margin: '16px 0 8px'
          }}>
            Smart-X IoT Ecosystem
          </h1>
          <p style={{
            fontSize: 16,
            color: colors.textSecondary,
            margin: 0
          }}>
            Select a module to start monitoring your IoT network
          </p>
        </div>

        {/* Pillar Cards */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {PILLARS.map((pillar) => (
            <button
              key={pillar.key}
              disabled={!pillar.enabled}
              onClick={() => onSelect(pillar.key)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 20,
                padding: '20px 28px',
                borderRadius: 12,
                border: pillar.enabled 
                  ? `1px solid ${colors.border}` 
                  : `1px solid ${colors.border}`,
                backgroundColor: pillar.enabled 
                  ? colors.surface 
                  : colors.surface2,
                cursor: pillar.enabled ? 'pointer' : 'not-allowed',
                width: '100%',
                transition: 'all 0.25s ease',
                opacity: pillar.enabled ? 1 : 0.4,
                textAlign: 'left'
              }}
              onMouseEnter={(e) => {
                if (pillar.enabled) {
                  e.currentTarget.style.transform = 'translateX(8px)';
                  e.currentTarget.style.borderColor = colors.primary;
                  e.currentTarget.style.backgroundColor = colors.surface3;
                  e.currentTarget.style.boxShadow = '0 8px 32px rgba(0,0,0,0.3)';
                }
              }}
              onMouseLeave={(e) => {
                if (pillar.enabled) {
                  e.currentTarget.style.transform = 'translateX(0)';
                  e.currentTarget.style.borderColor = colors.border;
                  e.currentTarget.style.backgroundColor = colors.surface;
                  e.currentTarget.style.boxShadow = 'none';
                }
              }}
            >
              <div style={{ fontSize: 36, flexShrink: 0 }}>{pillar.icon}</div>
              <div style={{ flex: 1 }}>
                <div style={{
                  fontSize: 18,
                  fontWeight: 600,
                  color: pillar.enabled ? colors.text : colors.textSecondary,
                  marginBottom: 4
                }}>
                  {pillar.label}
                </div>
                <div style={{
                  fontSize: 14,
                  color: pillar.enabled ? colors.textSecondary : colors.textSecondary,
                  opacity: pillar.enabled ? 1 : 0.6
                }}>
                  {pillar.description}
                </div>
              </div>
              <div>
                {pillar.enabled ? (
                  <span style={{
                    fontSize: 12,
                    color: colors.success,
                    backgroundColor: 'rgba(63, 185, 80, 0.15)',
                    padding: '4px 16px',
                    borderRadius: 20,
                    fontWeight: 600,
                    whiteSpace: 'nowrap'
                  }}>
                    Available →
                  </span>
                ) : (
                  <span style={{
                    fontSize: 12,
                    color: colors.textSecondary,
                    backgroundColor: colors.surface2,
                    padding: '4px 16px',
                    borderRadius: 20,
                    fontWeight: 500,
                    whiteSpace: 'nowrap'
                  }}>
                    Coming Soon
                  </span>
                )}
              </div>
            </button>
          ))}
        </div>

        {/* Footer */}
        <p style={{
          textAlign: 'center',
          marginTop: 36,
          fontSize: 13,
          color: colors.textSecondary,
          opacity: 0.5
        }}>
          Smart-X IoT Ecosystem v1.0 • Powered by .NET 10 & React
        </p>
      </div>
    </div>
  );
}