import { useState } from "react";
import LandingPage from "./components/LandingPage";
import Dashboard from "./components/Dashboard";

export default function App() {
  const [activeView, setActiveView] = useState(null);

  if (activeView === "ingestion") {
    return (
      <div>
        <button style={{ margin: 16 }} onClick={() => setActiveView(null)}>
          ← Back to menu
        </button>
        <Dashboard />
      </div>
    );
  }

  return <LandingPage onSelect={setActiveView} />;
}