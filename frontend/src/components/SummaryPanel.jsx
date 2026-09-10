import { useEffect, useState } from "react";
import { getSummary } from "../api/client";

export default function SummaryPanel({ refreshKey, colors }) {
  // Adapted from React useState Hook for Summary State Management (React Docs, 2024a)
  const [summary, setSummary] = useState(null);

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

  // Adapted from React useEffect Data Fetching Pattern (React Docs, 2024a)
  useEffect(() => {
    getSummary().then(setSummary).catch(() => {});
  }, [refreshKey]);

  // Adapted from Conditional Rendering Pattern (React Docs, 2024b)
  if (!summary) return null;

  const statItems = [
    { label: "Total Sensors", value: summary.totalSensors, color: darkColors.primary },
    { label: "Open Alerts", value: summary.openAlerts, color: summary.openAlerts > 0 ? darkColors.dangerText : darkColors.text },
    { label: "Resolutions", value: summary.totalResolutions, color: darkColors.text },
    { label: "Avg Rating", value: `${summary.averageStarRating.toFixed(1)}★`, color: darkColors.star },
  ];

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
MDN Web Docs, 2024. Basic concepts of flexbox. Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_flexible_box_layout/Basic_concepts_of_flexbox [Accessed 10 September 2026].
React Docs, 2024a. Synchronizing with Effects. React Documentation. Available at: https://react.dev/learn/synchronizing-with-effects [Accessed 10 September 2026].
React Docs, 2024b. Conditional Rendering. React Documentation. Available at: https://react.dev/learn/conditional-rendering [Accessed 10 September 2026].
*/