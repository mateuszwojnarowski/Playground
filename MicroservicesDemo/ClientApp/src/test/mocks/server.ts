// ============================================================================
// MSW SERVER SETUP
// ============================================================================
// Mock Service Worker server for intercepting HTTP requests in tests.
// ============================================================================

import { setupServer } from 'msw/node';
import { handlers } from './handlers';

// Create MSW server with default handlers
export const server = setupServer(...handlers);
