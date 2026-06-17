# Section 01 – TypeScript Fundamentals

A hands-on introduction to TypeScript's type system. Work through the examples
to learn each concept, then complete the exercises to test your understanding.

---

## What You'll Learn

- Primitive types, arrays, tuples, and enums
- Union types, literal types, and type assertions
- Interfaces and type aliases (and when to use each)
- Functions — optional/default/rest parameters, overloads, and generics
- Generics — constraints, multiple type parameters, utility patterns
- Built-in utility types (`Partial`, `Pick`, `Omit`, `Record`, and more)
- Type guards and discriminated unions

---

## Getting Started

```bash
# Install dependencies
npm install

# Run a specific example
npx tsx src/examples/01-basic-types.ts

# Run all tests (exercises)
npm test

# Run tests in watch mode while you work
npm run test:watch
```

---

## Topics

### 1. Basic Types (`src/examples/01-basic-types.ts`)

Covers the building blocks of every TypeScript program: primitives, arrays,
tuples, enums, union/literal types, and the special types `any`, `unknown`,
and `never`.

```ts
// Union type + literal type
type Theme = "light" | "dark" | "system";

// unknown forces you to narrow before use
const value: unknown = "hello";
if (typeof value === "string") {
  console.log(value.toUpperCase());
}
```

### 2. Interfaces & Type Aliases (`src/examples/02-interfaces-and-types.ts`)

Learn two ways to describe object shapes. Interfaces support declaration
merging; type aliases support unions and intersections.

```ts
interface User {
  readonly id: number;
  name: string;
  email?: string; // optional
}

// Intersection combines multiple types
type AdminUser = User & { permissions: string[] };
```

### 3. Functions (`src/examples/03-functions.ts`)

Type-safe functions with optional/default/rest parameters, overloads, and
generics.

```ts
function getProperty<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}

type MathOp = (a: number, b: number) => number;
const add: MathOp = (a, b) => a + b;
```

### 4. Generics (`src/examples/04-generics.ts`)

Write reusable, type-safe code with generic functions, interfaces, constraints,
and the Result (Ok/Err) pattern.

```ts
// Generic constraint
function logLength<T extends { length: number }>(value: T): T {
  console.log(value.length);
  return value;
}

// Result type for safe error handling
type Result<T, E = Error> =
  | { ok: true; value: T }
  | { ok: false; error: E };
```

### 5. Utility Types (`src/examples/05-utility-types.ts`)

Master the built-in type transformations that eliminate boilerplate.

```ts
// Partial for update payloads
function updateUser(user: User, updates: Partial<User>): User {
  return { ...user, ...updates };
}

// Compose utilities for precise types
type UserUpdate = Pick<User, "id"> & Partial<Omit<User, "id">>;
```

### 6. Type Guards (`src/examples/06-type-guards.ts`)

Narrow types at runtime with `typeof`, `instanceof`, the `in` operator, custom
type predicates, discriminated unions, and exhaustive checking.

```ts
// Custom type guard
function isCat(pet: Pet): pet is Cat {
  return pet.kind === "cat";
}

// Exhaustive check with never
function assertNever(value: never): never {
  throw new Error(`Unexpected value: ${value}`);
}
```

---

## Exercises

Each exercise file lives in `src/exercises/` and has a matching test in
`src/exercises/__tests__/`. The skeleton code compiles but the tests **fail**
until you fill in the TODOs.

| File | What to implement |
| ---- | ----------------- |
| `exercise-01-types-and-interfaces.ts` | `Product` interface, `CartItem` type, `calculateTotal`, `findProduct`, `groupByTag` |
| `exercise-02-generics.ts` | `identity`, `first`, `merge`, `Stack<T>`, `toLookup` |
| `exercise-03-advanced.ts` | `TypedEventEmitter`, `isNonNullable`, `DeepReadonly`, `pipe` / `pipe3`, `Builder` |

Run the tests to check your progress:

```bash
npm test                # run once
npm run test:watch      # re-run on save
```

---

## Tips

- **Start with the examples.** Read each one top-to-bottom and run it with
  `npx tsx` before attempting the exercises.
- **Let the compiler help you.** If you're stuck, hover over a variable in your
  editor to see its inferred type.
- **`unknown` over `any`.** Prefer `unknown` when you don't know the type —
  it forces you to narrow safely.
- **Use `as const`** to get literal types from arrays and objects.
- **Interfaces vs types:** Use interfaces for object shapes you expect others
  to extend; use type aliases for unions, intersections, and mapped types.
- **Read the error messages.** TypeScript errors are verbose but precise — the
  last line usually tells you exactly what's wrong.
