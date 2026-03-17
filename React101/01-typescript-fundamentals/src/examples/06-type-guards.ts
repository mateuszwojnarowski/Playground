// =============================================================================
// 06 - Type Guards
// =============================================================================
// Type guards let you narrow a value's type at runtime so TypeScript can
// provide accurate type information inside conditional branches.

// -----------------------------------------------------------------------------
// typeof Guards
// -----------------------------------------------------------------------------
// `typeof` works for primitive types: string, number, boolean, symbol, etc.

function padLeft(value: string, padding: string | number): string {
  if (typeof padding === "number") {
    // TypeScript knows `padding` is a number here
    return " ".repeat(padding) + value;
  }
  // TypeScript knows `padding` is a string here
  return padding + value;
}

console.log('padLeft("hello", 4):', padLeft("hello", 4));
console.log('padLeft("hello", ">>> "):', padLeft("hello", ">>> "));

// -----------------------------------------------------------------------------
// instanceof Guards
// -----------------------------------------------------------------------------
// `instanceof` works for class instances.

class ValidationError {
  constructor(
    public field: string,
    public message: string,
  ) {}
}

class NetworkError {
  constructor(
    public statusCode: number,
    public url: string,
  ) {}
}

type AppError = ValidationError | NetworkError;

function handleError(error: AppError): string {
  if (error instanceof ValidationError) {
    // TypeScript knows `error` is ValidationError
    return `Validation error on "${error.field}": ${error.message}`;
  }
  // TypeScript knows `error` is NetworkError
  return `Network error ${error.statusCode} for ${error.url}`;
}

console.log(handleError(new ValidationError("email", "Invalid format")));
console.log(handleError(new NetworkError(404, "/api/users")));

// -----------------------------------------------------------------------------
// `in` Operator Guards
// -----------------------------------------------------------------------------
// The `in` operator checks if a property exists on an object.

interface Fish {
  swim: () => void;
}

interface Bird {
  fly: () => void;
}

function move(animal: Fish | Bird): string {
  if ("swim" in animal) {
    animal.swim();
    return "Swimming!";
  }
  animal.fly();
  return "Flying!";
}

console.log(
  "Fish:",
  move({ swim: () => console.log("  🐟 splash") }),
);
console.log(
  "Bird:",
  move({ fly: () => console.log("  🐦 flap") }),
);

// -----------------------------------------------------------------------------
// Custom Type Guard Functions (the `is` keyword)
// -----------------------------------------------------------------------------
// A type predicate `param is Type` tells TypeScript the return value narrows.

interface Cat {
  kind: "cat";
  meow: () => string;
}

interface Dog {
  kind: "dog";
  bark: () => string;
}

type Pet = Cat | Dog;

function isCat(pet: Pet): pet is Cat {
  return pet.kind === "cat";
}

function isDog(pet: Pet): pet is Dog {
  return pet.kind === "dog";
}

function petSound(pet: Pet): string {
  if (isCat(pet)) {
    return pet.meow(); // TypeScript knows this is Cat
  }
  return pet.bark(); // TypeScript knows this is Dog
}

const kitty: Cat = { kind: "cat", meow: () => "Meow!" };
const doggo: Dog = { kind: "dog", bark: () => "Woof!" };

console.log("Cat says:", petSound(kitty));
console.log("Dog says:", petSound(doggo));

// A practical guard for filtering nulls:
function isNotNull<T>(value: T | null | undefined): value is T {
  return value != null;
}

const mixedArray = [1, null, 2, undefined, 3, null, 4];
const cleanArray = mixedArray.filter(isNotNull);
console.log("Filtered nulls:", cleanArray); // [1, 2, 3, 4]

// -----------------------------------------------------------------------------
// Discriminated Unions
// -----------------------------------------------------------------------------
// A pattern where each member of a union has a common literal property (the
// "discriminant") that TypeScript uses to narrow automatically.

interface Circle {
  kind: "circle";
  radius: number;
}

interface Rectangle {
  kind: "rectangle";
  width: number;
  height: number;
}

interface Triangle {
  kind: "triangle";
  base: number;
  height: number;
}

type Shape = Circle | Rectangle | Triangle;

function area(shape: Shape): number {
  switch (shape.kind) {
    case "circle":
      return Math.PI * shape.radius ** 2;
    case "rectangle":
      return shape.width * shape.height;
    case "triangle":
      return (shape.base * shape.height) / 2;
  }
}

console.log("Circle area:", area({ kind: "circle", radius: 5 }).toFixed(2));
console.log("Rectangle area:", area({ kind: "rectangle", width: 4, height: 6 }));
console.log("Triangle area:", area({ kind: "triangle", base: 3, height: 8 }));

// -----------------------------------------------------------------------------
// Exhaustive Checking with `never`
// -----------------------------------------------------------------------------
// Use `never` to ensure all cases in a union are handled. If you add a new
// variant to the union, the compiler will error at the `assertNever` call.

function assertNever(value: never): never {
  throw new Error(`Unexpected value: ${value}`);
}

function describeShape(shape: Shape): string {
  switch (shape.kind) {
    case "circle":
      return `A circle with radius ${shape.radius}`;
    case "rectangle":
      return `A ${shape.width}x${shape.height} rectangle`;
    case "triangle":
      return `A triangle with base ${shape.base} and height ${shape.height}`;
    default:
      // If Shape gets a new variant, TypeScript will error here
      return assertNever(shape);
  }
}

console.log(describeShape({ kind: "circle", radius: 10 }));
console.log(describeShape({ kind: "rectangle", width: 5, height: 3 }));
console.log(describeShape({ kind: "triangle", base: 6, height: 4 }));

console.log("\n✅ 06-type-guards complete!");
