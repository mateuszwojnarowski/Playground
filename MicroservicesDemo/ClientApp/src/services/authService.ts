// ============================================================================
// AUTHENTICATION SERVICE
// ============================================================================
// Handles OAuth2/OIDC authentication flow with IdentityServer using PKCE.
// Implements Authorization Code Flow with PKCE (Proof Key for Code Exchange)
// which is the recommended flow for Single Page Applications (SPAs).
//
// Flow:
// 1. User clicks login -> generate PKCE code verifier/challenge
// 2. Redirect to IdentityServer with challenge
// 3. User authenticates -> IdentityServer redirects back with code
// 4. Exchange code + verifier for access token
// 5. Store token and use for API calls
// ============================================================================

import { authConfig } from '../config';
import { AuthState, UserInfo } from '../types';

/**
 * Generate a cryptographically random string for PKCE code verifier
 * 
 * PKCE (RFC 7636) requires a high-entropy cryptographic random string.
 * This prevents authorization code interception attacks.
 * 
 * @param length - Length of the random string (recommended: 43-128 characters)
 * @returns URL-safe random string
 */
function generateRandomString(length: number): string {
  const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
  const values = new Uint8Array(length);
  crypto.getRandomValues(values); // Use crypto API for true randomness
  return Array.from(values)
    .map(v => charset[v % charset.length])
    .join('');
}

/**
 * Generate PKCE code challenge from code verifier
 * 
 * Creates a SHA-256 hash of the code verifier and base64url-encodes it.
 * This challenge is sent to the authorization server, and later the plain
 * verifier is sent during token exchange to prove possession.
 * 
 * @param codeVerifier - The random code verifier string
 * @returns Base64url-encoded SHA-256 hash of the verifier
 */
