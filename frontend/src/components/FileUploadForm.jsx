import { useState } from "react";
import { uploadFile } from "../api/client";

export default function FileUploadForm({ sensors }) {
  const [selectedMac, setSelectedMac] = useState("");
  const [file, setFile] = useState(null);
  const [status, setStatus] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);

    if (!selectedMac || !file) {
      setStatus({ type: "error", message: "Select a sensor and a file first." });
      return;
    }

    try {
      const result = await uploadFile(selectedMac, file);
      setStatus({ type: "success", message: `Uploaded ${result.fileName} to ${selectedMac}.` });
      setFile(null);
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: 24, padding: 16, border: "1px solid #ddd", borderRadius: 8 }}>
      <h3>Attach Config / Log / Photo to a Sensor</h3>

      <div style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "flex-end" }}>
        <label>
          Sensor
          <br />
          <select value={selectedMac} onChange={(e) => setSelectedMac(e.target.value)}>
            <option value="">-- Select a sensor --</option>
            {sensors.map((s) => (
              <option key={s.deviceMacAddress} value={s.deviceMacAddress}>
                {s.deviceMacAddress} ({s.zone})
              </option>
            ))}
          </select>
        </label>

        <label>
          File
          <br />
          <input type="file" onChange={(e) => setFile(e.target.files[0])} />
        </label>

        <button type="submit">Upload</button>
      </div>

      {status && (
        <p style={{ color: status.type === "error" ? "red" : "green", marginTop: 8 }}>
          {status.message}
        </p>
      )}
    </form>
  );
}
