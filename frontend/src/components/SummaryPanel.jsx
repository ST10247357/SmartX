import { useEffect, useState } from "react";
import { getSummary } from "../api/client";

export default function SummaryPanel({ refreshKey, colors }) {
  // Adapted from: React Docs (2024a) - "Synchronizing with Effects"
  // Initializes state for storing summary network statistics metrics
  const [summary, setSummary] = useState(null);

  // Default color palette tokens fallback for metric panel styling
  const darkColors = colors || {
    surface: '#161b22',
    surface2: '#1c2333',
    border: '#30363d',
    text: '#e6edf3',
    textSecondary: '#8b949e',
    primary: '#58a6ff',
    dangerText: '#f85149',
    star: '#e3b341',
  };

  // Adapted from: React Docs (2024a) - "Synchronizing with Effects"
  // Fetches aggregated summary stats on initial render and upon trigger of refreshKey updates
  useEffect(() => {
    getSummary().then(setSummary).catch(() => {});
  }, [refreshKey]);

  // Adapted from: React Docs (2024b) - "Conditional Rendering"
  // Prevents rendering markup prior to asynchronously populating summary data state
  if (!summary) return null;

  // Adapted from: Developer Mozilla (2024a) - "Number.prototype.toFixed()"
  // Maps API summary metrics into display items and formats floating point rating values
  const statItems = [
    { label: "Total Sensors", value: summary.totalSensors, color: darkColors.primary },
    { label: "Open Alerts", value: summary.openAlerts, color: summary.openAlerts > 0 ? darkColors.dangerText : darkColors.text },
    { label: "Resolutions", value: summary.totalResolutions, color: darkColors.text },
    { label: "Avg Rating", value: `${summary.averageStarRating.toFixed(1)}★`, color: darkColors.star },
  ];

  // Adapted from: W3Schools (2024) - "CSS Grid Layout"
  // Renders metric dashboard grid cards using CSS Grid auto-fit and minmax layout rules
  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(130px, 1fr))",
        gap: 12,
        marginBottom: 16,
        padding: 14,
        border: `1px solid ${darkColors.border}`,
        borderRadius: 8,
        backgroundColor: darkColors.surface,
      }}
    >
      {/* Adapted from: Developer Mozilla (2024b) - "Array.prototype.map()" */}
      {/* Renders each metric summary entry as a styled metric card container */}
      {statItems.map((item, idx) => (
        <div
          key={idx}
          style={{
            backgroundColor: darkColors.surface2,
            padding: "10px 14px",
            borderRadius: 6,
            border: `1px solid ${darkColors.border}`,
            display: "flex",
            flexDirection: "column",
            gap: 4,
          }}
        >
          <span style={{ fontSize: 11, fontWeight: 600, color: darkColors.textSecondary, textTransform: "uppercase", letterSpacing: 0.5 }}>
            {item.label}
          </span>
          <strong style={{ fontSize: 18, color: item.color, fontWeight: 700 }}>
            {item.value}
          </strong>
        </div>
      ))}
    </div>
  );
}

/*
References:
Developer Mozilla, 2024a. Number.prototype.toFixed(). MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/toFixed [Accessed 10 September 2026].
Developer Mozilla, 2024b. Array.prototype.map(). MDN Web Docs. Available at: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/map [Accessed 10 September 2026].
React Docs, 2024a. Synchronizing with Effects. React Documentation. Available at: https://react.dev/learn/synchronizing-with-effects [Accessed 10 September 2026].
React Docs, 2024b. Conditional Rendering. React Documentation. Available at: https://react.dev/learn/conditional-rendering [Accessed 10 September 2026].
W3Schools, 2024. CSS Grid Layout. W3Schools. Available at: https://www.w3schools.com/css/css_grid.asp [Accessed 10 September 2026].
*/