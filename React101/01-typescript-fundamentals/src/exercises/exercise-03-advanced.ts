// =============================================================================
// Exercise 03 — Advanced TypeScript
// =============================================================================
// Complete each TODO below. Run `npm test` to check your solutions.

// ---------------------------------------------------------------------------
// Exercise 1: Type-safe Event Emitter
// ---------------------------------------------------------------------------
// Define an EventMap type alias: keys are event names, values are arrays of
// callback argument types.  Then implement TypedEventEmitter<T>.

// Example EventMap:
//   { click: [x: number, y: number]; close: [] }

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type EventMap = Record<string, any[]>;

export class TypedEventEmitter<T extends EventMap> {
  // TODO: Add private listener storage

  on<K extends keyof T>(_event: K, _callback: (...args: T[K]) => void): void {
    // TODO: Implement — register the callback for the given event
  }

  off<K extends keyof T>(_event: K, _callback: (...args: T[K]) => void): void {
    // TODO: Implement — remove the callback for the given event
  }

  emit<K extends keyof T>(_event: K, ..._args: T[K]): void {
    // TODO: Implement — invoke all registered callbacks for the event
  }
}

// ---------------------------------------------------------------------------
// Exercise 2: Type guard — isNonNullable
// ---------------------------------------------------------------------------
// Implement a type guard that narrows T | null | undefined to T.

export function isNonNullable<T>(value: T): value is NonNullable<T> {
  // TODO: Implement
  return false;
}

// ---------------------------------------------------------------------------
// Exercise 3: DeepReadonly<T>
// ---------------------------------------------------------------------------
// Create a type that recursively makes every property (and nested object
// properties) readonly.

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type DeepReadonly<T> = T extends ((...args: any[]) => any) | primitive
  ? T
  : { readonly [K in keyof T]: DeepReadonly<T[K]> };

type primitive = string | number | boolean | symbol | bigint | null | undefined;

// ---------------------------------------------------------------------------
// Exercise 4: Pipe function
// ---------------------------------------------------------------------------
// Implement a pipe function that composes two functions left-to-right.
// pipe(f, g)(x) should equal g(f(x))
// Bonus: Also implement pipe3 for three functions.

export function pipe<A, B, C>(
  f: (a: A) => B,
  g: (b: B) => C,
): (a: A) => C {
  // TODO: Implement
  return (_a: A) => undefined as unknown as C;
}

export function pipe3<A, B, C, D>(
  f: (a: A) => B,
  g: (b: B) => C,
  h: (c: C) => D,
): (a: A) => D {
  // TODO: Implement
  return (_a: A) => undefined as unknown as D;
}

// ---------------------------------------------------------------------------
// Exercise 5: Type-safe Builder Pattern
// ---------------------------------------------------------------------------
// The Builder collects property values one at a time via set(key, value)
// and returns the finished object via build().

export class Builder<T extends Record<string, unknown>> {
  private data: Partial<T> = {};

  set<K extends keyof T>(_key: K, _value: T[K]): this {
    // TODO: Implement — store the value and return `this` for chaining
    return this;
  }

  build(): T {
    // TODO: Implement — return the accumulated object as T
    return {} as T;
  }
}
