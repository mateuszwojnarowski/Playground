// ============================================================================
// API SERVICE TESTS
// ============================================================================
// Tests for API client service with token management and error handling.
// ============================================================================

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { apiService } from '../../services/apiService';
import { mockProducts, mockOrders, mockAccessToken } from '../mockData';
import { authService } from '../../services/authService';

describe('ApiService', () => {
  beforeEach(() => {
    // Setup authenticated state for API calls
    sessionStorage.setItem('access_token', mockAccessToken);
  });

  describe('Products API', () => {
    it('should fetch all products', async () => {
      const products = await apiService.getProducts();
      
      expect(Array.isArray(products)).toBe(true);
      expect(products.length).toBe(3);
      expect(products[0]).toHaveProperty('id');
      expect(products[0]).toHaveProperty('name');
      expect(products[0]).toHaveProperty('cost');
    });

    it('should fetch a single product by ID', async () => {
      const productId = mockProducts[0].id;
      const product = await apiService.getProduct(productId);
      
      expect(product).toBeDefined();
      expect(product.id).toBe(productId);
      expect(product.name).toBe('Laptop');
    });

    it('should return 404 for non-existent product', async () => {
      await expect(
        apiService.getProduct('non-existent-id')
      ).rejects.toThrow();
    });

    it('should create a new product', async () => {
      const newProduct = {
        name: 'Test Product',
        description: 'Test Description',
        cost: 99.99,
        stockQuantity: 100
      };
      
      const created = await apiService.createProduct(newProduct);
      
      expect(created).toHaveProperty('id');
      expect(created.name).toBe(newProduct.name);
    });

    it('should update product stock', async () => {
      const productId = mockProducts[0].id;
      
      await expect(
        apiService.updateProductStock(productId, 50)
      ).resolves.not.toThrow();
    });

    it('should reject negative stock quantity', async () => {
      const productId = mockProducts[0].id;
      
      await expect(
        apiService.updateProductStock(productId, -5)
      ).rejects.toThrow();
    });

    it('should delete a product', async () => {
      const productId = mockProducts[0].id;
      
      await expect(
        apiService.deleteProduct(productId)
      ).resolves.not.toThrow();
    });
  });

  describe('Orders API', () => {
    it('should fetch all orders', async () => {
      const orders = await apiService.getOrders();
      
      expect(Array.isArray(orders)).toBe(true);
      expect(orders.length).toBeGreaterThan(0);
      expect(orders[0]).toHaveProperty('id');
      expect(orders[0]).toHaveProperty('orderDetails');
    });

    it('should fetch a single order by ID', async () => {
      const orderId = mockOrders[0].id;
      const order = await apiService.getOrder(orderId);
      
      expect(order).toBeDefined();
      expect(order.id).toBe(orderId);
    });

    it('should fetch order details', async () => {
      const orderId = mockOrders[0].id;
      const details = await apiService.getOrderDetails(orderId);
      
      expect(Array.isArray(details)).toBe(true);
      expect(details.length).toBeGreaterThan(0);
      expect(details[0]).toHaveProperty('productId');
      expect(details[0]).toHaveProperty('quantity');
    });

    it('should create a new order', async () => {
      const orderItems = [
        { productId: mockProducts[0].id, quantity: 2 },
        { productId: mockProducts[1].id, quantity: 1 }
      ];
      
      const created = await apiService.createOrder(orderItems);
      
      expect(created).toHaveProperty('id');
      expect(created).toHaveProperty('orderDetails');
    });
  });

  describe('Authentication Integration', () => {
    it('should include Bearer token in requests', async () => {
      // This is tested implicitly through MSW handlers
      // which check for Authorization header
      const products = await apiService.getProducts();
      expect(products).toBeDefined();
    });

    it('should handle 401 unauthorized by logging out', async () => {
      // Mock 401 response
      const { http, HttpResponse } = await import('msw');
      const { server } = await import('../mocks/server');
      
      server.use(
        http.get('/api/products', () => {
          return new HttpResponse(null, { status: 401 });
        })
      );
      
      const logoutSpy = vi.spyOn(authService, 'logout');
      
      try {
        await apiService.getProducts();
      } catch (error) {
        // Expected to fail
      }
      
      expect(logoutSpy).toHaveBeenCalled();
      logoutSpy.mockRestore();
    });
  });

  describe('Error Handling', () => {
    it('should handle network errors gracefully', async () => {
      const { http, HttpResponse } = await import('msw');
      const { server } = await import('../mocks/server');
      
      server.use(
        http.get('/api/products', () => {
          return HttpResponse.error();
        })
      );
      
      await expect(apiService.getProducts()).rejects.toThrow();
    });

    it('should handle server errors (500)', async () => {
      const { http, HttpResponse } = await import('msw');
      const { server } = await import('../mocks/server');
      
      server.use(
        http.get('/api/products', () => {
          return new HttpResponse(null, { status: 500 });
        })
      );
      
      await expect(apiService.getProducts()).rejects.toThrow();
    });
  });
});
