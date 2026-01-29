// ============================================================================
// LOGIN COMPONENT TESTS
// ============================================================================
// Tests for the login page component.
// ============================================================================

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import Login from '../../components/Login';
import { AuthProvider } from '../../contexts/AuthContext';
import { authService } from '../../services/authService';

describe('Login Component', () => {
  it('should render login page with title and button', () => {
    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    );
    
    expect(screen.getByText('Microservices Demo')).toBeInTheDocument();
    expect(screen.getByText('Please login to access the application')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login with identityserver/i })).toBeInTheDocument();
  });

  it('should call login function when button is clicked', async () => {
    const loginSpy = vi.spyOn(authService, 'login').mockResolvedValue();
    
    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    );
    
    const loginButton = screen.getByRole('button', { name: /login with identityserver/i });
    fireEvent.click(loginButton);
    
    expect(loginSpy).toHaveBeenCalled();
    
    loginSpy.mockRestore();
  });
});
