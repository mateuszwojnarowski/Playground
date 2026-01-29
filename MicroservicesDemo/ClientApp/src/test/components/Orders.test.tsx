// ============================================================================
// ORDERS COMPONENT TESTS
// ============================================================================
// Tests for the orders management component.
// ============================================================================

import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Orders from '../../components/Orders';
import { AuthProvider } from '../../contexts/AuthContext';
import { mockAccessToken, mockOrders } from '../mockData';

describe('Orders Component', () => {
  beforeEach(() => {
    sessionStorage.setItem('access_token', mockAccessToken);
  });

  it('should render loading state initially', () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    expect(screen.getByText('Loading orders...')).toBeInTheDocument();
  });

  it('should render orders table after loading', async () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
    });
    
    // Check table headers
    expect(screen.getByText('Order ID')).toBeInTheDocument();
    expect(screen.getByText('Items Count')).toBeInTheDocument();
    expect(screen.getByText('Actions')).toBeInTheDocument();
  });

  it('should display orders with correct data', async () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
    });
    
    // Check order IDs are displayed
    expect(screen.getByText(mockOrders[0].id)).toBeInTheDocument();
  });

  it('should open modal when "Create Order" button is clicked', async () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
    });
    
    const createButton = screen.getByRole('button', { name: /create order/i });
    fireEvent.click(createButton);
    
    // Modal should appear
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /create order/i })).toBeInTheDocument();
    });
  });

  it('should show "View Details" button for each order', async () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
    });
    
    const viewButtons = screen.getAllByRole('button', { name: /view details/i });
    expect(viewButtons.length).toBeGreaterThan(0);
  });

  it('should show empty state when no orders exist', async () => {
    const { http, HttpResponse } = await import('msw');
    const { server } = await import('../mocks/server');
    
    // Mock empty orders response
    server.use(
      http.get('/api/orders', () => {
        return HttpResponse.json([]);
      }),
      http.get('/api/products', () => {
        return HttpResponse.json([]);
      })
    );
    
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText(/no orders found/i)).toBeInTheDocument();
    });
  });

  it('should load and display products in create order form', async () => {
    render(
      <AuthProvider>
        <Orders />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
    });
    
    // Open create order modal
    const createButton = screen.getByRole('button', { name: /create order/i });
    fireEvent.click(createButton);
    
    // Check that product dropdown exists
    await waitFor(() => {
      expect(screen.getByText(/select a product/i)).toBeInTheDocument();
    });
  });
});
