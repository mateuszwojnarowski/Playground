// ============================================================================
// MAIN APPLICATION COMPONENT
// ============================================================================
// Root component that sets up routing and authentication context.
//
// Route Structure:
// - /login: Public route for unauthenticated users
// - /callback: OAuth callback handler (public)
// - /*: All other routes are protected and require authentication
//   - /: Home page
//   - /products: Products management
//   - /orders: Orders management
// ============================================================================

import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar';
import Login from './components/Login';
import Callback from './components/Callback';
import Home from './components/Home';
import Products from './components/Products';
import Orders from './components/Orders';
import './App.css';

/**
 * Main Application Component
 * 
 * Architecture:
 * 1. AuthProvider wraps everything to provide auth context
 * 2. BrowserRouter enables client-side routing
 * 3. Public routes (login, callback) are accessible to all
 * 4. Protected routes require authentication
 * 5. Navbar is only shown on protected routes
 */
const App: React.FC = () => {
  return (
    // Provide authentication context to entire app
    <AuthProvider>
      {/* Enable client-side routing */}
      <BrowserRouter>
        <Routes>
          {/* PUBLIC ROUTES */}
          {/* Login page for unauthenticated users */}
          <Route path="/login" element={<Login />} />
          
          {/* OAuth callback from IdentityServer */}
          <Route path="/callback" element={<Callback />} />
          
          {/* PROTECTED ROUTES */}
          {/* All other routes require authentication */}
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                {/* Show navbar on all authenticated pages */}
                <Navbar />
                {/* Nested routes for authenticated sections */}
                <Routes>
                  <Route path="/" element={<Home />} />
                  <Route path="/products" element={<Products />} />
                  <Route path="/orders" element={<Orders />} />
                  {/* Catch-all: redirect unknown routes to home */}
                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;
