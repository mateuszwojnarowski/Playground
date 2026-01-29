// ============================================================================
// TYPE DEFINITIONS
// ============================================================================
// This file contains all TypeScript interfaces and types used throughout
// the application. These ensure type safety and better IDE support.
// ============================================================================

/**
 * Product entity from the Products microservice
 * Represents a product that can be ordered
 */
export interface Product {
  id: string;                      // Unique product identifier (GUID)
  name: string;                    // Product name (max 250 chars)
  description: string | null;      // Optional product description (max 2048 chars)
  cost: number;                    // Product cost in decimal format
  stockQuantity: number;           // Available stock quantity
}

/**
 * Order item entity representing a single product in an order
 * Part of an order with quantity information
 */
export interface OrderItem {
  id: string;                      // Unique order item identifier (GUID)
  productId: string;               // Reference to the product being ordered
  quantity: number;                // Number of units ordered
}

/**
 * Order entity from the Orders microservice
 * Contains collection of order items
 */
export interface Order {
  id: string;                      // Unique order identifier (GUID)
  orderDetails: OrderItem[];       // List of items in this order
}

/**
 * OAuth2/OIDC configuration settings
 * Used to configure the authentication flow with IdentityServer
 */
export interface AuthConfig {
  authority: string;               // IdentityServer URL (token issuer)
  clientId: string;                // Client identifier registered in IdentityServer
  redirectUri: string;             // Where IdentityServer redirects after login
  postLogoutRedirectUri: string;   // Where to redirect after logout
  responseType: string;            // OAuth flow type (code = Authorization Code)
  scope: string;                   // Space-separated list of requested scopes
}

/**
 * Current authentication state of the user
 * Tracks whether user is logged in and their access token
 */
export interface AuthState {
  isAuthenticated: boolean;        // True if user has valid access token
  accessToken: string | null;      // JWT access token for API calls
  user: UserInfo | null;           // Decoded user information from token
}

/**
 * User information decoded from JWT access token
 * Contains identity claims from IdentityServer
 */
export interface UserInfo {
  sub: string;                     // Subject (unique user identifier)
  name?: string;                   // User's display name (optional)
  email?: string;                  // User's email address (optional)
}
