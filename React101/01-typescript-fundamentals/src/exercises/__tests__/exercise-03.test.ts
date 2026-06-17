import { describe, it, expect, vi } from "vitest";
import {
  TypedEventEmitter,
  isNonNullable,
  pipe,
  pipe3,
  Builder,
} from "../exercise-03-advanced";

describe("Exercise 03 — Advanced TypeScript", () => {
  // -------------------------------------------------------------------------
  // Exercise 1: TypedEventEmitter
  // -------------------------------------------------------------------------
  describe("TypedEventEmitter", () => {
    type TestEvents = {
      greet: [name: string];
      click: [x: number, y: number];
      close: [];
    };

    it("should call registered listeners on emit", () => {
      const emitter = new TypedEventEmitter<TestEvents>();
      const handler = vi.fn();
      emitter.on("greet", handler);
      emitter.emit("greet", "Alice");
      expect(handler).toHaveBeenCalledWith("Alice");
    });

    it("should support multiple listeners for the same event", () => {
      const emitter = new TypedEventEmitter<TestEvents>();
      const h1 = vi.fn();
      const h2 = vi.fn();
      emitter.on("click", h1);
      emitter.on("click", h2);
      emitter.emit("click", 10, 20);
      expect(h1).toHaveBeenCalledWith(10, 20);
      expect(h2).toHaveBeenCalledWith(10, 20);
    });

    it("should remove a listener with off", () => {
      const emitter = new TypedEventEmitter<TestEvents>();
      const handler = vi.fn();
      emitter.on("greet", handler);
      emitter.off("greet", handler);
      emitter.emit("greet", "Bob");
      expect(handler).not.toHaveBeenCalled();
    });

    it("should handle events with no arguments", () => {
      const emitter = new TypedEventEmitter<TestEvents>();
      const handler = vi.fn();
      emitter.on("close", handler);
      emitter.emit("close");
      expect(handler).toHaveBeenCalledTimes(1);
    });
  });

  // -------------------------------------------------------------------------
  // Exercise 2: isNonNullable
  // -------------------------------------------------------------------------
  describe("isNonNullable", () => {
    it("should return false for null", () => {
      expect(isNonNullable(null)).toBe(false);
    });

    it("should return false for undefined", () => {
      expect(isNonNullable(undefined)).toBe(false);
    });

    it("should return true for a number", () => {
      expect(isNonNullable(42)).toBe(true);
    });

    it("should return true for an empty string", () => {
      expect(isNonNullable("")).toBe(true);
    });

    it("should return true for zero", () => {
      expect(isNonNullable(0)).toBe(true);
    });

    it("should return true for false", () => {
      expect(isNonNullable(false)).toBe(true);
    });

    it("can be used to filter an array", () => {
      const values: (string | null | undefined)[] = ["a", null, "b", undefined, "c"];
      const filtered: string[] = values.filter(isNonNullable);
      expect(filtered).toEqual(["a", "b", "c"]);
    });
  });

  // -------------------------------------------------------------------------
  // Exercise 4: pipe / pipe3
  // -------------------------------------------------------------------------
  describe("pipe", () => {
    it("should compose two functions left-to-right", () => {
      const double = (n: number) => n * 2;
      const addOne = (n: number) => n + 1;
      const doubleThenAdd = pipe(double, addOne);
      expect(doubleThenAdd(5)).toBe(11); // 5*2 + 1
    });

    it("should work with different types", () => {
      const toString = (n: number) => String(n);
      const exclaim = (s: string) => s + "!";
      const toExclaimed = pipe(toString, exclaim);
      expect(toExclaimed(42)).toBe("42!");
    });
  });

  describe("pipe3", () => {
    it("should compose three functions left-to-right", () => {
      const double = (n: number) => n * 2;
      const addOne = (n: number) => n + 1;
      const toString = (n: number) => `Result: ${n}`;
      const composed = pipe3(double, addOne, toString);
      expect(composed(5)).toBe("Result: 11");
    });
  });

  // -------------------------------------------------------------------------
  // Exercise 5: Builder
  // -------------------------------------------------------------------------
  describe("Builder", () => {
    interface Config {
      host: string;
      port: number;
      debug: boolean;
    }

    it("should build an object from set calls", () => {
      const config = new Builder<Config>()
        .set("host", "localhost")
        .set("port", 3000)
        .set("debug", true)
        .build();

      expect(config).toEqual({ host: "localhost", port: 3000, debug: true });
    });

    it("should allow overwriting values", () => {
      const config = new Builder<Config>()
        .set("host", "example.com")
        .set("port", 80)
        .set("debug", false)
        .set("port", 443) // overwrite
        .build();

      expect(config.port).toBe(443);
    });

    it("should return a chainable builder", () => {
      const builder = new Builder<Config>();
      const returned = builder.set("host", "localhost");
      expect(returned).toBe(builder); // same reference
    });
  });
});
