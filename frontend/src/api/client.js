const BASE_URL = "http://localhost:5275/api";

export async function getSensors() {
  const res = await fetch(`${BASE_URL}/sensors`);
  if (!res.ok) throw new Error("Failed to fetch sensors");
  return res.json();
}

export async function registerSensor(sensor) {
  const res = await fetch(`${BASE_URL}/sensors/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(sensor),
  });
  if (!res.ok) throw new Error("Failed to register sensor");
  return res.json();
}

export async function sendTelemetry(deviceMacAddress, value) {
  const res = await fetch(`${BASE_URL}/telemetry`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ deviceMacAddress, value }),
  });
  if (!res.ok) throw new Error("Failed to send telemetry");
  return res.json();
}

export async function uploadFile(mac, file) {
  const formData = new FormData();
  formData.append("file", file);
  const res = await fetch(`${BASE_URL}/sensors/${mac}/upload`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) throw new Error("Failed to upload file");
  return res.json();
}

// ============================================
// GAMIFICATION API FUNCTIONS
// ============================================

export async function getGamificationStats() {
  const res = await fetch(`${BASE_URL}/gamification/stats`);
  if (!res.ok) throw new Error("Failed to fetch gamification stats");
  return res.json();
}

export async function getAlertHistory() {
  const res = await fetch(`${BASE_URL}/gamification/alerts`);
  if (!res.ok) throw new Error("Failed to fetch alert history");
  return res.json();
}

export async function resolveAlert(macAddress) {
  const res = await fetch(`${BASE_URL}/gamification/resolve/${macAddress}`, {
    method: "POST",
  });
  if (!res.ok) throw new Error("Failed to resolve alert");
  return res.json();
}

// ============================================
// DEPLOYMENT VALIDATION API FUNCTIONS
// ============================================

export async function validateDeployment(deploymentTree) {
  const res = await fetch(`${BASE_URL}/deployment/validate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(deploymentTree),
  });
  if (!res.ok) throw new Error("Failed to validate deployment");
  return res.json();
}