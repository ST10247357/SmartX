import { useState, useEffect } from 'react';
import { getGamificationStats, resolveAlert } from '../api/client';

// Severity mapping
const SEVERITY = {
  0: { label: 'Normal', color: '#2e7d32', bg: '#e8f5e9' },
  1: { label: 'Warning', color: '#f9a825', bg: '#fff8e1' },
  2: { label: 'Critical', color: '#c62828', bg: '#ffebee' },
};

// Category labels
const CATEGORY_LABELS = {
  0: 'Environmental',
  1: 'Power Consumption',
  2: 'Actuator',
};

export default function SensorTile({ sensor }) {
  const [showDetails, setShowDetails] = useState(false);
  const [resolutionCount, setResolutionCount] = useState(0);
  const [isResolving, setIsResolving] = useState(false);
  
  const severity = SEVERITY[sensor.currentSeverity] ?? SEVERITY[0];

  // Calculate "star rating" based on resolution history
  // In a real system, this would come from the backend
  // For demo purposes, we use a combination of severity and random factor
  const getStarRating = () => {
    // Base stars on severity level
    const baseStars = sensor.currentSeverity === 0 ? 5 : 
                     sensor.currentSeverity === 1 ? 3 : 1;
    
    // Add bonus for quick resolution history (if any)
    const bonusStars = Math.min(2, Math.floor(resolutionCount / 3));
    
    // Ensure we don't exceed 5
    return Math.min(5, baseStars + bonusStars);
  };
  
  const starRating = getStarRating();
  const stars = '⭐'.repeat(starRating) + '☆'.repeat(5 - starRating);

  // Handle resolving an alert
  const handleResolve = async (e) => {
    e.stopPropagation();
    setIsResolving(true);
    
    try {
      await resolveAlert(sensor.deviceMacAddress);
      // Increment resolution count for star rating
      setResolutionCount(prev => prev + 1);
      // Refresh the page after a short delay
      setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
      console.error('Failed to resolve alert:', error);
      setIsResolving(false);
    }
  };

  // Load gamification stats when component mounts
  useEffect(() => {
    const loadStats = async () => {
      try {
        const stats = await getGamificationStats();
        // Check if this sensor has resolution history
        if (stats.sensorResolutionCounts && stats.sensorResolutionCounts[sensor.deviceMacAddress]) {
          setResolutionCount(stats.sensorResolutionCounts[sensor.deviceMacAddress]);
        }
      } catch (error) {
        console.error('Failed to load gamification stats:', error);
      }
    };
    
    loadStats();
  }, [sensor.deviceMacAddress]);

  // Determine if this is a quick response
  const isQuickResponse = sensor.currentSeverity === 0 && 
    sensor.lastSeen && 
    new Date().getTime() - new Date(sensor.lastSeen).getTime() < 300000;

  // Get appropriate emoji for sensor status
  const getStatusEmoji = () => {
    if (sensor.currentSeverity === 0) return '✅';
    if (sensor.currentSeverity === 1) return '⚠️';
    return '🚨';
  };

  return (
    <div
      style={{
        border: `2px solid ${severity.color}`,
        backgroundColor: severity.bg,
        borderRadius: 8,
        padding: 16,
        minWidth: 220,
        cursor: 'pointer',
        transition: 'transform 0.2s, box-shadow 0.2s',
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'scale(1.02)';
        e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'scale(1)';
        e.currentTarget.style.boxShadow = 'none';
      }}
      onClick={() => setShowDetails(!showDetails)}
    >
      {/* Header with status emoji */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ fontWeight: 600, fontSize: 14 }}>{sensor.deviceMacAddress}</div>
        <div style={{ fontSize: 16 }}>{getStatusEmoji()}</div>
      </div>
      
      <div style={{ fontSize: 12, color: '#555' }}>{sensor.zone}</div>
      <div style={{ fontSize: 12, color: '#555', marginBottom: 8 }}>
        {CATEGORY_LABELS[sensor.category] || 'Unknown'}
      </div>

      <div style={{ fontSize: 24, fontWeight: 700 }}>
        {sensor.lastReading ?? '—'}
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 8 }}>
        <div
          style={{
            display: 'inline-block',
            padding: '2px 10px',
            borderRadius: 12,
            backgroundColor: severity.color,
            color: '#fff',
            fontSize: 12,
            fontWeight: 600,
          }}
        >
          {severity.label}
        </div>
        
        {/* Star Rating - Professional Gamification */}
        <div style={{ fontSize: 12, letterSpacing: 1 }} title={`${starRating} out of 5 stars`}>
          {stars}
        </div>
      </div>

      {/* Quick response indicator - subtle */}
      {isQuickResponse && (
        <div style={{ fontSize: 10, color: '#2e7d32', marginTop: 4, textAlign: 'right' }}>
          ⚡ Quick Response
        </div>
      )}

      {/* Resolution count badge */}
      {resolutionCount > 0 && (
        <div style={{ 
          fontSize: 10, 
          color: '#555', 
          marginTop: 4, 
          textAlign: 'right',
          opacity: 0.7
        }}>
          🏆 {resolutionCount} resolution{resolutionCount > 1 ? 's' : ''}
        </div>
      )}

      {/* Expanded details (on click) */}
      {showDetails && (
        <div style={{ marginTop: 12, padding: 8, backgroundColor: 'white', borderRadius: 4, fontSize: 12 }}>
          <div><strong>Category:</strong> {CATEGORY_LABELS[sensor.category] || 'Unknown'}</div>
          <div><strong>Last seen:</strong> {sensor.lastSeen ? new Date(sensor.lastSeen).toLocaleString() : 'Never'}</div>
          <div><strong>Severity:</strong> {sensor.currentSeverity === 0 ? 'Normal' : sensor.currentSeverity === 1 ? 'Warning' : 'Critical'}</div>
          <div><strong>Star Rating:</strong> {starRating} / 5</div>
          <div><strong>Resolutions:</strong> {resolutionCount}</div>
          
          {/* Quick Response Achievement */}
          {isQuickResponse && (
            <div style={{ marginTop: 4, color: '#2e7d32' }}>
              ✅ Quick Resolution Achievement!
            </div>
          )}
          
          {/* Resolve button for critical/warning alerts */}
          {sensor.currentSeverity > 0 && (
            <button
              onClick={handleResolve}
              disabled={isResolving}
              style={{
                marginTop: 8,
                padding: '4px 12px',
                backgroundColor: isResolving ? '#999' : '#2e7d32',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: isResolving ? 'not-allowed' : 'pointer',
                fontSize: 12,
                width: '100%',
              }}
            >
              {isResolving ? 'Resolving...' : '✅ Mark as Resolved'}
            </button>
          )}
          
          {/* Achievement badge for normal status */}
          {sensor.currentSeverity === 0 && resolutionCount > 0 && (
            <div style={{ 
              marginTop: 8, 
              padding: '4px 8px', 
              backgroundColor: '#e8f5e9', 
              borderRadius: 4,
              color: '#2e7d32',
              fontSize: 11,
              textAlign: 'center'
            }}>
              🌟 Achievement: System Guardian
            </div>
          )}
        </div>
      )}
    </div>
  );
}