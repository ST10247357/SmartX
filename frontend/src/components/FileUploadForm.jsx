import { useState } from "react";
import { uploadFile } from "../api/client";

export default function FileUploadForm({ sensors, colors }) {
  // Adapted from: React Docs (2024b) - "useState"
  // Initializes component local state variables to track inputs, upload state, and feedback status messages
  const [selectedMac, setSelectedMac] = useState("");
  const [file, setFile] = useState(null);
  const [status, setStatus] = useState(null);
  const [isUploading, setIsUploading] = useState(false);

  // Fallback color scheme object for UI styling theme consistency
  const darkColors = colors || {
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
  };

  // Adapted from: React Docs (2024a) - "Responding to Events"
  // Prevents default form submission, validates file criteria, and dispatches API payload asynchronously
  async function handleSubmit(e) {
    e.preventDefault();
    setStatus(null);
    setIsUploading(true);

    if (!selectedMac || !file) {
      setStatus({ type: "error", message: "Select a sensor and a file first." });
      setIsUploading(false);
      return;
    }

    // Adapted from: Developer Mozilla (2024a) - "String.prototype.slice()"
    // Extracts file extension using lastIndexOf to enforce strict upload type restrictions
    const allowedTypes = [".txt", ".log", ".json", ".jpg", ".jpeg", ".png"];
    const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
    if (!allowedTypes.includes(extension)) {
      setStatus({ type: "error", message: `File type ${extension} not allowed. Use: ${allowedTypes.join(", ")}` });
      setIsUploading(false);
      return;
    }

    const maxSizeBytes = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSizeBytes) {
      setStatus({ type: "error", message: "File is too large. Maximum size is 5MB." });
      setIsUploading(false);
      return;
    }

    try {
      const result = await uploadFile(selectedMac, file);
      setStatus({ type: "success", message: `Uploaded ${result.fileName} to ${selectedMac}.` });
      setFile(null);
      
      // Adapted from: MDN Web Docs (2024b) - "HTMLInputElement.files"
      // Resets file input value directly in the DOM post successful file upload
      document.getElementById('fileInput').value = '';
    } catch (err) {
      setStatus({ type: "error", message: err.message });
    } finally {
      setIsUploading(false);
    }
  }

  // Adapted from: W3Schools (2024) - "React Forms"
  // Render file selection and dynamic sensor dropdown inputs within styled JSX layout
  return (
    <form onSubmit={handleSubmit}>
      <h3 style={{ marginTop: 0, marginBottom: 14, fontSize: 15, color: darkColors.text }}>
        Attach File
      </h3>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: 14, marginBottom: 14 }}>
        <div>
          <label style={{ fontSize: 12, fontWeight: 600, color: darkColors.textSecondary, display: 'block', marginBottom: 4 }}>
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
              boxSizing: 'border-box',
              cursor: 'pointer',
            }}
          >
            <option value="">-- Select a sensor --</option>
            {(sensors || []).map((s) => (
              <option key={s.deviceMacAddress} value={s.deviceMacAddress}>
                {s.deviceMacAddress} ({s.zone})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label style={{ fontSize: 12, fontWeight: 600, color: darkColors.textSecondary, display: 'block', marginBottom: 4 }}>
            Choose File
          </label>
          <input
            id="fileInput"
            type="file"
            accept=".txt,.log,.json,.jpg,.jpeg,.png"
            onChange={(e) => setFile(e.target.files[0])}
            style={{
              width: '100%',
              padding: '6px 8px',
              borderRadius: 6,
              border: `1px solid ${darkColors.border}`,
              backgroundColor: darkColors.surface2,
              color: darkColors.text,
              fontSize: 12,
              boxSizing: 'border-box',
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
          width: '100%',
        }}
      >
        {isUploading ? 'Uploading...' : 'Upload File'}
      </button>

      {status && (
        <p style={{ marginTop: 10, color: status.type === "error" ? '#f85149' : '#3fb950', fontSize: 13, fontWeight: 500 }}>
          {status.message}
        </p>
      )}
    </form>
  );
}

/*
References:
Developer Mozilla, 2024a. String.prototype.slice(). MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/slice [Accessed 3 September 2026].
MDN Web Docs, 2024b. HTMLInputElement.files. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/API/HTMLInputElement/files [Accessed 1 September 2026].
React Docs, 2024a. Responding to Events. React Documentation. Available at: https://react.dev/learn/responding-to-events [Accessed 6 September 2026].
React Docs, 2024b. useState. React Documentation. Available at: https://react.dev/reference/react/useState [Accessed 4 September 2026].
W3Schools, 2024. React Forms. W3Schools. Available at: https://www.w3schools.com/react/react_forms.asp [Accessed 2 September 2026].
*/