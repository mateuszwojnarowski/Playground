// ============================================================================
// PROTECTED ROUTE COMPONENT
// ============================================================================
// Route guard that requires authentication to access wrapped components.
// Redirects unauthenticated users to login page.
// ============================================================================

import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;  // Components to render if authenticated
}

/**
 * Protected Route Guard Component
 * 
 * Wraps routes that require authentication.
 * 
 * Usage in routing:
 *   <Route path="/products" element={
 *     <ProtectedRoute><Products /></ProtectedRoute>
 *   } />
 * 
 * Logic:
 * - If authenticated: render children (the protected component)
 * - If not authenticated: redirect to /login
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated } = useAuth();

  // Guard: redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // User is authenticated, render the protected component
  return <>{children}</>;
};

export default ProtectedRoute;
