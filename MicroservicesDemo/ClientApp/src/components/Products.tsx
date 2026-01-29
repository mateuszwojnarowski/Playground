// ============================================================================
// PRODUCTS MANAGEMENT COMPONENT
// ============================================================================
// Full CRUD interface for managing products in the catalog.
// Features:
// - List all products in a table
// - Create new products
// - Update product stock quantity
// - Delete products
// - Modal-based forms for better UX
// ============================================================================

import React, { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import { Product } from '../types';

/**
 * Products Management Component
 * 
 * Provides complete product catalog management with CRUD operations.
 * Communicates with Products microservice via API Gateway.
 */
const Products: React.FC = () => {
  // ==========================================================================
  // STATE MANAGEMENT
  // ==========================================================================
  
  // Product data from API
  const [products, setProducts] = useState<Product[]>([]);
  
  // Loading and error states for better UX
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Modal visibility control
  const [showModal, setShowModal] = useState(false);
  
  // Track if we're editing (has product) or creating (null)
  // Track if we're editing (has product) or creating (null)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  
  // Form data for create/edit operations
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    cost: 0,
    stockQuantity: 0
  });

  // ==========================================================================
  // EFFECTS
  // ==========================================================================
  
  /**
   * Load products on component mount
   */
  useEffect(() => {
    loadProducts();
  }, []); // Empty deps = run once on mount

  // ==========================================================================
  // API OPERATIONS
  // ==========================================================================
  
  /**
   * Fetch all products from the API
   * Sets loading state and handles errors
   */
  const loadProducts = async () => {
    try {
      setLoading(true);
      setError(null); // Clear any previous errors
      const data = await apiService.getProducts();
      setProducts(data);
    } catch (err: any) {
      // Extract error message from response or use fallback
      setError(err.response?.data?.message || err.message || 'Failed to load products');
    } finally {
      setLoading(false); // Always stop loading indicator
    }
  };

  // ==========================================================================
  // EVENT HANDLERS
  // ==========================================================================
  
  /**
   * Open modal to create a new product
   * Clears form and editing state
   */
  const handleCreate = () => {
    setEditingProduct(null); // Not editing, creating
    setFormData({ name: '', description: '', cost: 0, stockQuantity: 0 });
    setShowModal(true);
  };

  /**
   * Open modal to edit an existing product's stock
   * 
   * Note: API only supports updating stock quantity, not other fields
   * 
   * @param product - Product to edit
   */
  const handleEdit = (product: Product) => {
    setEditingProduct(product); // Set product being edited
    // Pre-fill form with current product data
    setFormData({
      name: product.name,
      description: product.description || '',
      cost: product.cost,
      stockQuantity: product.stockQuantity
    });
    setShowModal(true);
  };

  /**
   * Handle form submission for create/update
   * 
   * @param e - Form submit event
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); // Prevent default form submission
    
    try {
      if (editingProduct) {
        // UPDATE: Only stock can be updated via API
        await apiService.updateProductStock(editingProduct.id, formData.stockQuantity);
      } else {
        // CREATE: Create new product with all fields
        await apiService.createProduct(formData);
      }
      
      // Success! Close modal and refresh list
      setShowModal(false);
      loadProducts();
    } catch (err: any) {
      // Show error but keep modal open so user can fix it
      setError(err.response?.data?.message || err.message || 'Operation failed');
    }
  };

  /**
   * Delete a product after confirmation
   * 
   * @param id - Product ID to delete
   */
  const handleDelete = async (id: string) => {
    // Confirm with user before destructive action
    if (!window.confirm('Are you sure you want to delete this product?')) return;
    
    try {
      await apiService.deleteProduct(id);
      loadProducts(); // Refresh list after deletion
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to delete product');
    }
  };

  // ==========================================================================
  // RENDER
  // ==========================================================================
  
  // Show loading indicator while fetching data
  if (loading) return <div className="loading">Loading products...</div>;

  return (
    <div className="container">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1>Products</h1>
        <button className="primary" onClick={handleCreate}>Add Product</button>
      </div>

      {error && <div className="error">{error}</div>}

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Cost</th>
            <th>Stock</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.length === 0 ? (
            <tr>
              <td colSpan={5} style={{ textAlign: 'center', padding: '2rem' }}>
                No products found. Create one to get started!
              </td>
            </tr>
          ) : (
            products.map(product => (
              <tr key={product.id}>
                <td>{product.name}</td>
                <td>{product.description || '-'}</td>
                <td>${product.cost.toFixed(2)}</td>
                <td>{product.stockQuantity}</td>
                <td>
                  <button className="secondary" onClick={() => handleEdit(product)} style={{ marginRight: '0.5rem' }}>
                    Update Stock
                  </button>
                  <button className="danger" onClick={() => handleDelete(product.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingProduct ? 'Update Stock' : 'Add Product'}</h2>
              <button onClick={() => setShowModal(false)}>×</button>
            </div>
            <form onSubmit={handleSubmit}>
              {!editingProduct && (
                <>
                  <div className="form-group">
                    <label>Name *</label>
                    <input
                      type="text"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      required
                      maxLength={250}
                    />
                  </div>
                  <div className="form-group">
                    <label>Description</label>
                    <textarea
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      maxLength={2048}
                      rows={3}
                    />
                  </div>
                  <div className="form-group">
                    <label>Cost *</label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.cost}
                      onChange={(e) => setFormData({ ...formData, cost: parseFloat(e.target.value) })}
                      required
                      min="0"
                    />
                  </div>
                </>
              )}
              <div className="form-group">
                <label>Stock Quantity *</label>
                <input
                  type="number"
                  value={formData.stockQuantity}
                  onChange={(e) => setFormData({ ...formData, stockQuantity: parseInt(e.target.value) })}
                  required
                  min="0"
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="secondary" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="primary">
                  {editingProduct ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Products;
