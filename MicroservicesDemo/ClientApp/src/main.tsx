// ============================================================================
// APPLICATION ENTRY POINT
// ============================================================================
// This is the first file executed when the app loads.
// It renders the React application into the DOM.
// ============================================================================

import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

// Get the root DOM element where React will render
// (defined in index.html as <div id="root"></div>)
ReactDOM.createRoot(document.getElementById('root')!).render(
  // StrictMode enables additional development checks and warnings
  // It renders components twice in dev mode to detect side effects
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
