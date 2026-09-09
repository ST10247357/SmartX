const BASE_URL = "http://localhost:5275/api";

async function handleResponse(response) {
  if (!response.ok) {
    let errorMessage = `HTTP error! status: ${response.status}`;
    try {
      const errorData = await response.json();
      if (errorData.message) {
        errorMessage = errorData.message;
      }
    } catch {
      errorMessage = response.statusText || errorMessage;
    }
    throw new Error(errorMessage);
  }
  return response.json();
}

export async function getSensors() {
  const res = await fetch(`${BASE_URL}/sensors`);
  return handleResponse(res);
}

export async function registerSensor(sensor) {
  const res = await fetch(`${BASE_URL}/sensors/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(sensor),
  });
  return handleResponse(res);
}

export async function sendTelemetry(deviceMacAddress, value) {
  const res = await fetch(`${BASE_URL}/telemetry`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ deviceMacAddress, value }),
  });
  return handleResponse(res);
}

export async function uploadFile(mac, file) {
  const formData = new FormData();
  formData.append("file", file);
  const res = await fetch(`${BASE_URL}/sensors/${mac}/upload`, {
    method: "POST",
    body: formData,
  });
  return handleResponse(res);
}

export async function getGamificationStats() {
  const res = await fetch(`${BASE_URL}/gamification/stats`);
  return handleResponse(res);
}

export async function getAlertHistory() {
  const res = await fetch(`${BASE_URL}/gamification/alerts`);
  return handleResponse(res);
}

export async function resolveAlert(macAddress) {
  const res = await fetch(`${BASE_URL}/gamification/resolve/${macAddress}`, {
    method: "POST",
  });
  return handleResponse(res);
}

export async function validateDeployment(deploymentTree) {
  const res = await fetch(`${BASE_URL}/deployment/validate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(deploymentTree),
  });
  return handleResponse(res);
}

export async function getZoneStatus() {
  const res = await fetch(`${BASE_URL}/deployment/zone-status`);
  return handleResponse(res);
}

export function getCategoryLabel(category) {
  const labels = {
    0: '🌱 Environmental',
    1: '⚡ Power Consumption',
    2: '🔧 Actuator',
  };
  return labels[category] || 'Unknown';
}

export function getCategoryIcon(category) {
  const icons = {
    0: '🌱',
    1: '⚡',
    2: '🔧',
  };
  return icons[category] || '📡';
}

export function getSeverity(severity) {
  const severities = {
    0: { label: 'Normal', color: '#2e7d32', bg: '#e8f5e9', icon: '✅' },
    1: { label: 'Warning', color: '#f9a825', bg: '#fff8e1', icon: '⚠️' },
    2: { label: 'Critical', color: '#c62828', bg: '#ffebee', icon: '🚨' },
  };
  return severities[severity] || severities[0];
}

export function formatDate(dateString) {
  if (!dateString) return 'Never';
  try {
    return new Date(dateString).toLocaleString();
  } catch {
    return 'Invalid date';
  }
}

export function isQuickResponse(lastSeen) {
  if (!lastSeen) return false;
  try {
    const lastSeenDate = new Date(lastSeen);
    const now = new Date();
    const diffMs = now.getTime() - lastSeenDate.getTime();
    return diffMs < 300000;
  } catch {
    return false;
  }
}

export function calculateStarRating(severity, resolutionCount) {
  const baseStars = severity === 0 ? 5 : severity === 1 ? 3 : 1;
  const bonusStars = Math.min(2, Math.floor((resolutionCount || 0) / 3));
  return Math.min(5, baseStars + bonusStars);
}

export function getStars(rating) {
  const clampedRating = Math.max(0, Math.min(5, rating));
  return '⭐'.repeat(clampedRating) + '☆'.repeat(5 - clampedRating);
}

export function countBySeverity(sensors, severity) {
  return sensors.filter(s => s.currentSeverity === severity).length;
}

export default {
  getSensors,
  registerSensor,
  sendTelemetry,
  uploadFile,
  getGamificationStats,
  getAlertHistory,
  resolveAlert,
  validateDeployment,
  getZoneStatus,
  getCategoryLabel,
  getCategoryIcon,
  getSeverity,
  formatDate,
  isQuickResponse,
  calculateStarRating,
  getStars,
  countBySeverity,
};