# Microservices Demo - React Client Application

A React TypeScript client application that demonstrates OAuth2/OIDC authentication with IdentityServer and communication with microservices through an API Gateway.

## Features

- **OAuth2/OIDC Authentication**: Implements Authorization Code Flow with PKCE for secure authentication
- **API Gateway Integration**: All API calls route through the API Gateway with bearer token authentication
- **Product Management**: View, create, update stock, and delete products
- **Order Management**: View orders, create new orders with multiple items
- **Modern UI**: Clean, responsive interface built with React and TypeScript

## Architecture

```
┌─────────────────┐
│  React Client   │
│   (Port 3000)   │
└────────┬────────┘
         │
         ├─────────► IdentityServer (Port 5001) - Authentication
         │
         └─────────► API Gateway (Port 7290)
                            │
                            ├─────► Products Service
                            └─────► Orders Service
```

## Technology Stack

- **React 18** with TypeScript
- **Vite** for fast development and building
- **React Router** for navigation
- **Axios** for HTTP requests
- **OAuth2/OIDC** for authentication
- **CSS3** for styling

## Prerequisites

- Node.js 18+ and npm
- The backend services must be running:
  - IdentityServer (https://localhost:5001)
  - API Gateway (https://localhost:7290)
  - ProductsService
  - OrderService

## Getting Started

### Development Mode

1. **Install dependencies:**
   ```bash
   cd ClientApp
   npm install
   ```

2. **Start the development server:**
   ```bash
   npm run dev
   ```

3. **Open your browser:**
   Navigate to http://localhost:3000

### Running Tests

```bash
# Run all tests
npm test

# Run tests with UI
npm run test:ui

# Generate coverage report
npm run test:coverage
```

See [TESTING.md](TESTING.md) for comprehensive testing documentation.

### Production Build

```bash
npm run build
npm run preview
```

### Docker Deployment

The application is configured to run in Docker as part of the microservices stack:

```bash
# From the root directory
docker-compose up client-app
```

Or run all services:
```bash
docker-compose up
```

The application will be available at http://localhost:3000

## Configuration

### Authentication Settings

Configure in [src/config.ts](src/config.ts):

```typescript
export const authConfig: AuthConfig = {
  authority: 'https://localhost:5001',      // IdentityServer URL
  clientId: 'react-client',                 // Client ID from IdentityServer
  redirectUri: 'http://localhost:3000/callback',
  postLogoutRedirectUri: 'http://localhost:3000',
  responseType: 'code',
  scope: 'openid profile order.edit order.view product.edit product.view product.stock'
};
```

### API Gateway Settings

Configure in [src/config.ts](src/config.ts):

```typescript
export const API_BASE_URL = '/api';  // Proxied to API Gateway via Vite
```

The Vite dev server proxies `/api` requests to the API Gateway (configured in [vite.config.ts](vite.config.ts)).

## Project Structure

```
ClientApp/
├── src/
│   ├── components/          # React components
│   │   ├── Callback.tsx     # OAuth callback handler
│   │   ├── Home.tsx         # Home page
│   │   ├── Login.tsx        # Login page
│   │   ├── Navbar.tsx       # Navigation bar
│   │   ├── Orders.tsx       # Orders management
│   │   ├── Products.tsx     # Products management
│   │   └── ProtectedRoute.tsx  # Route guard
│   ├── contexts/
│   │   └── AuthContext.tsx  # Authentication context
│   ├── services/
│   │   ├── authService.ts   # Authentication service (PKCE flow)
│   │   └── apiService.ts    # API client with token management
│   ├── App.tsx              # Main application component
│   ├── App.css              # Global styles
│   ├── config.ts            # Application configuration
│   ├── types.ts             # TypeScript type definitions
│   └── main.tsx             # Application entry point
├── Dockerfile               # Docker configuration
├── nginx.conf               # Nginx configuration for production
├── package.json             # Dependencies and scripts
├── tsconfig.json            # TypeScript configuration
└── vite.config.ts           # Vite configuration
```

## Authentication Flow

1. **User clicks "Login"** → Redirected to IdentityServer
2. **User authenticates** → IdentityServer redirects back with authorization code
3. **Client exchanges code for token** → Using PKCE for security
4. **Token stored in session** → Used for all API requests
5. **Automatic token inclusion** → Axios interceptor adds Bearer token to requests
6. **401 handling** → Automatically redirects to login on token expiration

## API Integration

### Products API

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}/{stockQuantity}` - Update product stock
- `DELETE /api/products/{id}` - Delete product

### Orders API

- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/{id}/OrderDetails` - Get order details
- `POST /api/orders` - Create new order

## Security Features

- **PKCE (Proof Key for Code Exchange)**: Prevents authorization code interception
- **Token Management**: Secure storage in session storage
- **Automatic Token Refresh**: Redirects to login on token expiration
- **Route Protection**: Unauthenticated users redirected to login
- **CORS Configuration**: Properly configured in IdentityServer

## Default Users

Configure test users in IdentityServer's `SeedData.cs`:

- Username: `alice` / Password: `alice` (Full permissions)
- Username: `bob` / Password: `bob` (View only)

## Troubleshooting

### "Failed to fetch" errors
- Ensure all backend services are running
- Check that IdentityServer is accessible at https://localhost:5001
- Check that API Gateway is accessible at https://localhost:7290
- Accept self-signed certificates in your browser

### Authentication redirect loop
- Clear browser storage (session/local storage)
- Ensure redirect URIs match in IdentityServer Config.cs
- Check browser console for errors

### CORS errors
- Verify `AllowedCorsOrigins` in IdentityServer Config.cs includes `http://localhost:3000`
- Ensure API Gateway allows the requests

### API calls fail with 401
- Check that the access token includes required scopes
- Verify token hasn't expired (check browser console)
- Ensure API Gateway is configured to validate tokens from IdentityServer

## Development Tips

- Use browser DevTools to inspect authentication flow
- Check the Network tab to see token exchange
- Session storage contains the access token (inspect in Application tab)
- API errors are logged to the console

## Building for Production

The production build:
1. Compiles TypeScript to JavaScript
2. Bundles with Vite (tree-shaking, minification)
3. Creates optimized static files in `dist/`
4. Deployed via Nginx in Docker container

## License

This is a demo application for educational purposes.
