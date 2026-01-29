// ============================================================================
// LOGIN COMPONENT
// ============================================================================
// Landing page for unauthenticated users.
// Provides a button to initiate OAuth authentication with IdentityServer.
// ============================================================================

import React from 'react';
import { useAuth } from '../contexts/AuthContext';

/**
 * Login Page Component
 * 
 * Displays when user is not authenticated (via routing in App.tsx).
 * Clicking the login button triggers the OAuth Authorization Code flow.
 */
const Login: React.FC = () => {
  const { login } = useAuth(); // Get login function from auth context

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Microservices Demo</h1>
        <p>Please login to access the application</p>
        {/* Login button starts OAuth flow - redirects to IdentityServer */}
        <button className="primary" onClick={login} style={{ marginTop: '1rem', width: '100%' }}>
          Login with IdentityServer
        </button>
      </div>
    </div>
  );
};

export default Login;
