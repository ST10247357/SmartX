import { useEffect, useState } from 'react';
import { getGamificationStats, getAlertHistory } from '../api/client';

interface GamificationStats {
  totalCriticalAlerts: number;
  resolvedAlerts: number;
  quickResolutions: number;
  stabilityStreakHours: number;
  healthScore: number;
  sensorResolutionCounts: Record<string, number>;
}

interface AlertRecord {
  deviceMacAddress: string;
  severity: number;
  detectedAt: string;
  resolvedAt: string | null;
  valueAtDetection: number;
  valueAtResolution: number | null;
  isResolved: boolean;
  isQuickResolution: boolean;
}

export default function GamificationStats() {
  const [stats, setStats] = useState<GamificationStats | null>(null);
  const [alerts, setAlerts] = useState<AlertRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadGamificationData();
    const interval = setInterval(loadGamificationData, 30000); // Refresh every 30s
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

  if (loading) return <div>Loading gamification stats...</div>;
  if (!stats) return <div>No stats available</div>;

  const quickRate = stats.totalCriticalAlerts > 0 
    ? Math.round((stats.quickResolutions / stats.totalCriticalAlerts) * 100)
    : 100;

  // Health score color
  const getHealthColor = (score: number) => {
    if (score >= 80) return '#2e7d32'; // Green
    if (score >= 60) return '#f9a825'; // Yellow
    return '#c62828'; // Red
  };

  return (
    <div style={{
      display: 'grid',
      gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
      gap: 16,
      marginBottom: 24,
      padding: 16,
      backgroundColor: '#f5f5f5',
      borderRadius: 8,
    }}>
      {/* Health Score */}
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 12, color: '#666', fontWeight: 600 }}>System Health</div>
        <div style={{
          fontSize: 48,
          fontWeight: 700,
          color: getHealthColor(stats.healthScore),
        }}>
          {stats.healthScore}
        </div>
        <div style={{ fontSize: 12, color: '#666' }}>/ 100</div>
      </div>

      {/* Stability Streak */}
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 12, color: '#666', fontWeight: 600 }}>Stability Streak</div>
        <div style={{ fontSize: 36, fontWeight: 700 }}>
          {stats.stabilityStreakHours}
        </div>
        <div style={{ fontSize: 12, color: '#666' }}>hours without critical alert</div>
        {stats.stabilityStreakHours >= 24 && (
          <div style={{ fontSize: 11, color: '#2e7d32', marginTop: 4 }}>
            🌟 Excellent!
          </div>
        )}
      </div>

      {/* Quick Response Rate */}
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 12, color: '#666', fontWeight: 600 }}>Quick Response Rate</div>
        <div style={{ fontSize: 36, fontWeight: 700 }}>
          {quickRate}%
        </div>
        <div style={{ fontSize: 12, color: '#666' }}>
          resolved within 5 minutes
        </div>
        {quickRate >= 80 && (
          <div style={{ fontSize: 11, color: '#2e7d32', marginTop: 4 }}>
            ⚡ Fast response!
          </div>
        )}
      </div>

      {/* Critical Alerts */}
      <div style={{ textAlign: 'center' }}>
        <div style={{ fontSize: 12, color: '#666', fontWeight: 600 }}>Critical Alerts</div>
        <div style={{ fontSize: 36, fontWeight: 700, color: '#c62828' }}>
          {stats.totalCriticalAlerts}
        </div>
        <div style={{ fontSize: 12, color: '#666' }}>
          {stats.resolvedAlerts} resolved
        </div>
      </div>

      {/* Recent Alerts (compact list) */}
      <div style={{ gridColumn: '1 / -1', marginTop: 8 }}>
        <details>
          <summary style={{ cursor: 'pointer', fontWeight: 600, color: '#555' }}>
            Recent Alerts ({alerts.filter(a => !a.isResolved).length} active)
          </summary>
          <div style={{ maxHeight: 150, overflowY: 'auto', marginTop: 8 }}>
            {alerts.slice(0, 10).map((alert, index) => (
              <div key={index} style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '4px 8px',
                fontSize: 12,
                borderBottom: '1px solid #eee',
                backgroundColor: alert.isResolved ? '#e8f5e9' : '#ffebee',
              }}>
                <span>{alert.deviceMacAddress}</span>
                <span>
                  {alert.isResolved 
                    ? `✅ Resolved ${alert.isQuickResolution ? '(quick)' : ''}` 
                    : '⚠️ Active'
                  }
                </span>
              </div>
            ))}
          </div>
        </details>
      </div>
    </div>
  );
}