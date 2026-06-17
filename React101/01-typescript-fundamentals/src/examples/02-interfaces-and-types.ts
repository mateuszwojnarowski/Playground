// =============================================================================
// 02 - Interfaces and Type Aliases
// =============================================================================
// TypeScript provides two main ways to define object shapes: interfaces and
// type aliases. Both are powerful — this file shows when to use each.

// -----------------------------------------------------------------------------
// Type Aliases
// -----------------------------------------------------------------------------
// Type aliases create a new name for any type, including primitives and unions.

type UserID = string | number;
type Coordinates = [number, number];
type Callback = (data: string) => void;

const userId: UserID = "user-42";
const point: Coordinates = [10, 20];
const log: Callback = (data) => console.log("Callback:", data);

console.log("UserID:", userId);
console.log("Coordinates:", point);
log("hello from callback");

// -----------------------------------------------------------------------------
// Interfaces
// -----------------------------------------------------------------------------
// Interfaces describe the shape of objects. They are ideal for defining
// contracts that classes or objects must satisfy.

interface User {
  id: number;
  name: string;
  email: string;
}

const alice: User = { id: 1, name: "Alice", email: "alice@example.com" };
console.log("User:", alice);

// -----------------------------------------------------------------------------
// Optional and Readonly Properties
// -----------------------------------------------------------------------------

interface BlogPost {
  readonly id: number; // Cannot be changed after creation
  title: string;
  content: string;
  publishedAt?: Date; // Optional — may be undefined
}

const draft: BlogPost = {
  id: 1,
  title: "Learning TypeScript",
  content: "TypeScript is great!",
  // publishedAt is omitted — that's fine, it's optional
};

console.log("Draft post:", draft.title);
// draft.id = 2; // ❌ Error: Cannot assign to 'id' because it is a read-only property

// -----------------------------------------------------------------------------
// Extending Interfaces
// -----------------------------------------------------------------------------
// Interfaces can extend one or more other interfaces.

interface Timestamped {
  createdAt: Date;
  updatedAt: Date;
}

interface SoftDeletable {
  deletedAt?: Date;
}

interface Article extends Timestamped, SoftDeletable {
  id: number;
  title: string;
  body: string;
}

const article: Article = {
  id: 1,
  title: "Extending Interfaces",
  body: "Interfaces compose beautifully.",
  createdAt: new Date(),
  updatedAt: new Date(),
};

console.log("Article:", article.title, "created:", article.createdAt.toISOString());

// -----------------------------------------------------------------------------
// Intersection Types
// -----------------------------------------------------------------------------
// Intersection types combine multiple types into one using `&`.

type WithID = { id: number };
type WithName = { name: string };
type WithEmail = { email: string };

type Contact = WithID & WithName & WithEmail;

const contact: Contact = { id: 1, name: "Bob", email: "bob@example.com" };
console.log("Contact:", contact);

// Intersections work with interfaces too:
type AdminUser = User & { permissions: string[] };

const admin: AdminUser = {
  id: 99,
  name: "SuperAdmin",
  email: "admin@example.com",
  permissions: ["read", "write", "delete"],
};

console.log("Admin:", admin.name, "permissions:", admin.permissions);

// -----------------------------------------------------------------------------
// Key Differences: Interface vs Type Alias
// -----------------------------------------------------------------------------

// 1) Declaration merging — interfaces with the same name merge automatically.
interface Config {
  debug: boolean;
}

interface Config {
  version: string;
}

// Config now has both `debug` and `version`:
const config: Config = { debug: true, version: "1.0.0" };
console.log("Merged Config:", config);

// Type aliases CANNOT be merged — redeclaring causes a compile error:
// type Settings = { debug: boolean };
// type Settings = { version: string }; // ❌ Duplicate identifier

// 2) Union types — only type aliases can express unions:
type Result = "success" | "error";
// interface Result2 = "success" | "error"; // ❌ Not valid syntax

const outcome: Result = "success";
console.log("Result:", outcome);

// Rule of thumb:
// • Use `interface` for object shapes (especially in public APIs).
// • Use `type` when you need unions, intersections, or mapped types.

console.log("\n✅ 02-interfaces-and-types complete!");
