// ============================================================================
// OAUTH CALLBACK COMPONENT
// ============================================================================
// Handles the OAuth redirect from IdentityServer after user authenticates.
// Processes the authorization code and exchanges it for an access token.
// ============================================================================

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

/**
 * OAuth Callback Handler Component
 * 
 * Flow:
 * 1. IdentityServer redirects here with ?code=xxx&state=xxx
 * 2. authService.handleCallback() exchanges code for token
 * 3. On success: navigate to home page
 * 4. On error: navigate back to login
 * 
 * User sees this page briefly during token exchange.
 */
const Callback: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    /**
     * Process OAuth callback
     * Runs once when component mounts
     */
    const handleCallback = async () => {
      try {
        // Exchange authorization code for access token
        await authService.handleCallback();
        
        // Success! Redirect to home page
        navigate('/');
      } catch (error) {
        // Token exchange failed, go back to login
        console.error('Authentication error:', error);
        navigate('/login');
      }
    };

    handleCallback();
  }, [navigate]); // Depend on navigate to satisfy linter

  // Show loading message while processing
  return (
    <div className="loading">
      <h2>Processing authentication...</h2>
    </div>
  );
};

export default Callback;
