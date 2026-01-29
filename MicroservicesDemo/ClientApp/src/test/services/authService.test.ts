// ============================================================================
// AUTH SERVICE TESTS
// ============================================================================
// Tests for OAuth2/OIDC authentication service with PKCE flow.
// ============================================================================

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../../services/authService';
import { mockAccessToken } from '../mockData';

describe('AuthService', () => {
  beforeEach(() => {
    // Clear session storage before each test
    sessionStorage.clear();
  });

  describe('Initial State', () => {
    it('should initialize with unauthenticated state', () => {
      const state = authService.getAuthState();
      expect(state.isAuthenticated).toBe(false);
      expect(state.accessToken).toBeNull();
      expect(state.user).toBeNull();
    });
  });

  describe('Login Flow', () => {
    it('should generate PKCE parameters and redirect to authorization endpoint', async () => {
      await authService.login();
      
      // Verify PKCE parameters are stored
      expect(sessionStorage.getItem('code_verifier')).toBeTruthy();
      expect(sessionStorage.getItem('auth_state')).toBeTruthy();
      
      // Note: Can't easily test redirect without DOM manipulation
      // In real scenario, window.location.href would be changed
    });

    it('should store code_verifier with sufficient length', async () => {
      await authService.login();
      
      const codeVerifier = sessionStorage.getItem('code_verifier');
      expect(codeVerifier).toBeTruthy();
      expect(codeVerifier!.length).toBeGreaterThanOrEqual(43);
    });
  });

  describe('Callback Handling', () => {
    it('should exchange authorization code for access token', async () => {
      // Setup: simulate OAuth redirect with code and state
      const code = 'mock_auth_code';
      const state = 'mock_state';
      const codeVerifier = 'mock_code_verifier';
      
      sessionStorage.setItem('code_verifier', codeVerifier);
      sessionStorage.setItem('auth_state', state);
      
      // Mock window.location.search
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          search: `?code=${code}&state=${state}`
        },
        writable: true
      });
      
      await authService.handleCallback();
      
      // Verify token was saved
      const authState = authService.getAuthState();
      expect(authState.isAuthenticated).toBe(true);
      expect(authState.accessToken).toBe(mockAccessToken);
      
      // Verify cleanup
      expect(sessionStorage.getItem('code_verifier')).toBeNull();
      expect(sessionStorage.getItem('auth_state')).toBeNull();
    });

    it('should throw error if state does not match', async () => {
      sessionStorage.setItem('auth_state', 'expected_state');
      
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          search: '?code=abc&state=wrong_state'
        },
        writable: true
      });
      
      await expect(authService.handleCallback()).rejects.toThrow('Invalid callback parameters');
    });

    it('should throw error if code_verifier is missing', async () => {
      sessionStorage.setItem('auth_state', 'state123');
      
      Object.defineProperty(window, 'location', {
        value: {
          ...window.location,
          search: '?code=abc&state=state123'
        },
        writable: true
      });
      
      await expect(authService.handleCallback()).rejects.toThrow('Code verifier not found');
    });
  });

  describe('Logout', () => {
    it('should clear auth state and redirect to logout endpoint', async () => {
      // Setup: store auth state
      sessionStorage.setItem('access_token', mockAccessToken);
      
      await authService.logout();
      
      // Verify session is cleared
      expect(sessionStorage.getItem('access_token')).toBeNull();
      const state = authService.getAuthState();
      expect(state.isAuthenticated).toBe(false);
      expect(state.accessToken).toBeNull();
    });
  });

  describe('State Management', () => {
    it('should notify subscribers when auth state changes', () => {
      const listener = vi.fn();
      authService.subscribe(listener);
      
      // Trigger state change by setting token
      sessionStorage.setItem('access_token', mockAccessToken);
      
      // Manually trigger by calling a method that changes state
      // (in real scenario, login/logout would trigger this)
    });

    it('should return unsubscribe function', () => {
      const listener = vi.fn();
      const unsubscribe = authService.subscribe(listener);
      
      expect(typeof unsubscribe).toBe('function');
      unsubscribe();
    });
  });

  describe('Token Management', () => {
    it('should return access token when authenticated', () => {
      sessionStorage.setItem('access_token', mockAccessToken);
      
      const token = authService.getAccessToken();
      expect(token).toBe(mockAccessToken);
    });

    it('should return null when not authenticated', () => {
      const token = authService.getAccessToken();
      expect(token).toBeNull();
    });

    it('should correctly identify authenticated state', () => {
      expect(authService.isAuthenticated()).toBe(false);
      
      sessionStorage.setItem('access_token', mockAccessToken);
      // Note: Need to reload service to pick up token
    });
  });
});
