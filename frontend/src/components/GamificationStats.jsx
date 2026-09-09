import { useEffect, useState } from 'react';
import { getGamificationStats, getAlertHistory } from '../api/client';

export default function GamificationStats({ colors }) {
  const [stats, setStats] = useState(null);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);

  const darkColors = colors || {
    surface: '#161b22',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
  };

  useEffect(() => {
    loadGamificationData();
    const interval = setInterval(loadGamificationData, 10000);
    return () => clearInterval(interval);
  }, []);

  async function loadGamificationData() {
    try {
      const [statsData, alertsData] = await Promise.all([
        getGamificationStats(),
        getAlertHistory()
      ]);
      setStats(statsData);
      setAlerts(alertsData);
    } catch (error) {
      console.error('Failed to load gamification data:', error);
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: 16, color: darkColors.textSecondary, fontSize: 13 }}>
        🔄 Loading stats...
      </div>
    );
  }
  
  if (!stats) return null;

  const getHealthColor = (score) => {
    if (score >= 80) return '#3fb950';
    if (score >= 60) return '#d29922';
    return '#f85149';
  };

  const activeAlerts = alerts.filter(a => !a.isResolved).length;

  return (
    <div style={{
      backgroundColor: darkColors.surface,
      borderRadius: 12,
      padding: '16px 20px',
      marginBottom: 20,
      border: `1px solid ${darkColors.border}`
    }}>
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
        gap: 16,
        textAlign: 'center'
      }}>
        <div style={{ padding: '8px' }}>
          <div style={{ fontSize: 11, color: darkColors.textSecondary, fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
            System Health
          </div>
          <div style={{
            fontSize: 32,
            fontWeight: 700,
            color: getHealthColor(stats.healthScore)
          }}>
            {stats.healthScore}
          </div>
          <div style={{ fontSize: 10, color: darkColors.textSecondary }}>/ 100</div>
        </div>

        <div style={{ padding: '8px' }}>
          <div style={{ fontSize: 11, color: darkColors.textSecondary, fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
            Stability Streak
          </div>
          <div style={{ fontSize: 28, fontWeight: 700, color: '#58a6ff' }}>
            {stats.stabilityStreakHours}
          </div>
          <div style={{ fontSize: 10, color: darkColors.textSecondary }}>hours without critical alert</div>
          {stats.stabilityStreakHours >= 24 && (
            <div style={{ fontSize: 11, color: '#3fb950', marginTop: 4 }}>🌟 Excellent!</div>
          )}
        </div>

        <div style={{ padding: '8px' }}>
          <div style={{ fontSize: 11, color: darkColors.textSecondary, fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
            Critical Alerts
          </div>
          <div style={{ fontSize: 28, fontWeight: 700, color: '#f85149' }}>
            {activeAlerts}
          </div>
          <div style={{ fontSize: 10, color: darkColors.textSecondary }}>
            {stats.resolvedAlerts} resolved • {stats.totalCriticalAlerts} total
          </div>
        </div>
      </div>

      {alerts.length > 0 && (
        <details style={{ marginTop: 12, borderTop: `1px solid ${darkColors.border}`, paddingTop: 12 }}>
          <summary style={{
            cursor: 'pointer',
            fontWeight: 600,
            color: darkColors.textSecondary,
            fontSize: 12,
            userSelect: 'none'
          }}>
            📋 Recent Alerts ({activeAlerts} active)
          </summary>
          <div style={{ maxHeight: 100, overflowY: 'auto', marginTop: 8 }}>
            {alerts.slice(0, 10).map((alert, index) => (
              <div key={index} style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '4px 10px',
                fontSize: 11,
                borderBottom: `1px solid ${darkColors.border}`,
                backgroundColor: alert.isResolved ? 'rgba(63, 185, 80, 0.08)' : 'rgba(248, 81, 73, 0.08)',
                borderRadius: index === 0 ? '4px 4px 0 0' : '0'
              }}>
                <span style={{ color: darkColors.text }}>{alert.deviceMacAddress}</span>
                <span>
                  {alert.isResolved 
                    ? `✅ Resolved` 
                    : '⚠️ Active'
                  }
                </span>
              </div>
            ))}
          </div>
        </details>
      )}
    </div>
  );
}