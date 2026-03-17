// =============================================================================
// 03 - Functions in TypeScript
// =============================================================================
// Functions are first-class citizens in TypeScript. This file covers typed
// parameters, return types, overloads, generics, and more.

// -----------------------------------------------------------------------------
// Typed Parameters and Return Types
// -----------------------------------------------------------------------------
// Always annotate parameters. Return types are inferred but explicit is clearer.

function add(a: number, b: number): number {
  return a + b;
}

console.log("add(2, 3):", add(2, 3));

function greet(name: string): string {
  return `Hello, ${name}!`;
}

console.log(greet("TypeScript"));

// -----------------------------------------------------------------------------
// Optional and Default Parameters
// -----------------------------------------------------------------------------
// Optional parameters use `?` and must come after required ones.

function createUser(name: string, age?: number): string {
  return age !== undefined ? `${name}, age ${age}` : name;
}

console.log("With age:", createUser("Alice", 30));
console.log("Without age:", createUser("Bob"));

// Default parameters provide a fallback value.
function repeat(text: string, times: number = 3): string {
  return text.repeat(times);
}

console.log("repeat('ha'):", repeat("ha"));
console.log("repeat('ha', 5):", repeat("ha", 5));

// -----------------------------------------------------------------------------
// Rest Parameters
// -----------------------------------------------------------------------------
// Rest parameters collect remaining arguments into a typed array.

function sum(...numbers: number[]): number {
  return numbers.reduce((total, n) => total + n, 0);
}

console.log("sum(1,2,3,4,5):", sum(1, 2, 3, 4, 5));

function buildPath(base: string, ...segments: string[]): string {
  return [base, ...segments].join("/");
}

console.log("buildPath:", buildPath("/api", "users", "42", "posts"));

// -----------------------------------------------------------------------------
// Function Overloads
// -----------------------------------------------------------------------------
// Overloads let a function have multiple call signatures.

function format(value: string): string;
function format(value: number): string;
function format(value: string | number): string {
  if (typeof value === "string") {
    return value.trim().toLowerCase();
  }
  return value.toFixed(2);
}

console.log('format("  HELLO  "):', format("  HELLO  "));
console.log("format(3.14159):", format(3.14159));

// A more practical overload example:
function createElement(tag: "a"): HTMLAnchorElement;
function createElement(tag: "canvas"): HTMLCanvasElement;
function createElement(tag: "table"): HTMLTableElement;
function createElement(tag: string): HTMLElement;
function createElement(tag: string): HTMLElement {
  return document.createElement(tag);
}
// (We skip calling createElement here since there's no DOM in Node.)

// -----------------------------------------------------------------------------
// Generic Functions
// -----------------------------------------------------------------------------
// Generics let functions work with many types while preserving type safety.

function identity<T>(value: T): T {
  return value;
}

console.log("identity(42):", identity(42));
console.log('identity("hello"):', identity("hello"));

function wrapInArray<T>(value: T): T[] {
  return [value];
}

console.log("wrapInArray(42):", wrapInArray(42));

function getProperty<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}

const person = { name: "Alice", age: 30 };
console.log("getProperty:", getProperty(person, "name"));

// -----------------------------------------------------------------------------
// Arrow Functions with Type Annotations
// -----------------------------------------------------------------------------
// Arrow functions follow the same typing rules.

const double = (n: number): number => n * 2;
const isEven = (n: number): boolean => n % 2 === 0;
const toUpper = (s: string): string => s.toUpperCase();

console.log("double(21):", double(21));
console.log("isEven(4):", isEven(4));
console.log('toUpper("hello"):', toUpper("hello"));

// Typing a function variable separately:
type MathOp = (a: number, b: number) => number;

const multiply: MathOp = (a, b) => a * b;
const subtract: MathOp = (a, b) => a - b;

console.log("multiply(6, 7):", multiply(6, 7));
console.log("subtract(10, 4):", subtract(10, 4));

console.log("\n✅ 03-functions complete!");
