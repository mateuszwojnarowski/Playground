// =============================================================================
// Exercise 02 — Generics
// =============================================================================
// Complete each TODO below. Run `npm test` to check your solutions.

// ---------------------------------------------------------------------------
// Exercise 1: Implement a generic identity function
// It should return exactly the value it receives, preserving its type.
// ---------------------------------------------------------------------------

export function identity<T>(value: T): T {
  // TODO: Implement
  return undefined as unknown as T;
}

// ---------------------------------------------------------------------------
// Exercise 2: Return the first element of an array
// Return undefined for empty arrays.
// ---------------------------------------------------------------------------

export function first<T>(arr: T[]): T | undefined {
  // TODO: Implement
  return undefined;
}

// ---------------------------------------------------------------------------
// Exercise 3: Merge two objects into one
// The second object's properties should override the first's.
// ---------------------------------------------------------------------------

export function merge<A extends object, B extends object>(a: A, b: B): A & B {
  // TODO: Implement
  return {} as A & B;
}

// ---------------------------------------------------------------------------
// Exercise 4: Implement a generic Stack class
// Methods: push(item), pop(), peek(), isEmpty(), size()
// pop() and peek() should return undefined when the stack is empty.
// ---------------------------------------------------------------------------

export class Stack<T> {
  // TODO: Add private storage and implement methods

  push(_item: T): void {
    // TODO: Implement
  }

  pop(): T | undefined {
    // TODO: Implement
    return undefined;
  }

  peek(): T | undefined {
    // TODO: Implement
    return undefined;
  }

  isEmpty(): boolean {
    // TODO: Implement
    return true;
  }

  size(): number {
    // TODO: Implement
    return 0;
  }
}

// ---------------------------------------------------------------------------
// Exercise 5: Create a lookup map from an array of objects
// Given an array of objects and a key property name, return a Map that maps
// each key value to its corresponding object.
// If duplicate keys exist, later items overwrite earlier ones.
// ---------------------------------------------------------------------------

export function toLookup<T extends Record<string, unknown>, K extends keyof T>(
  items: T[],
  key: K,
): Map<T[K], T> {
  // TODO: Implement
  return new Map();
}
