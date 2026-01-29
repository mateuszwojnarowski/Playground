// ============================================================================
// API SERVICE
// ============================================================================
// Centralized API client for communicating with backend microservices through
// the API Gateway. Handles:
// - Automatic bearer token injection
// - Request/response interceptors
// - Error handling and authentication expiry
// - Type-safe API methods
// ============================================================================

import axios, { AxiosInstance } from 'axios';
import { authService } from './authService';
import { Product, Order, OrderItem } from '../types';
import { API_BASE_URL } from '../config';

/**
 * API Service Class
 * 
 * Provides methods for all backend API operations with:
 * - Automatic access token attachment
 * - Centralized error handling
 * - TypeScript type safety
 */
class ApiService {
  private axiosInstance: AxiosInstance;

  constructor() {
    // Create configured axios instance
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,  // '/api' - proxied to API Gateway
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // ========================================================================
    // REQUEST INTERCEPTOR
    // ========================================================================
    // Automatically adds the Bearer token to every request
    // This runs before each request is sent
    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = authService.getAccessToken();
        if (token) {
          // Add Authorization header with Bearer token
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // ========================================================================
    // RESPONSE INTERCEPTOR
    // ========================================================================
    // Handles errors, especially 401 Unauthorized (expired/invalid token)
    // This runs after each response is received
    this.axiosInstance.interceptors.response.use(
      (response) => response, // Pass through successful responses
      (error) => {
        // If we get 401 Unauthorized, token is invalid/expired
        if (error.response?.status === 401) {
          // Automatically logout and redirect to login
          authService.logout();
        }
        return Promise.reject(error);
      }
    );
  }

  // ==========================================================================
  // PRODUCTS API METHODS
  // ==========================================================================
  // All methods communicate with the Products microservice via API Gateway
  // Routes are configured in API Gateway to forward /products to ProductsService

  /**
   * Get all products from the catalog
   * 
   * @returns Array of all products
   */
  async getProducts(): Promise<Product[]> {
    const response = await this.axiosInstance.get<Product[]>('/products');
    return response.data;
  }

  /**
   * Get a single product by ID
   * 
   * @param id - Product GUID
   * @returns Product details
   */
  async getProduct(id: string): Promise<Product> {
    const response = await this.axiosInstance.get<Product>(`/products/${id}`);
    return response.data;
  }

  /**
   * Create a new product
   * 
   * Requires 'product.edit' scope in access token
   * 
   * @param product - Product data (without ID, server generates it)
   * @returns Created product with generated ID
   */
  async createProduct(product: Omit<Product, 'id'>): Promise<Product> {
    const response = await this.axiosInstance.post<Product>('/products', product);
    return response.data;
  }

  /**
   * Update product stock quantity
   * 
   * Requires 'product.stock' scope in access token.
   * Note: This endpoint only updates stock, not other fields.
   * 
   * @param id - Product GUID
   * @param stockQuantity - New stock quantity (must be >= 0)
   */
  async updateProductStock(id: string, stockQuantity: number): Promise<void> {
    await this.axiosInstance.put(`/products/${id}/${stockQuantity}`);
  }

  /**
   * Delete a product
   * 
   * Requires 'product.edit' scope in access token
   * 
   * @param id - Product GUID to delete
   */
  async deleteProduct(id: string): Promise<void> {
    await this.axiosInstance.delete(`/products/${id}`);
  }

  // ==========================================================================
  // ORDERS API METHODS
  // ==========================================================================
  // All methods communicate with the Orders microservice via API Gateway
  // Routes are configured in API Gateway to forward /orders to OrderService

  /**
   * Get all orders
   * 
   * @returns Array of all orders with their items
   */
  async getOrders(): Promise<Order[]> {
    const response = await this.axiosInstance.get<Order[]>('/orders');
    return response.data;
  }

  /**
   * Get a single order by ID
   * 
   * @param id - Order GUID
   * @returns Order details
   */
  async getOrder(id: string): Promise<Order> {
    const response = await this.axiosInstance.get<Order>(`/orders/${id}`);
    return response.data;
  }

  /**
   * Get detailed items for a specific order
   * 
   * Returns the list of products and quantities in this order
   * 
   * @param id - Order GUID
   * @returns Array of order items with product IDs and quantities
   */
  async getOrderDetails(id: string): Promise<OrderItem[]> {
    const response = await this.axiosInstance.get<OrderItem[]>(`/orders/${id}/OrderDetails`);
    return response.data;
  }

  /**
   * Create a new order
   * 
   * Requires 'order.edit' scope in access token.
   * 
   * The backend will:
   * 1. Validate all products exist and have sufficient stock
   * 2. Reduce stock quantities
   * 3. Create the order
   * 
   * If stock is insufficient, the request will fail with 400 Bad Request.
   * 
   * @param orderDetails - Array of items to order (productId + quantity)
   * @returns Created order with generated ID
   */
  async createOrder(orderDetails: Omit<OrderItem, 'id'>[]): Promise<Order> {
    const order = {
      orderDetails: orderDetails
    };
    const response = await this.axiosInstance.post<Order>('/orders', order);
    return response.data;
  }
}

// Export singleton instance
export const apiService = new ApiService();
