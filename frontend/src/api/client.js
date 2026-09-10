const BASE_URL = "http://localhost:5275/api";

// Adapted from HTTP Error Handling & JSON Response Parsing (MDN Web Docs, 2024b)
async function handleResponse(res) {
  if (!res.ok) {
    const data = await res.json().catch(() => ({}));
    throw new Error(data.message || `Request failed (${res.status})`);
  }
  return res.json();
}

// Adapted from JavaScript Fetch API Asynchronous Requests (MDN Web Docs, 2024a)
export async function getSensors() {
  return handleResponse(await fetch(`${BASE_URL}/sensors`));
}

export async function registerSensor(sensor) {
  return handleResponse(await fetch(`${BASE_URL}/sensors/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(sensor),
  }));
}

// Adapted from FormData Interface for File Uploads (MDN Web Docs, 2024c)
export async function uploadFile(mac, file) {
  const formData = new FormData();
  formData.append("file", file);
  return handleResponse(await fetch(`${BASE_URL}/sensors/${mac}/upload`, {
    method: "POST",
    body: formData,
  }));
}

export async function getSummary() {
  return handleResponse(await fetch(`${BASE_URL}/gamification/summary`));
}

export async function resolveAlert(mac) {
  return handleResponse(await fetch(`${BASE_URL}/gamification/resolve/${mac}`, {
    method: "POST",
  }));
}

// Adapted from ES6 Object Dictionary Mapping (Flanagan, 2020)
export function getCategoryLabel(category) {
  const labels = { 0: "Environmental", 1: "Power Consumption", 2: "Actuator" };
  return labels[category] ?? "Unknown";
}

/*
References:
Flanagan, D., 2020. JavaScript: The Definitive Guide. 7th ed. Sebastopol: O'Reilly Media.
MDN Web Docs, 2024a. Fetch API. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API [Accessed 10 September 2026].
MDN Web Docs, 2024b. Using the Fetch API: Checking that the request was successful. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch#checking_that_the_request_was_successful [Accessed 10 September 2026].
MDN Web Docs, 2024c. FormData. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/API/FormData [Accessed 10 September 2026].
*/