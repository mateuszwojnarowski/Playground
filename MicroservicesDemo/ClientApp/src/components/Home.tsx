// ============================================================================
// HOME PAGE COMPONENT
// ============================================================================
// Landing page shown to authenticated users.
// Provides overview and navigation to main features.
// ============================================================================

import React from 'react';

/**
 * Home Page Component
 * 
 * Welcome screen that explains the application's features.
 * Displayed after successful login.
 */
const Home: React.FC = () => {
  return (
    <div className="container">
      <div className="card">
        <h1>Welcome to Microservices Demo</h1>
        <p>This is a React client application that demonstrates:</p>
        <ul>
          {/* List of key features */}
          <li>OAuth2/OIDC authentication with IdentityServer</li>
          <li>API Gateway integration</li>
          <li>Microservices communication</li>
          <li>Product and Order management</li>
        </ul>
        <p>Use the navigation above to explore the Products and Orders sections.</p>
      </div>
    </div>
  );
};

export default Home;
