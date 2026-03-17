import { describe, it, expect } from "vitest";
import {
  type Product,
  type CartItem,
  calculateTotal,
  findProduct,
  groupByTag,
} from "../exercise-01-types-and-interfaces";

// ---------------------------------------------------------------------------
// Sample data
// ---------------------------------------------------------------------------

const products: Product[] = [
  { id: 1, name: "Laptop", price: 999, tags: ["electronics", "computers"] },
  { id: 2, name: "Phone", price: 699, description: "Smartphone", tags: ["electronics", "mobile"] },
  { id: 3, name: "Shirt", price: 29, tags: ["clothing"] },
  { id: 4, name: "Widget", price: 5, tags: [] },
];

const cart: CartItem[] = [
  { ...products[0], quantity: 1 },
  { ...products[2], quantity: 3 },
];

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("Exercise 01 — Types and Interfaces", () => {
  describe("Product interface", () => {
    it("should have the required properties", () => {
      const p: Product = {
        id: 10,
        name: "Test",
        price: 42,
        tags: ["a"],
      };
      expect(p.id).toBe(10);
      expect(p.name).toBe("Test");
      expect(p.price).toBe(42);
      expect(p.tags).toEqual(["a"]);
    });

    it("should allow an optional description", () => {
      const p: Product = {
        id: 1,
        name: "Test",
        price: 1,
        description: "hi",
        tags: [],
      };
      expect(p.description).toBe("hi");
    });
  });

  describe("CartItem type", () => {
    it("should include Product properties plus quantity", () => {
      const item: CartItem = {
        id: 1,
        name: "Test",
        price: 10,
        tags: [],
        quantity: 2,
      };
      expect(item.quantity).toBe(2);
      expect(item.price).toBe(10);
    });
  });

  describe("calculateTotal", () => {
    it("should return 0 for an empty cart", () => {
      expect(calculateTotal([])).toBe(0);
    });

    it("should correctly sum price × quantity", () => {
      expect(calculateTotal(cart)).toBe(999 * 1 + 29 * 3);
    });

    it("should handle a single item", () => {
      expect(calculateTotal([cart[0]])).toBe(999);
    });
  });

  describe("findProduct", () => {
    it("should return the product when found", () => {
      expect(findProduct(products, 2)).toEqual(products[1]);
    });

    it("should return undefined when not found", () => {
      expect(findProduct(products, 999)).toBeUndefined();
    });
  });

  describe("groupByTag", () => {
    it("should group products by their first tag", () => {
      const grouped = groupByTag(products);
      expect(grouped["electronics"]).toEqual([products[0], products[1]]);
      expect(grouped["clothing"]).toEqual([products[2]]);
    });

    it("should put tagless products under 'untagged'", () => {
      const grouped = groupByTag(products);
      expect(grouped["untagged"]).toEqual([products[3]]);
    });

    it("should return an empty object for an empty array", () => {
      expect(groupByTag([])).toEqual({});
    });
  });
});
