// =============================================================================
// 01 - Basic Types in TypeScript
// =============================================================================
// TypeScript adds a static type system on top of JavaScript. This file
// demonstrates the fundamental types you'll use every day.

// -----------------------------------------------------------------------------
// Primitive Types
// -----------------------------------------------------------------------------
// The three most common primitives: string, number, and boolean.

const greeting: string = "Hello, TypeScript!";
const year: number = 2024;
const isAwesome: boolean = true;

console.log(`${greeting} Year: ${year}, Awesome: ${isAwesome}`);

// -----------------------------------------------------------------------------
// Arrays
// -----------------------------------------------------------------------------
// Two equivalent syntaxes for typed arrays.

const fibonacci: number[] = [1, 1, 2, 3, 5, 8, 13];
const languages: Array<string> = ["TypeScript", "JavaScript", "Rust"];

console.log("Fibonacci:", fibonacci);
console.log("Languages:", languages);

// -----------------------------------------------------------------------------
// Tuples
// -----------------------------------------------------------------------------
// Tuples are fixed-length arrays where each position has a specific type.

const coordinate: [number, number] = [40.7128, -74.006];
const nameAge: [string, number] = ["Alice", 30];

console.log(`Coordinate: ${coordinate[0]}, ${coordinate[1]}`);
console.log(`${nameAge[0]} is ${nameAge[1]} years old`);

// -----------------------------------------------------------------------------
// Enums
// -----------------------------------------------------------------------------
// Enums define a set of named constants. Numeric enums auto-increment.

enum Direction {
  Up = 0,
  Down = 1,
  Left = 2,
  Right = 3,
}

// String enums require explicit values for every member.
enum LogLevel {
  Debug = "DEBUG",
  Info = "INFO",
  Warn = "WARN",
  Error = "ERROR",
}

console.log("Direction.Up:", Direction.Up);
console.log("LogLevel.Error:", LogLevel.Error);

// -----------------------------------------------------------------------------
// Union Types
// -----------------------------------------------------------------------------
// A union type allows a value to be one of several types.

type StringOrNumber = string | number;

function formatId(id: StringOrNumber): string {
  if (typeof id === "string") {
    return id.toUpperCase();
  }
  return `#${id.toString().padStart(4, "0")}`;
}

console.log("String ID:", formatId("abc"));
console.log("Number ID:", formatId(42));

// -----------------------------------------------------------------------------
// Literal Types
// -----------------------------------------------------------------------------
// Literal types narrow a type to specific values.

type Theme = "light" | "dark" | "system";
type DiceRoll = 1 | 2 | 3 | 4 | 5 | 6;

const currentTheme: Theme = "dark";
const roll: DiceRoll = 4;

console.log(`Theme: ${currentTheme}, Dice roll: ${roll}`);

// -----------------------------------------------------------------------------
// any, unknown, and never
// -----------------------------------------------------------------------------

// `any` disables type checking — avoid when possible.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const anything: any = "could be anything";
console.log("any:", anything.toUpperCase()); // No compile error, even if wrong

// `unknown` is the type-safe counterpart of `any`. You must narrow before use.
const mysterious: unknown = "surprise!";
if (typeof mysterious === "string") {
  console.log("unknown (narrowed):", mysterious.toUpperCase());
}

// `never` represents values that never occur (e.g., a function that always throws).
function fail(message: string): never {
  throw new Error(message);
}

try {
  fail("Something went wrong");
} catch (e) {
  console.log("never: caught error —", (e as Error).message);
}

// -----------------------------------------------------------------------------
// Type Assertions
// -----------------------------------------------------------------------------
// Type assertions tell the compiler to treat a value as a specific type.
// Use them sparingly — they bypass the type checker.

const rawValue: unknown = "hello world";

// "as" syntax (preferred)
const length1 = (rawValue as string).length;

// Angle-bracket syntax (not available in .tsx files)
const length2 = (<string>rawValue).length;

console.log(`Assertion lengths: ${length1}, ${length2}`);

// A practical use: narrowing DOM types
// const input = document.getElementById("my-input") as HTMLInputElement;
// input.value = "typed!";

console.log("\n✅ 01-basic-types complete!");
