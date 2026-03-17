// =============================================================================
// 05 - Utility Types
// =============================================================================
// TypeScript ships with many built-in utility types that transform existing
// types. Mastering them will save you from writing repetitive type definitions.

// Base types used throughout this file:

interface User {
  id: number;
  name: string;
  email: string;
  age: number;
  isAdmin: boolean;
}

interface Todo {
  title: string;
  description: string;
  completed: boolean;
  priority: "low" | "medium" | "high";
}

// -----------------------------------------------------------------------------
// Partial<T> — Makes all properties optional
// -----------------------------------------------------------------------------
// Useful for update/patch operations where you only send changed fields.

function updateUser(user: User, updates: Partial<User>): User {
  return { ...user, ...updates };
}

const user: User = { id: 1, name: "Alice", email: "a@b.com", age: 30, isAdmin: false };
const updated = updateUser(user, { name: "Alicia", age: 31 });
console.log("Partial — updated:", updated);

// -----------------------------------------------------------------------------
// Required<T> — Makes all properties required
// -----------------------------------------------------------------------------
// The opposite of Partial. Useful when you need to ensure every field is set.

type DraftTodo = Partial<Todo>;

function publishTodo(draft: DraftTodo): Required<Todo> {
  return {
    title: draft.title ?? "Untitled",
    description: draft.description ?? "",
    completed: draft.completed ?? false,
    priority: draft.priority ?? "medium",
  };
}

const published = publishTodo({ title: "Learn utility types" });
console.log("Required — published:", published);

// -----------------------------------------------------------------------------
// Pick<T, K> — Select a subset of properties
// -----------------------------------------------------------------------------

type UserSummary = Pick<User, "id" | "name">;

const summary: UserSummary = { id: 1, name: "Alice" };
console.log("Pick — summary:", summary);

// -----------------------------------------------------------------------------
// Omit<T, K> — Remove specific properties
// -----------------------------------------------------------------------------

type CreateUserInput = Omit<User, "id">;

const newUser: CreateUserInput = {
  name: "Bob",
  email: "bob@example.com",
  age: 25,
  isAdmin: false,
};
console.log("Omit — newUser:", newUser);

// -----------------------------------------------------------------------------
// Record<K, V> — Construct an object type with keys K and values V
// -----------------------------------------------------------------------------

type Role = "admin" | "editor" | "viewer";

const roleDescriptions: Record<Role, string> = {
  admin: "Full access to all resources",
  editor: "Can create and edit content",
  viewer: "Read-only access",
};

console.log("Record — roles:", roleDescriptions);

// Dynamic key mapping:
const todosByStatus: Record<string, Todo[]> = {
  active: [{ title: "Code", description: "Write code", completed: false, priority: "high" }],
  done: [{ title: "Plan", description: "Make plan", completed: true, priority: "low" }],
};

console.log("Record — todos by status:", Object.keys(todosByStatus));

// -----------------------------------------------------------------------------
// Readonly<T> — Makes all properties readonly
// -----------------------------------------------------------------------------

const frozenUser: Readonly<User> = {
  id: 1,
  name: "Alice",
  email: "a@b.com",
  age: 30,
  isAdmin: false,
};

// frozenUser.name = "Bob"; // ❌ Error: Cannot assign to 'name' — it is read-only
console.log("Readonly — frozenUser:", frozenUser.name);

// -----------------------------------------------------------------------------
// ReturnType<T> — Extracts the return type of a function type
// -----------------------------------------------------------------------------

function createResponse(status: number, body: string) {
  return { status, body, timestamp: Date.now() };
}

type Response = ReturnType<typeof createResponse>;
// { status: number; body: string; timestamp: number }

const res: Response = { status: 200, body: "OK", timestamp: Date.now() };
console.log("ReturnType — response:", res);

// -----------------------------------------------------------------------------
// Parameters<T> — Extracts parameter types as a tuple
// -----------------------------------------------------------------------------

function search(query: string, limit: number, offset: number): string[] {
  return [`Results for "${query}" (limit=${limit}, offset=${offset})`];
}

type SearchParams = Parameters<typeof search>;
// [string, number, number]

const params: SearchParams = ["typescript", 10, 0];
console.log("Parameters — search:", search(...params));

// -----------------------------------------------------------------------------
// Exclude<T, U> — Remove types from a union
// -----------------------------------------------------------------------------

type AllColors = "red" | "green" | "blue" | "yellow";
type WarmColors = Exclude<AllColors, "blue" | "green">;
// "red" | "yellow"

const warm: WarmColors = "red";
console.log("Exclude — warm color:", warm);

// -----------------------------------------------------------------------------
// Extract<T, U> — Keep only types assignable to U
// -----------------------------------------------------------------------------

type CoolColors = Extract<AllColors, "blue" | "green" | "purple">;
// "blue" | "green"

const cool: CoolColors = "blue";
console.log("Extract — cool color:", cool);

// Practical example: extract function types from a union
type Mixed = string | number | (() => void) | ((x: number) => number);
type FunctionTypes = Extract<Mixed, (...args: never[]) => unknown>;

// -----------------------------------------------------------------------------
// NonNullable<T> — Remove null and undefined from a type
// -----------------------------------------------------------------------------

type MaybeString = string | null | undefined;
type DefiniteString = NonNullable<MaybeString>;
// string

function processValue(value: MaybeString): DefiniteString {
  return value ?? "default";
}

console.log("NonNullable — process null:", processValue(null));
console.log("NonNullable — process value:", processValue("hello"));

// -----------------------------------------------------------------------------
// Combining Utility Types
// -----------------------------------------------------------------------------
// Utility types compose naturally for expressive type transformations.

// An update payload that requires an id but all other fields are optional:
type UserUpdate = Pick<User, "id"> & Partial<Omit<User, "id">>;

const userUpdate: UserUpdate = { id: 1, name: "Updated Alice" };
console.log("Combined — userUpdate:", userUpdate);

// A readonly version of a picked type:
type ReadonlyUserSummary = Readonly<Pick<User, "id" | "name" | "email">>;

const readonlySummary: ReadonlyUserSummary = { id: 1, name: "Alice", email: "a@b.com" };
// readonlySummary.name = "Bob"; // ❌ Error
console.log("Combined — readonlySummary:", readonlySummary);

console.log("\n✅ 05-utility-types complete!");
