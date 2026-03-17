// =============================================================================
// 04 - Generics
// =============================================================================
// Generics are one of TypeScript's most powerful features. They allow you to
// write reusable, type-safe code that works with many different types.

// -----------------------------------------------------------------------------
// Generic Functions
// -----------------------------------------------------------------------------

function firstElement<T>(arr: T[]): T | undefined {
  return arr[0];
}

console.log("firstElement([1,2,3]):", firstElement([1, 2, 3]));
console.log('firstElement(["a","b"]):', firstElement(["a", "b"]));
console.log("firstElement([]):", firstElement([]));

function map<T, U>(arr: T[], fn: (item: T) => U): U[] {
  return arr.map(fn);
}

console.log(
  "map([1,2,3], n => n*2):",
  map([1, 2, 3], (n) => n * 2),
);

// -----------------------------------------------------------------------------
// Generic Interfaces
// -----------------------------------------------------------------------------
// Interfaces can be parameterized with type variables.

interface ApiResponse<T> {
  data: T;
  status: number;
  message: string;
}

const userResponse: ApiResponse<{ name: string }> = {
  data: { name: "Alice" },
  status: 200,
  message: "OK",
};

console.log("ApiResponse:", userResponse);

interface Repository<T> {
  getById(id: number): T | undefined;
  getAll(): T[];
  save(item: T): void;
}

// Example in-memory implementation
class InMemoryRepo<T extends { id: number }> implements Repository<T> {
  private items: T[] = [];

  getById(id: number): T | undefined {
    return this.items.find((item) => item.id === id);
  }

  getAll(): T[] {
    return [...this.items];
  }

  save(item: T): void {
    const index = this.items.findIndex((i) => i.id === item.id);
    if (index >= 0) {
      this.items[index] = item;
    } else {
      this.items.push(item);
    }
  }
}

interface Todo {
  id: number;
  title: string;
  done: boolean;
}

const todoRepo = new InMemoryRepo<Todo>();
todoRepo.save({ id: 1, title: "Learn generics", done: false });
todoRepo.save({ id: 2, title: "Build something", done: false });

console.log("All todos:", todoRepo.getAll());
console.log("Todo #1:", todoRepo.getById(1));

// -----------------------------------------------------------------------------
// Generic Constraints
// -----------------------------------------------------------------------------
// Use `extends` to constrain what types are acceptable.

interface HasLength {
  length: number;
}

function logLength<T extends HasLength>(value: T): T {
  console.log(`Length: ${value.length}`);
  return value;
}

logLength("hello");       // string has .length
logLength([1, 2, 3]);     // array has .length
logLength({ length: 42 }); // object with .length property

// Constrain to object keys:
function pick<T, K extends keyof T>(obj: T, keys: K[]): Pick<T, K> {
  const result = {} as Pick<T, K>;
  for (const key of keys) {
    result[key] = obj[key];
  }
  return result;
}

const fullUser = { id: 1, name: "Alice", email: "alice@example.com", age: 30 };
const summary = pick(fullUser, ["name", "email"]);
console.log("Picked:", summary);

// -----------------------------------------------------------------------------
// Multiple Type Parameters
// -----------------------------------------------------------------------------

function zip<A, B>(as: A[], bs: B[]): [A, B][] {
  const length = Math.min(as.length, bs.length);
  const result: [A, B][] = [];
  for (let i = 0; i < length; i++) {
    result.push([as[i], bs[i]]);
  }
  return result;
}

console.log("zip:", zip(["a", "b", "c"], [1, 2, 3]));

function mapRecord<K extends string, V, U>(
  record: Record<K, V>,
  fn: (value: V, key: K) => U,
): Record<K, U> {
  const result = {} as Record<K, U>;
  for (const key of Object.keys(record) as K[]) {
    result[key] = fn(record[key], key);
  }
  return result;
}

const prices = { apple: 1.5, banana: 0.75, cherry: 3.0 };
const formatted = mapRecord(prices, (v) => `$${v.toFixed(2)}`);
console.log("mapRecord:", formatted);

// -----------------------------------------------------------------------------
// Generic Utility: Result Type (Ok/Err Pattern)
// -----------------------------------------------------------------------------
// A type-safe alternative to exceptions for expected failure cases.

type Result<T, E = Error> =
  | { ok: true; value: T }
  | { ok: false; error: E };

function ok<T>(value: T): Result<T, never> {
  return { ok: true, value };
}

function err<E>(error: E): Result<never, E> {
  return { ok: false, error };
}

function divide(a: number, b: number): Result<number, string> {
  if (b === 0) {
    return err("Division by zero");
  }
  return ok(a / b);
}

const result1 = divide(10, 3);
const result2 = divide(10, 0);

if (result1.ok) {
  console.log("10 / 3 =", result1.value);
} else {
  console.log("Error:", result1.error);
}

if (result2.ok) {
  console.log("10 / 0 =", result2.value);
} else {
  console.log("Error:", result2.error);
}

// -----------------------------------------------------------------------------
// keyof and Indexed Access Types
// -----------------------------------------------------------------------------
// `keyof T` produces a union of T's property names.
// `T[K]` accesses the type of property K on T.

interface Product {
  id: number;
  name: string;
  price: number;
  inStock: boolean;
}

type ProductKey = keyof Product; // "id" | "name" | "price" | "inStock"

function getField<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}

const laptop: Product = { id: 1, name: "Laptop", price: 999, inStock: true };

const productName: string = getField(laptop, "name");
const productPrice: number = getField(laptop, "price");

console.log(`Product: ${productName}, Price: ${productPrice}`);

// Indexed access can chain:
type ProductName = Product["name"]; // string
type IdOrName = Product["id" | "name"]; // number | string

console.log("\n✅ 04-generics complete!");
