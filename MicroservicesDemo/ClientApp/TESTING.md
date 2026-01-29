# Testing Guide

## Overview

This project uses a comprehensive testing setup with:

- **Vitest** - Fast Vite-native test runner
- **React Testing Library** - Component testing utilities
- **MSW (Mock Service Worker)** - API mocking
- **@testing-library/jest-dom** - Custom matchers

## Running Tests

```bash
# Run all tests
npm test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Run tests in watch mode
npm test -- --watch

# Run specific test file
npm test -- Products.test.tsx
```

## Test Structure

```
src/test/
├── setup.ts                      # Global test setup
├── mockData.ts                   # Reusable mock data
├── mocks/
│   ├── server.ts                 # MSW server setup
│   └── handlers.ts               # API request handlers
├── services/
│   ├── authService.test.ts       # Auth service tests
│   └── apiService.test.ts        # API client tests
├── contexts/
│   └── AuthContext.test.tsx      # Auth context tests
└── components/
    ├── Login.test.tsx            # Login component tests
    ├── ProtectedRoute.test.tsx   # Route guard tests
    ├── Products.test.tsx         # Products component tests
    └── Orders.test.tsx           # Orders component tests
```

## Test Coverage

### Services (100% target)

**authService.ts**
- ✅ PKCE code generation
- ✅ Login flow initiation
- ✅ OAuth callback handling
- ✅ Token storage and retrieval
- ✅ Logout functionality
- ✅ State management and subscriptions

**apiService.ts**
- ✅ HTTP interceptors (request/response)
- ✅ Automatic token injection
- ✅ Products CRUD operations
- ✅ Orders CRUD operations
- ✅ Error handling
- ✅ 401 auto-logout

### Components

**Login.tsx**
- ✅ Renders login UI
- ✅ Triggers OAuth flow on button click

**ProtectedRoute.tsx**
- ✅ Redirects unauthenticated users
- ✅ Renders protected content when authenticated

**Products.tsx**
- ✅ Loads and displays products
- ✅ Create product modal
- ✅ Update stock functionality
- ✅ Delete product with confirmation
- ✅ Empty state handling

**Orders.tsx**
- ✅ Loads and displays orders
- ✅ View order details
- ✅ Create order with multiple items
- ✅ Product selection
- ✅ Empty state handling

### Context

**AuthContext.tsx**
- ✅ Provides auth state to children
- ✅ Exposes login/logout methods
- ✅ Updates on auth changes
- ✅ Throws error when used incorrectly

## Mock Service Worker (MSW)

MSW intercepts network requests at the network level, providing realistic API mocking.

### Available Mocked Endpoints

**Authentication**
- `POST /connect/token` - OAuth token exchange

**Products API**
- `GET /api/products` - Get all products
- `GET /api/products/:id` - Get single product
- `POST /api/products` - Create product
- `PUT /api/products/:id/:stockQuantity` - Update stock
- `DELETE /api/products/:id` - Delete product

**Orders API**
- `GET /api/orders` - Get all orders
- `GET /api/orders/:id` - Get single order
- `GET /api/orders/:id/OrderDetails` - Get order items
- `POST /api/orders` - Create order

### Customizing Handlers

Override default handlers for specific tests:

```typescript
import { http, HttpResponse } from 'msw';
import { server } from './test/mocks/server';

it('should handle API error', async () => {
  // Override handler for this test
  server.use(
    http.get('/api/products', () => {
      return new HttpResponse(null, { status: 500 });
    })
  );
  
  // Test error handling...
});
```

## Writing New Tests

### Component Test Template

```typescript
import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { AuthProvider } from '../../contexts/AuthContext';
import { mockAccessToken } from '../mockData';
import YourComponent from '../../components/YourComponent';

describe('YourComponent', () => {
  beforeEach(() => {
    sessionStorage.setItem('access_token', mockAccessToken);
  });

  it('should render correctly', async () => {
    render(
      <AuthProvider>
        <YourComponent />
      </AuthProvider>
    );
    
    await waitFor(() => {
      expect(screen.getByText('Expected Text')).toBeInTheDocument();
    });
  });
});
```

### Service Test Template

```typescript
import { describe, it, expect } from 'vitest';
import { yourService } from '../../services/yourService';

describe('YourService', () => {
  it('should perform operation', async () => {
    const result = await yourService.doSomething();
    expect(result).toBeDefined();
  });
});
```

## Best Practices

### 1. **Arrange-Act-Assert Pattern**
```typescript
it('should do something', () => {
  // Arrange: Set up test data
  const data = { value: 123 };
  
  // Act: Perform action
  const result = processData(data);
  
  // Assert: Verify result
  expect(result).toBe(123);
});
```

### 2. **Use Semantic Queries**
Prefer queries that match how users interact:
```typescript
// Good
screen.getByRole('button', { name: /submit/i });
screen.getByLabelText('Email');
screen.getByText('Welcome');

// Avoid
screen.getByTestId('submit-button');
```

### 3. **Wait for Async Operations**
```typescript
await waitFor(() => {
  expect(screen.getByText('Loaded')).toBeInTheDocument();
});
```

### 4. **Clean Up**
Tests automatically clean up with `afterEach` in setup.ts. No manual cleanup needed.

### 5. **Mock Only What's Necessary**
MSW handles API mocking. Mock only external dependencies:
```typescript
const logSpy = vi.spyOn(console, 'log');
// ... test code ...
logSpy.mockRestore();
```

## Debugging Tests

### View Test Output
```bash
npm run test:ui
```
Opens Vitest UI in browser for interactive debugging.

### Debug Specific Test
```typescript
it.only('should debug this test', () => {
  // Only this test will run
});
```

### Check Rendered Output
```typescript
import { screen, debug } from '@testing-library/react';

it('should render', () => {
  render(<Component />);
  screen.debug(); // Prints DOM to console
});
```

## Coverage Reports

After running `npm run test:coverage`, view the HTML report:
```
open coverage/index.html
```

## CI/CD Integration

Tests can run in CI pipelines:

```yaml
# GitHub Actions example
- name: Run tests
  run: npm test -- --run

- name: Generate coverage
  run: npm run test:coverage
```

## Troubleshooting

### Tests Hang or Timeout
- Check for missing `await` on async operations
- Verify MSW handlers are correctly set up
- Increase timeout: `it('test', async () => {...}, 10000)`

### "Network request failed"
- Ensure MSW server is running (check setup.ts)
- Verify handler endpoints match test URLs
- Check for typos in URLs

### Component Not Found
- Ensure component is wrapped in necessary providers (AuthProvider, Router, etc.)
- Use `await waitFor()` for async rendering

### Mock Not Working
- Check that mock is setup before the code runs
- Use `vi.clearAllMocks()` between tests (automatic in setup)
- Verify import paths match

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [MSW Documentation](https://mswjs.io/)
- [Testing Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)
