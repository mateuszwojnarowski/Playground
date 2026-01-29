// ============================================================================
// NAVIGATION BAR COMPONENT
// ============================================================================
// Top navigation bar displayed to authenticated users.
// Shows navigation links, user info, and logout button.
// ============================================================================

import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/**
 * Navigation Bar Component
 * 
 * Displays:
 * - Links to main app sections (Home, Products, Orders)
 * - User's name/ID from JWT token
 * - Logout button
 */
const Navbar: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <nav>
      <ul>
        {/* Navigation links using React Router */}
        <li><Link to="/">Home</Link></li>
        <li><Link to="/products">Products</Link></li>
        <li><Link to="/orders">Orders</Link></li>
        
        {/* User info section (pushed to right with CSS) */}
        <div className="user-info">
          {/* Display user's name from token, fallback to subject ID */}
          <span>Hello, {user?.name || user?.sub}</span>
          
          {/* Logout button triggers logout flow */}
          <button className="secondary" onClick={logout}>Logout</button>
        </div>
      </ul>
    </nav>
  );
};

export default Navbar;
