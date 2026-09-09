import { useState, useEffect } from 'react';
import { getSeverity, getCategoryLabel, getStars } from '../api/client';

export default function SensorTable({ sensors, colors }) {
  const [sortField, setSortField] = useState('deviceMacAddress');
  const [sortDirection, setSortDirection] = useState('asc');
  const [filterCategory, setFilterCategory] = useState('all');
  const [filterSeverity, setFilterSeverity] = useState('all');
  const [filterZone, setFilterZone] = useState('all');

  const darkColors = colors || {
    surface: '#161b22',
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
    success: '#3fb950',
    danger: '#f85149',
    warning: '#d29922',
  };

  const zones = ['all', ...new Set((sensors || []).map(s => s.zone))];

  const filteredSensors = (sensors || [])
    .filter(s => filterCategory === 'all' || s.category === parseInt(filterCategory))
    .filter(s => filterSeverity === 'all' || s.currentSeverity === parseInt(filterSeverity))
    .filter(s => filterZone === 'all' || s.zone === filterZone);

  const sortedSensors = [...filteredSensors].sort((a, b) => {
    let aVal = a[sortField];
    let bVal = b[sortField];
    
    if (aVal === undefined || aVal === null) aVal = '';
    if (bVal === undefined || bVal === null) bVal = '';
    
    if (typeof aVal === 'string') {
      return sortDirection === 'asc' 
        ? aVal.localeCompare(bVal) 
        : bVal.localeCompare(aVal);
    }
    return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
  });

  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (field) => {
    if (sortField !== field) return '↕';
    return sortDirection === 'asc' ? '↑' : '↓';
  };

  const getStarRating = (severity) => {
    const rating = severity === 0 ? 5 : severity === 1 ? 3 : 1;
    return getStars(rating);
  };

  const formatTime = (timestamp) => {
    if (!timestamp) return 'Never';
    try {
      const date = new Date(timestamp);
      return date.toLocaleTimeString();
    } catch {
      return 'Invalid';
    }
  };

  if (!sensors || sensors.length === 0) {
    return (
      <div style={{
        backgroundColor: darkColors.surface,
        borderRadius: 12,
        padding: 60,
        textAlign: 'center',
        border: `1px solid ${darkColors.border}`,
        color: darkColors.textSecondary
      }}>
        <div style={{ fontSize: 48, marginBottom: 16 }}>📡</div>
        <h3 style={{ margin: 0, color: darkColors.text }}>No Sensors Registered</h3>
        <p style={{ margin: '8px 0 0' }}>Register a sensor using the form above to get started.</p>
      </div>
    );
  }

  return (
    <div style={{
      backgroundColor: darkColors.surface,
      borderRadius: 12,
      border: `1px solid ${darkColors.border}`,
      overflow: 'hidden'
    }}>
      {/* Header */}
      <div style={{
        padding: '14px 20px',
        backgroundColor: darkColors.surface2,
        borderBottom: `1px solid ${darkColors.border}`,
        display: 'flex',
        flexWrap: 'wrap',
        gap: 10,
        alignItems: 'center'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 16 }}>📊</span>
          <span style={{ fontWeight: 600, fontSize: 14, color: darkColors.text }}>
            Sensor Data
          </span>
        </div>
        <span style={{ fontSize: 12, color: darkColors.textSecondary }}>
          {sortedSensors.length} of {sensors.length} sensors
        </span>
        
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginLeft: 'auto' }}>
          <select 
            value={filterCategory} 
            onChange={(e) => setFilterCategory(e.target.value)}
            style={{
              padding: '4px 10px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 12,
              outline: 'none',
              cursor: 'pointer',
              minWidth: 120
            }}
          >
            <option value="all">📋 All Categories</option>
            <option value="0">🌱 Environmental</option>
            <option value="1">⚡ Power</option>
            <option value="2">🔧 Actuator</option>
          </select>

          <select 
            value={filterSeverity} 
            onChange={(e) => setFilterSeverity(e.target.value)}
            style={{
              padding: '4px 10px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 12,
              outline: 'none',
              cursor: 'pointer',
              minWidth: 120
            }}
          >
            <option value="all">🎯 All Severities</option>
            <option value="0">✅ Normal</option>
            <option value="1">⚠️ Warning</option>
            <option value="2">🚨 Critical</option>
          </select>

          <select 
            value={filterZone} 
            onChange={(e) => setFilterZone(e.target.value)}
            style={{
              padding: '4px 10px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 12,
              outline: 'none',
              cursor: 'pointer',
              minWidth: 120
            }}
          >
            {zones.map(zone => (
              <option key={zone} value={zone}>
                📍 {zone === 'all' ? 'All Zones' : zone}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Table */}
      <div style={{ overflowX: 'auto' }}>
        <table style={{
          width: '100%',
          borderCollapse: 'collapse',
          fontSize: 13,
          color: darkColors.text
        }}>
          <thead>
            <tr style={{ backgroundColor: darkColors.surface2, borderBottom: `2px solid ${darkColors.border}` }}>
              <th 
                onClick={() => handleSort('deviceMacAddress')}
                style={{ padding: '10px 14px', textAlign: 'left', cursor: 'pointer', fontWeight: 600, color: darkColors.textSecondary }}
              >
                MAC Address {getSortIcon('deviceMacAddress')}
              </th>
              <th 
                onClick={() => handleSort('zone')}
                style={{ padding: '10px 14px', textAlign: 'left', cursor: 'pointer', fontWeight: 600, color: darkColors.textSecondary }}
              >
                Zone {getSortIcon('zone')}
              </th>
              <th 
                onClick={() => handleSort('category')}
                style={{ padding: '10px 14px', textAlign: 'left', cursor: 'pointer', fontWeight: 600, color: darkColors.textSecondary }}
              >
                Category {getSortIcon('category')}
              </th>
              <th 
                onClick={() => handleSort('lastReading')}
                style={{ padding: '10px 14px', textAlign: 'right', cursor: 'pointer', fontWeight: 600, color: darkColors.textSecondary }}
              >
                Reading {getSortIcon('lastReading')}
              </th>
              <th 
                onClick={() => handleSort('currentSeverity')}
                style={{ padding: '10px 14px', textAlign: 'center', cursor: 'pointer', fontWeight: 600, color: darkColors.textSecondary }}
              >
                Status {getSortIcon('currentSeverity')}
              </th>
              <th style={{ padding: '10px 14px', textAlign: 'center', fontWeight: 600, color: darkColors.textSecondary }}>
                Rating
              </th>
              <th style={{ padding: '10px 14px', textAlign: 'center', fontWeight: 600, color: darkColors.textSecondary }}>
                Last Updated
              </th>
            </tr>
          </thead>
          <tbody>
            {sortedSensors.length === 0 ? (
              <tr>
                <td colSpan="7" style={{ padding: '40px', textAlign: 'center', color: darkColors.textSecondary }}>
                  🔍 No sensors match the current filters
                </td>
              </tr>
            ) : (
              sortedSensors.map((sensor) => {
                const severity = getSeverity(sensor.currentSeverity);
                const stars = getStarRating(sensor.currentSeverity);
                
                return (
                  <tr 
                    key={sensor.deviceMacAddress}
                    style={{
                      borderBottom: `1px solid ${darkColors.border}`,
                      backgroundColor: sensor.currentSeverity === 2 ? 'rgba(248, 81, 73, 0.08)' : 'transparent',
                      transition: 'background-color 0.2s'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.backgroundColor = darkColors.surface2;
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.backgroundColor = 
                        sensor.currentSeverity === 2 ? 'rgba(248, 81, 73, 0.08)' : 'transparent';
                    }}
                  >
                    <td style={{ padding: '10px 14px', fontWeight: 500 }}>
                      {sensor.deviceMacAddress}
                    </td>
                    <td style={{ padding: '10px 14px' }}>
                      {sensor.zone}
                    </td>
                    <td style={{ padding: '10px 14px' }}>
                      {getCategoryLabel(sensor.category)}
                    </td>
                    <td style={{ padding: '10px 14px', textAlign: 'right', fontWeight: 600 }}>
                      {sensor.lastReading !== null ? sensor.lastReading : '—'}
                    </td>
                    <td style={{ padding: '10px 14px', textAlign: 'center' }}>
                      <span style={{
                        display: 'inline-block',
                        padding: '3px 14px',
                        borderRadius: 20,
                        backgroundColor: severity.bg,
                        color: severity.color,
                        fontSize: 12,
                        fontWeight: 600
                      }}>
                        {severity.icon} {severity.label}
                      </span>
                    </td>
                    <td style={{ padding: '10px 14px', textAlign: 'center', fontSize: 13, letterSpacing: 0.5 }}>
                      {stars}
                    </td>
                    <td style={{ padding: '10px 14px', textAlign: 'center', fontSize: 11, color: darkColors.textSecondary }}>
                      {formatTime(sensor.lastSeen)}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Footer */}
      <div style={{
        padding: '10px 20px',
        backgroundColor: darkColors.surface2,
        borderTop: `1px solid ${darkColors.border}`,
        display: 'flex',
        justifyContent: 'space-between',
        fontSize: 12,
        color: darkColors.textSecondary
      }}>
        <span>📋 Showing {sortedSensors.length} sensors</span>
        <span>
          🚨 {sensors.filter(s => s.currentSeverity === 2).length} critical • 
          ⚠️ {sensors.filter(s => s.currentSeverity === 1).length} warning • 
          ✅ {sensors.filter(s => s.currentSeverity === 0).length} normal
        </span>
      </div>
    </div>
  );
}