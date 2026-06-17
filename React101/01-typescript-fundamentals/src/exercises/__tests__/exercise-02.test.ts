import { describe, it, expect } from "vitest";
import {
  identity,
  first,
  merge,
  Stack,
  toLookup,
} from "../exercise-02-generics";

describe("Exercise 02 — Generics", () => {
  describe("identity", () => {
    it("should return the same number", () => {
      expect(identity(42)).toBe(42);
    });

    it("should return the same string", () => {
      expect(identity("hello")).toBe("hello");
    });

    it("should return the same object reference", () => {
      const obj = { a: 1 };
      expect(identity(obj)).toBe(obj);
    });
  });

  describe("first", () => {
    it("should return the first element", () => {
      expect(first([10, 20, 30])).toBe(10);
    });

    it("should return undefined for an empty array", () => {
      expect(first([])).toBeUndefined();
    });

    it("should work with strings", () => {
      expect(first(["a", "b"])).toBe("a");
    });
  });

  describe("merge", () => {
    it("should merge two objects", () => {
      const result = merge({ a: 1 }, { b: 2 });
      expect(result).toEqual({ a: 1, b: 2 });
    });

    it("should let the second object override the first", () => {
      const result = merge({ a: 1, b: 2 }, { b: 99 });
      expect(result).toEqual({ a: 1, b: 99 });
    });

    it("should work with different shapes", () => {
      const result = merge({ name: "Alice" }, { age: 30, active: true });
      expect(result).toEqual({ name: "Alice", age: 30, active: true });
    });
  });

  describe("Stack", () => {
    it("should start empty", () => {
      const stack = new Stack<number>();
      expect(stack.isEmpty()).toBe(true);
      expect(stack.size()).toBe(0);
    });

    it("should push and pop items (LIFO order)", () => {
      const stack = new Stack<string>();
      stack.push("a");
      stack.push("b");
      stack.push("c");
      expect(stack.size()).toBe(3);
      expect(stack.pop()).toBe("c");
      expect(stack.pop()).toBe("b");
      expect(stack.pop()).toBe("a");
      expect(stack.isEmpty()).toBe(true);
    });

    it("should peek without removing", () => {
      const stack = new Stack<number>();
      stack.push(1);
      stack.push(2);
      expect(stack.peek()).toBe(2);
      expect(stack.size()).toBe(2); // unchanged
    });

    it("should return undefined when popping/peeking an empty stack", () => {
      const stack = new Stack<number>();
      expect(stack.pop()).toBeUndefined();
      expect(stack.peek()).toBeUndefined();
    });
  });

  describe("toLookup", () => {
    const users = [
      { id: 1, name: "Alice" },
      { id: 2, name: "Bob" },
      { id: 3, name: "Charlie" },
    ];

    it("should create a Map keyed by the given property", () => {
      const lookup = toLookup(users, "id");
      expect(lookup.get(1)).toEqual({ id: 1, name: "Alice" });
      expect(lookup.get(2)).toEqual({ id: 2, name: "Bob" });
      expect(lookup.size).toBe(3);
    });

    it("should handle string keys", () => {
      const lookup = toLookup(users, "name");
      expect(lookup.get("Alice")).toEqual({ id: 1, name: "Alice" });
    });

    it("should overwrite on duplicate keys", () => {
      const items = [
        { id: 1, value: "first" },
        { id: 1, value: "second" },
      ];
      const lookup = toLookup(items, "id");
      expect(lookup.get(1)).toEqual({ id: 1, value: "second" });
      expect(lookup.size).toBe(1);
    });

    it("should return an empty Map for an empty array", () => {
      const lookup = toLookup([], "id" as never);
      expect(lookup.size).toBe(0);
    });
  });
});
