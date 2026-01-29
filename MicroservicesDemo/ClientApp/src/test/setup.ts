// ============================================================================
// TEST SETUP FILE
// ============================================================================
// Runs before all test files. Sets up testing library and global mocks.
// ============================================================================

import '@testing-library/jest-dom';
import { afterEach, beforeAll, afterAll, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import { server } from './mocks/server';

// Setup MSW (Mock Service Worker) for API mocking
beforeAll(() => {
  server.listen({ onUnhandledRequest: 'error' });
});

// Clean up after each test
afterEach(() => {
  cleanup(); // Clean up React components
  server.resetHandlers(); // Reset MSW handlers
  vi.clearAllMocks(); // Clear all mocks
  sessionStorage.clear(); // Clear session storage
});

// Cleanup after all tests
afterAll(() => {
  server.close();
});

// Mock window.location methods
Object.defineProperty(window, 'location', {
  value: {
    href: 'http://localhost:3000',
    origin: 'http://localhost:3000',
    protocol: 'http:',
    host: 'localhost:3000',
    hostname: 'localhost',
    port: '3000',
    pathname: '/',
    search: '',
    hash: '',
    replace: vi.fn(),
    assign: vi.fn(),
    reload: vi.fn(),
    toString: () => 'http://localhost:3000'
  },
  writable: true,
  configurable: true
});

// Mock crypto.getRandomValues (used by PKCE)
Object.defineProperty(global, 'crypto', {
  value: {
    getRandomValues: (arr: Uint8Array) => {
      for (let i = 0; i < arr.length; i++) {
        arr[i] = Math.floor(Math.random() * 256);
      }
      return arr;
    },
    subtle: {
      digest: async (_algorithm: string, _data: BufferSource) => {
        // Simple mock for SHA-256
        const buffer = new Uint8Array(32);
        for (let i = 0; i < buffer.length; i++) {
          buffer[i] = i;
        }
        return buffer.buffer;
      }
    }
  }
});
