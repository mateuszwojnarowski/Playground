// ============================================================================
// PRODUCTS COMPONENT TESTS
// ============================================================================
// Tests for the products management component.
// ============================================================================

import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import Products from '../../components/Products';
import { AuthProvider } from '../../contexts/AuthContext';
import { mockAccessToken } from '../mockData';

describe('Products Component', () => {
  beforeEach(() => {
    sessionStorage.setItem('access_token', mockAccessToken);
  });

  it('should render loading state initially', () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    expect(screen.getByText('Loading products...')).toBeInTheDocument();
  });

  it('should render products table after loading', async () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    // Wait for products to load
    await waitFor(() => {
      expect(screen.queryByText('Loading products...')).not.toBeInTheDocument();
    });
    
    // Check table headers
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Cost')).toBeInTheDocument();
    expect(screen.getByText('Stock')).toBeInTheDocument();
    
    // Check product data
    expect(screen.getByText('Laptop')).toBeInTheDocument();
    expect(screen.getByText('Mouse')).toBeInTheDocument();
  });

  it('should open modal when "Add Product" button is clicked', async () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.queryByText('Loading products...')).not.toBeInTheDocument();
    });
    
    const addButton = screen.getByRole('button', { name: /add product/i });
    fireEvent.click(addButton);
    
    // Modal should appear
    expect(screen.getByRole('heading', { name: /add product/i })).toBeInTheDocument();
  });

  it('should display products with correct data', async () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText('Laptop')).toBeInTheDocument();
    });
    
    // Check first product details
    expect(screen.getByText('$999.99')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument(); // Stock quantity
  });

  it('should show "Update Stock" button for each product', async () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText('Laptop')).toBeInTheDocument();
    });
    
    const updateButtons = screen.getAllByRole('button', { name: /update stock/i });
    expect(updateButtons.length).toBeGreaterThan(0);
  });

  it('should show "Delete" button for each product', async () => {
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText('Laptop')).toBeInTheDocument();
    });
    
    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    expect(deleteButtons.length).toBeGreaterThan(0);
  });

  it('should show empty state when no products exist', async () => {
    const { http, HttpResponse } = await import('msw');
    const { server } = await import('../mocks/server');
    
    // Mock empty products response
    server.use(
      http.get('/api/products', () => {
        return HttpResponse.json([]);
      })
    );
    
    render(
      <AuthProvider>
        <Products />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText(/no products found/i)).toBeInTheDocument();
    });
  });
});
