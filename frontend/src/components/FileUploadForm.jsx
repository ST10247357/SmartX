import { useState } from "react";
import { uploadFile } from "../api/client";

export default function FileUploadForm({ sensors, colors }) {
  const [selectedMac, setSelectedMac] = useState("");
  const [file, setFile] = useState(null);
  const [status, setStatus] = useState(null);
  const [isUploading, setIsUploading] = useState(false);

  const darkColors = colors || {
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
  };

  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);
    setIsUploading(true);

    if (!selectedMac || !file) {
      setStatus({ type: "error", message: "Select a sensor and a file first." });
      setIsUploading(false);
      return;
    }

    try {
      const result = await uploadFile(selectedMac, file);
      setStatus({ type: "success", message: `✅ Uploaded ${result.fileName} to ${selectedMac}` });
      setFile(null);
      document.getElementById('fileInput').value = '';
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    } finally {
      setIsUploading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <div style={{
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: 12,
        marginBottom: 12
      }}>
        <div>
          <label style={{
            fontSize: 12,
            fontWeight: 600,
            color: darkColors.textSecondary,
            display: 'block',
            marginBottom: 4
          }}>
            Select Sensor
          </label>
          <select
            value={selectedMac}
            onChange={(e) => setSelectedMac(e.target.value)}
            style={{
              width: '100%',
              padding: '8px 12px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 13,
              outline: 'none',
              cursor: 'pointer'
            }}
          >
            <option value="" style={{
              backgroundColor: darkColors.surface2,
              color: darkColors.textSecondary
            }}>
              -- Select a sensor --
            </option>
            {sensors.map((s) => (
              <option key={s.deviceMacAddress} value={s.deviceMacAddress} style={{
                backgroundColor: darkColors.surface2,
                color: darkColors.text,
                padding: '6px'
              }}>
                {s.deviceMacAddress} ({s.zone})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label style={{
            fontSize: 12,
            fontWeight: 600,
            color: darkColors.textSecondary,
            display: 'block',
            marginBottom: 4
          }}>
            Choose File
          </label>
          <input
            id="fileInput"
            type="file"
            onChange={(e) => setFile(e.target.files[0])}
            style={{
              width: '100%',
              padding: '6px 8px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 12
            }}
          />
        </div>
      </div>

      <button
        type="submit"
        disabled={isUploading}
        style={{
          padding: '8px 20px',
          borderRadius: 6,
          border: 'none',
          backgroundColor: isUploading ? darkColors.textSecondary : darkColors.primary,
          color: 'white',
          fontWeight: 600,
          fontSize: 13,
          cursor: isUploading ? 'not-allowed' : 'pointer',
          transition: 'all 0.2s',
          width: '100%'
        }}
        onMouseEnter={(e) => {
          if (!isUploading) e.currentTarget.style.backgroundColor = '#4a8dd6';
        }}
        onMouseLeave={(e) => {
          if (!isUploading) e.currentTarget.style.backgroundColor = darkColors.primary;
        }}
      >
        {isUploading ? '⏳ Uploading...' : '📤 Upload File'}
      </button>

      {status && (
        <p style={{
          marginTop: 10,
          color: status.type === "error" ? '#f85149' : '#3fb950',
          fontSize: 13,
          fontWeight: 500
        }}>
          {status.message}
        </p>
      )}
    </form>
  );
}