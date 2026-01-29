// ============================================================================
// MSW REQUEST HANDLERS
// ============================================================================
// Mock API responses for testing without hitting real backend.
// ============================================================================

import { http, HttpResponse } from 'msw';
import { mockProducts, mockOrders, mockOrderItems, mockTokenResponse } from '../mockData';

/**
 * API request handlers
 * These intercept API calls and return mock data
 */
export const handlers = [
  // ========================================================================
  // AUTHENTICATION ENDPOINTS
  // ========================================================================
  
  // Token endpoint (OAuth token exchange)
  http.post('https://localhost:5001/connect/token', () => {
    return HttpResponse.json(mockTokenResponse);
  }),
  
  // ========================================================================
  // PRODUCTS ENDPOINTS
  // ========================================================================
  
  // GET /api/products - Get all products
  http.get('/api/products', () => {
    return HttpResponse.json(mockProducts);
  }),
  
  // GET /api/products/:id - Get single product
  http.get('/api/products/:id', ({ params }) => {
    const product = mockProducts.find(p => p.id === params.id);
    if (!product) {
      return new HttpResponse(null, { status: 404 });
    }
    return HttpResponse.json(product);
  }),
  
  // POST /api/products - Create product
  http.post('/api/products', async ({ request }) => {
    const newProduct = await request.json() as any;
    const created = {
      id: '550e8400-e29b-41d4-a716-446655440999',
      ...newProduct
    };
    return HttpResponse.json(created, { status: 201 });
  }),
  
  // PUT /api/products/:id/:stockQuantity - Update stock
  http.put('/api/products/:id/:stockQuantity', ({ params }) => {
    const stockQuantity = parseInt(params.stockQuantity as string);
    if (stockQuantity < 0) {
      return HttpResponse.json(
        { message: 'Stock quantity cannot be negative' },
        { status: 400 }
      );
    }
    return new HttpResponse(null, { status: 204 });
  }),
  
  // DELETE /api/products/:id - Delete product
  http.delete('/api/products/:id', () => {
    return new HttpResponse(null, { status: 204 });
  }),
  
  // ========================================================================
  // ORDERS ENDPOINTS
  // ========================================================================
  
  // GET /api/orders - Get all orders
  http.get('/api/orders', () => {
    return HttpResponse.json(mockOrders);
  }),
  
  // GET /api/orders/:id - Get single order
  http.get('/api/orders/:id', ({ params }) => {
    const order = mockOrders.find(o => o.id === params.id);
    if (!order) {
      return new HttpResponse(null, { status: 404 });
    }
    return HttpResponse.json(order);
  }),
  
  // GET /api/orders/:id/OrderDetails - Get order items
  http.get('/api/orders/:id/OrderDetails', () => {
    return HttpResponse.json(mockOrderItems);
  }),
  
  // POST /api/orders - Create order
  http.post('/api/orders', async ({ request }) => {
    const orderData = await request.json() as any;
    const created = {
      id: '770e8400-e29b-41d4-a716-446655440999',
      ...orderData
    };
    return HttpResponse.json(created, { status: 201 });
  })
];
