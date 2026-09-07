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