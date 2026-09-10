import { useState } from "react";
import LandingPage from "./components/LandingPage";
import Dashboard from "./components/Dashboard";

const DARK_THEME = {
  bg: '#0d1117',
  surface: '#161b22',
  surface2: '#1c2333',
  border: '#30363d',
  text: '#e6edf3',
  textSecondary: '#8b949e',
  primary: '#58a6ff',
  primaryHover: '#4a8dd6',
  dangerBg: 'rgba(248, 81, 73, 0.15)',
  dangerText: '#f85149',
  successText: '#3fb950',
  star: '#e3b341',
};

export default function App() {
  // Adapted from React State Management for Dynamic Views (React Docs, 2024a)
  const [activeView, setActiveView] = useState(null);

  if (activeView === "ingestion") {
    return (
      <div style={{
        minHeight: '100vh',
        backgroundColor: DARK_THEME.bg,
        color: DARK_THEME.text,
        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif',
        padding: '16px 24px'
      }}>
        <div style={{
          maxWidth: 1200,
          margin: '0 auto'
        }}>
          {/* Back Navigation Bar */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            justify: 'space-between',
            marginBottom: 20,
            paddingBottom: 12,
            borderBottom: `1px solid ${DARK_THEME.border}`
          }}>
            <button
              onClick={() => setActiveView(null)}
              style={{
                padding: '8px 16px',
                borderRadius: 6,
                border: `1px solid ${DARK_THEME.border}`,
                backgroundColor: DARK_THEME.surface,
                color: DARK_THEME.text,
                fontSize: 13,
                fontWeight: 600,
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: 6,
                transition: 'all 0.2s'
              }}
              onMouseEnter={(e) => e.currentTarget.style.backgroundColor = DARK_THEME.surface2}
              onMouseLeave={(e) => e.currentTarget.style.backgroundColor = DARK_THEME.surface}
            >
              ← Back to menu
            </button>
            <span style={{ fontSize: 13, color: DARK_THEME.textSecondary, fontWeight: 500 }}>
              IoT Sensor Management Engine
            </span>
          </div>

          <Dashboard colors={DARK_THEME} />
        </div>
      </div>
    );
  }

  return (
    <div style={{
      minHeight: '100vh',
      backgroundColor: DARK_THEME.bg,
      color: DARK_THEME.text,
      fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif'
    }}>
      <LandingPage onSelect={setActiveView} colors={DARK_THEME} />
    </div>
  );
}

/*
References:
MDN Web Docs, 2024. Using CSS custom properties (variables). Mozilla Developer Network. Available at: https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties [Accessed 10 September 2026].
React Docs, 2024a. Conditional Rendering. React Documentation. Available at: https://react.dev/learn/conditional-rendering [Accessed 10 September 2026].
React Docs, 2024b. Lifting State Up. React Documentation. Available at: https://react.dev/learn/sharing-state-between-components [Accessed 10 September 2026].
*/