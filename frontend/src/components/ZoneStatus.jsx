import { useEffect, useState } from 'react';
import { getZoneStatus } from '../api/client';

export default function ZoneStatus({ colors }) {
  const [zoneStatus, setZoneStatus] = useState(null);
  const [loading, setLoading] = useState(true);

  const darkColors = colors || {
    surface: '#161b22',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    success: '#3fb950',
    danger: '#f85149',
  };

  useEffect(() => {
    loadZoneStatus();
    const interval = setInterval(loadZoneStatus, 10000);
    return () => clearInterval(interval);
  }, []);

  async function loadZoneStatus() {
    try {
      const data = await getZoneStatus();
      setZoneStatus(data);
    } catch (error) {
      console.error('Failed to load zone status:', error);
    } finally {
      setLoading(false);
    }
  }

  if (loading) return null;
  if (!zoneStatus) return null;

  return (
    <div style={{
      backgroundColor: zoneStatus.isValid ? 'rgba(63, 185, 80, 0.08)' : 'rgba(248, 81, 73, 0.08)',
      borderRadius: 10,
      padding: '12px 20px',
      marginBottom: 20,
      border: `1px solid ${zoneStatus.isValid ? darkColors.success : darkColors.danger}`,
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      flexWrap: 'wrap',
      gap: 8
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <span style={{ fontSize: 18 }}>
          {zoneStatus.isValid ? '✅' : '⚠️'}
        </span>
        <span style={{ color: darkColors.text, fontSize: 14 }}>
          <strong style={{ fontWeight: 600 }}>
            {zoneStatus.isValid ? 'All zones operational' : 'Zone issue detected'}
          </strong>
          {!zoneStatus.isValid && zoneStatus.invalidZone && (
            <span style={{
              marginLeft: 10,
              backgroundColor: 'rgba(248, 81, 73, 0.15)',
              color: darkColors.danger,
              padding: '2px 12px',
              borderRadius: 12,
              fontSize: 12
            }}>
              {zoneStatus.invalidZone}
            </span>
          )}
        </span>
      </div>
      <div style={{ fontSize: 12, color: darkColors.textSecondary }}>
        🔄 {zoneStatus.nodeCount} zones • Recursive validation
      </div>
    </div>
  );
}