async function generateCodeChallenge(codeVerifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(codeVerifier);
  const hash = await crypto.subtle.digest('SHA-256', data); // SHA-256 hash
  const base64 = btoa(String.fromCharCode(...new Uint8Array(hash)));
  // Convert to base64url format (replace +/= with -_)
  return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

/**
 * Parse JWT token and extract user information
 * 
 * NOTE: This does NOT verify the token signature! Verification happens
 * server-side. This is only for displaying user info in the UI.
 * 
 * JWT structure: header.payload.signature (all base64url-encoded)
 * 
 * @param token - JWT access token from IdentityServer
 * @returns Decoded user information from token payload
 */
function parseJwt(token: string): UserInfo {
  try {
    const base64Url = token.split('.')[1]; // Get payload (middle part)
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/'); // base64url -> base64
    // Decode base64 and properly handle Unicode characters
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error parsing JWT:', error);
    throw error;
  }
}

/**
 * Authentication Service (Singleton)
 * 
 * Manages the complete OAuth2/OIDC authentication lifecycle:
 * - Initiating login flow with PKCE
 * - Handling authorization callbacks
 * - Token storage and retrieval
 * - Logout functionality
 * - Notifying components of auth state changes
 */
class AuthService {
  // Current authentication state
  private authState: AuthState = {
    isAuthenticated: false,
    accessToken: null,
    user: null
  };

  // Observer pattern: components can subscribe to auth state changes
  private listeners: Array<(state: AuthState) => void> = [];

  constructor() {
    // Initialize by loading any existing auth state from storage
    this.loadAuthState();
  }

  /**
   * Subscribe to authentication state changes (Observer pattern)
   * 
   * Components can subscribe to be notified when auth state changes
   * (e.g., user logs in/out). Returns an unsubscribe function.
   * 
   * @param listener - Callback function to invoke on state changes
   * @returns Unsubscribe function to remove this listener
   */
  subscribe(listener: (state: AuthState) => void): () => void {
    this.listeners.push(listener);
    // Return unsubscribe function
    return () => {
      this.listeners = this.listeners.filter(l => l !== listener);
    };
  }

  /**
   * Notify all subscribed listeners of auth state change
   * Called internally whenever auth state is updated
   */
  private notifyListeners(): void {
    this.listeners.forEach(listener => listener(this.authState));
  }

  /**
   * Load authentication state from session storage
   * 
   * Called on app initialization to restore auth state if user
   * refreshes the page. Session storage persists for the browser tab.
   */
  private loadAuthState(): void {
    const accessToken = sessionStorage.getItem('access_token');
    if (accessToken) {
      try {
        // Parse token to get user info
        const user = parseJwt(accessToken);
        this.authState = {
          isAuthenticated: true,
          accessToken,
          user
        };
      } catch (error) {
        console.error('Error loading auth state:', error);
        this.clearAuthState(); // Invalid token, clear it
      }
    }
  }

  /**
   * Save authentication state to session storage
   * 
   * Stores access token and updates internal state after successful login.
   * Session storage is used (not localStorage) so tokens don't persist
   * across browser sessions.
   * 
   * @param accessToken - JWT access token from IdentityServer
   */
  private saveAuthState(accessToken: string): void {
    sessionStorage.setItem('access_token', accessToken);
    const user = parseJwt(accessToken);
    this.authState = {
      isAuthenticated: true,
      accessToken,
      user
    };
    // Notify all components that auth state changed
    this.notifyListeners();
  }

  /**
   * Clear authentication state
   * 
   * Removes all auth-related data from storage and resets state.
   * Called on logout or when token is invalid.
   */
  private clearAuthState(): void {
    sessionStorage.removeItem('access_token');
    sessionStorage.removeItem('code_verifier');
    this.authState = {
      isAuthenticated: false,
      accessToken: null,
      user: null
    };
    this.notifyListeners();
  }

  /**
   * Get current authentication state
   * 
   * @returns Copy of current auth state (prevents direct mutation)
   */
  getAuthState(): AuthState {
    return { ...this.authState };
  }

  /**
   * Initiate OAuth2 login flow with PKCE
   * 
   * Steps:
   * 1. Generate PKCE code verifier and challenge
   * 2. Store verifier in session storage (needed later for token exchange)
   * 3. Build authorization URL with all OAuth parameters
   * 4. Redirect user to IdentityServer login page
   * 
   * After successful auth, IdentityServer redirects back to our callback URL
   */
  async login(): Promise<void> {
    // Step 1: Generate PKCE parameters
    const codeVerifier = generateRandomString(128);
    const codeChallenge = await generateCodeChallenge(codeVerifier);
    const state = generateRandomString(32); // CSRF protection

    // Step 2: Store verifier and state for later use
    sessionStorage.setItem('code_verifier', codeVerifier);
    sessionStorage.setItem('auth_state', state);

    // Step 3: Build authorization URL with OAuth parameters
    const params = new URLSearchParams({
      client_id: authConfig.clientId,           // Who is requesting auth
      redirect_uri: authConfig.redirectUri,     // Where to send user after auth
      response_type: authConfig.responseType,   // 'code' = Authorization Code flow
      scope: authConfig.scope,                  // What permissions we're requesting
      state,                                    // CSRF token
      code_challenge: codeChallenge,            // PKCE challenge
      code_challenge_method: 'S256'             // SHA-256 hashing method
    });

    const authUrl = `${authConfig.authority}/connect/authorize?${params.toString()}`;
    
    // Step 4: Redirect to IdentityServer
    window.location.href = authUrl;
  }

  /**
   * Handle OAuth callback after user authenticates
   * 
   * Called when IdentityServer redirects back to our app with an auth code.
   * 
   * Steps:
   * 1. Validate callback parameters (code, state)
   * 2. Exchange authorization code for access token (with PKCE verifier)
   * 3. Store token and update auth state
   * 4. Clean up temporary storage
   */
  async handleCallback(): Promise<void> {
    // Step 1: Extract and validate callback parameters
    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');              // Authorization code
    const state = params.get('state');            // CSRF token
    const storedState = sessionStorage.getItem('auth_state');

    // Validate: must have code and state must match what we sent
    if (!code || !state || state !== storedState) {
      throw new Error('Invalid callback parameters');
    }

    // Get the stored PKCE verifier
    const codeVerifier = sessionStorage.getItem('code_verifier');
    if (!codeVerifier) {
      throw new Error('Code verifier not found');
    }

    // Step 2: Exchange authorization code for access token
    const tokenResponse = await fetch(`${authConfig.authority}/connect/token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: new URLSearchParams({
        client_id: authConfig.clientId,
        code,                                     // The authorization code
        redirect_uri: authConfig.redirectUri,
        code_verifier: codeVerifier,              // PKCE verifier (proves we initiated the flow)
        grant_type: 'authorization_code'
      })
    });

    if (!tokenResponse.ok) {
      const errorText = await tokenResponse.text();
      console.error('Token exchange failed:', errorText);
      throw new Error('Failed to exchange code for tokens');
    }

    const tokens = await tokenResponse.json();
    
    // Step 3: Save the access token
    this.saveAuthState(tokens.access_token);

    // Step 4: Clean up temporary data
    sessionStorage.removeItem('code_verifier');
    sessionStorage.removeItem('auth_state');
  }

  /**
   * Logout user and clear session
   * 
   * Steps:
   * 1. Clear local auth state and storage
   * 2. Redirect to IdentityServer logout endpoint
   * 3. IdentityServer will clear its session and redirect back to our app
   */
  async logout(): Promise<void> {
    // Clear local state first
    this.clearAuthState();
    
    // Build logout URL
    const params = new URLSearchParams({
      post_logout_redirect_uri: authConfig.postLogoutRedirectUri
    });

    // Redirect to IdentityServer to clear server-side session
    window.location.href = `${authConfig.authority}/connect/endsession?${params.toString()}`;
  }

  /**
   * Get current access token
   * 
   * @returns JWT access token or null if not authenticated
   */
  getAccessToken(): string | null {
    return this.authState.accessToken;
  }

  /**
   * Check if user is currently authenticated
   * 
   * @returns True if user has a valid access token
   */
  isAuthenticated(): boolean {
    return this.authState.isAuthenticated;
  }
}

// Export singleton instance
export const authService = new AuthService();
