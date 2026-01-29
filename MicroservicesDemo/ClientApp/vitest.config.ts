// ============================================================================
// VITEST CONFIGURATION
// ============================================================================
// Test configuration for Vitest (Vite-native test runner).
// Sets up the test environment and integrations.
// ============================================================================

import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    // Use jsdom to simulate browser environment
    environment: 'jsdom',
    
    // Setup files to run before each test file
    setupFiles: ['./src/test/setup.ts'],
    
    // Global test settings
    globals: true,
    
    // Coverage configuration
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockData.ts'
      ]
    }
  }
});
