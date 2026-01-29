// ============================================================================
// AUTHENTICATION CONTEXT
// ============================================================================
// React Context for managing authentication state across the application.
// Uses Context API + Provider pattern to make auth state available to all
// components without prop drilling.
//
// Components can access auth state and methods using the useAuth() hook.
// ============================================================================

import React, { createContext, useContext, useEffect, useState } from 'react';
import { authService } from '../services/authService';
import { AuthState } from '../types';

/**
 * Extended auth context type
 * Combines auth state with action methods
 */
interface AuthContextType extends AuthState {
  login: () => Promise<void>;    // Initiates OAuth login flow
  logout: () => Promise<void>;   // Logs out and clears session
}

// Create context with undefined default (will be provided by AuthProvider)
const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Authentication Provider Component
 * 
 * Wraps the app to provide auth state to all child components.
 * Subscribes to authService and updates React state when auth changes.
 * 
 * @param children - Child components that will have access to auth context
 */
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Local React state synced with authService
  const [authState, setAuthState] = useState<AuthState>(authService.getAuthState());

  useEffect(() => {
    // Subscribe to auth state changes from authService
    // This ensures React state stays in sync with the service
    const unsubscribe = authService.subscribe(setAuthState);
    
    // Cleanup: unsubscribe when component unmounts
    return unsubscribe;
  }, []); // Empty deps = run once on mount

  /**
   * Login method exposed to components
   * Delegates to authService to start OAuth flow
   */
  const login = async () => {
    await authService.login();
  };

  /**
   * Logout method exposed to components
   * Delegates to authService to clear session and redirect
   */
  const logout = async () => {
    await authService.logout();
  };

  // Provide auth state and methods to all children
  return (
    <AuthContext.Provider value={{ ...authState, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

/**
 * Custom hook to access authentication context
 * 
 * Usage in components:
 *   const { isAuthenticated, user, login, logout } = useAuth();
 * 
 * @returns Auth context with state and methods
 * @throws Error if used outside of AuthProvider
 */
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
