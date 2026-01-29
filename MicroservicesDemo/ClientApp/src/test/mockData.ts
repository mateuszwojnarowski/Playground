// ============================================================================
// MOCK DATA FOR TESTS
// ============================================================================
// Reusable test data for products, orders, and users.
// ============================================================================

import { Product, Order, OrderItem } from '../types';

/**
 * Mock Products
 */
export const mockProducts: Product[] = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    name: 'Laptop',
    description: 'High-performance laptop',
    cost: 999.99,
    stockQuantity: 10
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    name: 'Mouse',
    description: 'Wireless mouse',
    cost: 29.99,
    stockQuantity: 50
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    name: 'Keyboard',
    description: null,
    cost: 79.99,
    stockQuantity: 0 // Out of stock
  }
];

/**
 * Mock Order Items
 */
export const mockOrderItems: OrderItem[] = [
  {
    id: '660e8400-e29b-41d4-a716-446655440001',
    productId: '550e8400-e29b-41d4-a716-446655440001',
    quantity: 2
  },
  {
    id: '660e8400-e29b-41d4-a716-446655440002',
    productId: '550e8400-e29b-41d4-a716-446655440002',
    quantity: 1
  }
];

/**
 * Mock Orders
 */
export const mockOrders: Order[] = [
  {
    id: '770e8400-e29b-41d4-a716-446655440001',
    orderDetails: mockOrderItems
  },
  {
    id: '770e8400-e29b-41d4-a716-446655440002',
    orderDetails: [
      {
        id: '660e8400-e29b-41d4-a716-446655440003',
        productId: '550e8400-e29b-41d4-a716-446655440003',
        quantity: 1
      }
    ]
  }
];

/**
 * Mock JWT Access Token
 * Contains user info in payload
 */
export const mockAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJqb2huQGV4YW1wbGUuY29tIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c';

/**
 * Mock User Info (decoded from token)
 */
export const mockUser = {
  sub: '1234567890',
  name: 'John Doe',
  email: 'john@example.com'
};

/**
 * Mock OAuth Response
 */
export const mockTokenResponse = {
  access_token: mockAccessToken,
  token_type: 'Bearer',
  expires_in: 3600
};
