// =============================================================================
// Exercise 01 — Types and Interfaces
// =============================================================================
// Complete each TODO below. Run `npm test` to check your solutions.

// ---------------------------------------------------------------------------
// Exercise 1: Define a Product interface
// Properties: id (number), name (string), price (number),
//             description (optional string), tags (array of strings)
// ---------------------------------------------------------------------------

export interface Product {
  // TODO: Add properties
  id: number;
  name: string;
  price: number;
  description?: string;
  tags: string[];
}

// ---------------------------------------------------------------------------
// Exercise 2: Define a CartItem type
// A CartItem has everything a Product has, plus a quantity (number) property.
// ---------------------------------------------------------------------------

// TODO: Define the CartItem type
export type CartItem = Product & { quantity: number };

// ---------------------------------------------------------------------------
// Exercise 3: Calculate the total price of cart items
// Return the sum of (price × quantity) for every item.
// ---------------------------------------------------------------------------

export function calculateTotal(items: CartItem[]): number {
  // TODO: Implement
  return 0;
}

// ---------------------------------------------------------------------------
// Exercise 4: Find a product by its id
// Return the product if found, or undefined if not.
// ---------------------------------------------------------------------------

export function findProduct(
  products: Product[],
  id: number,
): Product | undefined {
  // TODO: Implement
  return undefined;
}

// ---------------------------------------------------------------------------
// Exercise 5: Group products by their first tag
// Return a Record where keys are tag names and values are arrays of products.
// Products with no tags should be grouped under the key "untagged".
// ---------------------------------------------------------------------------

export function groupByTag(products: Product[]): Record<string, Product[]> {
  // TODO: Implement
  return {};
}
