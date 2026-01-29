// ============================================================================
// VITE CONFIGURATION
// ============================================================================
// Vite is the build tool and dev server for this React application.
// Configuration includes:
// - React plugin for Fast Refresh and JSX support
// - Development server settings
// - API proxy to backend services
// ============================================================================

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  // Plugins
  plugins: [
    react() // Enables React Fast Refresh and JSX transformation
  ],
  
  // Development server configuration
  server: {
    port: 3000, // Run dev server on port 3000
    
    // Proxy configuration
    // In development, all /api requests are proxied to the API Gateway
    // This avoids CORS issues during development
    proxy: {
      '/api': {
        target: 'https://localhost:7290',  // API Gateway URL
        changeOrigin: true,                 // Change origin header to target
        secure: false,                      // Accept self-signed certificates
        rewrite: (path) => path.replace(/^\/api/, '') // Remove /api prefix before forwarding
        // Example: /api/products -> https://localhost:7290/products
      }
    }
  }
})
