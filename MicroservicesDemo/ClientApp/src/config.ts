// ============================================================================
// APPLICATION CONFIGURATION
// ============================================================================
// Centralized configuration for authentication and API endpoints.
// Modify these values to match your environment (dev, staging, production).
// ============================================================================

import { AuthConfig } from './types';

/**
 * OAuth2/OIDC Authentication Configuration
 * 
 * This configures the Authorization Code Flow with PKCE for secure authentication.
 * The client is registered in IdentityServer with these exact settings.
 * 
 * IMPORTANT: Update these URLs for different environments!
 */
export const authConfig: AuthConfig = {
  authority: 'https://localhost:5001',           // IdentityServer URL (must match server config)
  clientId: 'react-client',                      // Client ID from IdentityServer Config.cs
  redirectUri: 'http://localhost:3000/callback', // Where auth server sends user after login
  postLogoutRedirectUri: 'http://localhost:3000', // Where to go after logout
  responseType: 'code',                          // Use Authorization Code flow (most secure for SPAs)
  scope: 'openid profile order.edit order.view product.edit product.view product.stock' // Requested permissions
};

/**
 * API Base URL
 * 
 * In development: Vite proxies /api requests to https://localhost:7290 (API Gateway)
 * In production: Nginx proxies /api requests to the api-gateway service
 * 
 * This abstraction allows the frontend to work in both environments without code changes.
 */
export const API_BASE_URL = '/api';
