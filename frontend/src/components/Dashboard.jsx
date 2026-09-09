import { useEffect, useState } from "react";
import { getSensors } from "../api/client";
import SensorTable from "./SensorTable";
import RegisterSensorForm from "./RegisterSensorForm";
import FileUploadForm from "./FileUploadForm";
import GamificationStats from "./GamificationStats";
import ZoneStatus from "./ZoneStatus";

export default function Dashboard() {
  const [sensors, setSensors] = useState([]);
  const [error, setError] = useState(null);
  const [showGamification, setShowGamification] = useState(true);
  const [isLoading, setIsLoading] = useState(true);
  const [lastRefresh, setLastRefresh] = useState(null);

  const colors = {
    background: '#0d1117',
    surface: '#161b22',
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
    success: '#3fb950',
    warning: '#d29922',
    danger: '#f85149',
  };

  async function loadSensors() {
    try {
      const data = await getSensors();
      setSensors(data);
      setLastRefresh(new Date());
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadSensors();
    const interval = setInterval(loadSensors, 5000);
    return () => clearInterval(interval);
  }, []);

  const criticalCount = sensors.filter((s) => s.currentSeverity === 2).length;
  const warningCount = sensors.filter((s) => s.currentSeverity === 1).length;
  const normalCount = sensors.filter((s) => s.currentSeverity === 0).length;

  return (
    <div style={{
      minHeight: '100vh',
      backgroundColor: colors.background,
      color: colors.text,
      fontFamily: 'system-ui, -apple-system, sans-serif'
    }}>
      <header style={{
        backgroundColor: colors.surface,
        borderBottom: `1px solid ${colors.border}`,
        padding: '16px 32px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: 12
      }}>
        <div>
          <h1 style={{ margin: 0, fontSize: 20, fontWeight: 700, color: colors.text }}>
            🌿 Smart-X IoT
          </h1>
          <p style={{ margin: '2px 0 0', color: colors.textSecondary, fontSize: 13 }}>
            {sensors.length} sensors • {criticalCount} critical
            {lastRefresh && (
              <span style={{ marginLeft: 12, fontSize: 11, opacity: 0.6 }}>
                Updated: {lastRefresh.toLocaleTimeString()}
              </span>
            )}
          </p>
        </div>
        <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', gap: 6 }}>
            <span style={{
              padding: '4px 12px',
              borderRadius: 12,
              backgroundColor: 'rgba(63, 185, 80, 0.15)',
              color: colors.success,
              fontSize: 12,
              fontWeight: 600
            }}>
              ✅ {normalCount}
            </span>
            <span style={{
              padding: '4px 12px',
              borderRadius: 12,
              backgroundColor: 'rgba(210, 153, 34, 0.15)',
              color: colors.warning,
              fontSize: 12,
              fontWeight: 600
            }}>
              ⚠️ {warningCount}
            </span>
            <span style={{
              padding: '4px 12px',
              borderRadius: 12,
              backgroundColor: 'rgba(248, 81, 73, 0.15)',
              color: colors.danger,
              fontSize: 12,
              fontWeight: 600
            }}>
              🚨 {criticalCount}
            </span>
          </div>
          
          <button
            onClick={() => setShowGamification(!showGamification)}
            style={{
              padding: '6px 14px',
              borderRadius: 6,
              border: `1px solid ${colors.border}`,
              cursor: 'pointer',
              backgroundColor: showGamification ? colors.surface2 : 'transparent',
              color: colors.text,
              fontSize: 12,
              transition: 'all 0.2s'
            }}
          >
            {showGamification ? '🏆 Stats' : '📊 Stats'}
          </button>
          
          <button
            onClick={() => loadSensors()}
            style={{
              padding: '6px 14px',
              borderRadius: 6,
              border: `1px solid ${colors.border}`,
              cursor: 'pointer',
              backgroundColor: colors.surface2,
              color: colors.text,
              fontSize: 12,
              transition: 'all 0.2s'
            }}
            onMouseEnter={(e) => e.currentTarget.style.backgroundColor = colors.surface}
            onMouseLeave={(e) => e.currentTarget.style.backgroundColor = colors.surface2}
          >
            🔄
          </button>
        </div>
      </header>

      <main style={{ maxWidth: 1400, margin: '0 auto', padding: '24px 32px' }}>
        
        <div style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: 20,
          marginBottom: 24
        }}>
          <div style={{
            backgroundColor: colors.surface,
            borderRadius: 12,
            padding: '20px 24px',
            border: `1px solid ${colors.border}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <span style={{ fontSize: 18 }}>➕</span>
              <h3 style={{ margin: 0, fontSize: 15, color: colors.text }}>Add New Sensor</h3>
            </div>
            <RegisterSensorForm onRegistered={loadSensors} colors={colors} />
          </div>
          
          <div style={{
            backgroundColor: colors.surface,
            borderRadius: 12,
            padding: '20px 24px',
            border: `1px solid ${colors.border}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <span style={{ fontSize: 18 }}>📎</span>
              <h3 style={{ margin: 0, fontSize: 15, color: colors.text }}>Attach File</h3>
            </div>
            <FileUploadForm sensors={sensors} colors={colors} />
          </div>
        </div>

        {error && (
          <div style={{
            backgroundColor: 'rgba(248, 81, 73, 0.1)',
            color: colors.danger,
            padding: '10px 16px',
            borderRadius: 8,
            marginBottom: 16,
            border: `1px solid ${colors.danger}`
          }}>
            ❌ {error}
          </div>
        )}

        {isLoading ? (
          <div style={{ textAlign: 'center', padding: 60, color: colors.textSecondary }}>
            <div style={{ fontSize: 32, marginBottom: 12 }}>🔄</div>
            <div>Loading sensors...</div>
          </div>
        ) : (
          <>
            <ZoneStatus colors={colors} />
            
            {showGamification && <GamificationStats colors={colors} />}

            <SensorTable 
              sensors={sensors} 
              colors={colors} 
            />
          </>
        )}
      </main>
    </div>
  );
}