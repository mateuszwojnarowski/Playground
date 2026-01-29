// ============================================================================
// ORDERS MANAGEMENT COMPONENT
// ============================================================================
// Interface for viewing and creating orders.
// Features:
// - List all orders with item counts
// - View order details (which products were ordered)
// - Create new orders with multiple products
// - Dynamic form for adding/removing order items
// - Real-time product availability check
// ============================================================================

import React, { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import { Order, Product, OrderItem } from '../types';

/**
 * Orders Management Component
 * 
 * Provides order viewing and creation functionality.
 * Communicates with Orders and Products microservices via API Gateway.
 */
const Orders: React.FC = () => {
  // ==========================================================================
  // STATE MANAGEMENT
  // ==========================================================================
  
  // Orders and products data from APIs
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]); // Needed for product names and stock
  
  // Loading and error states
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Modal control
  const [showModal, setShowModal] = useState(false);
  
  // Order details viewing
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [orderItems, setOrderItems] = useState<OrderItem[]>([]);
  
  // New order creation - array of items user wants to order
  const [newOrderItems, setNewOrderItems] = useState<Array<{ productId: string; quantity: number }>>([
    { productId: '', quantity: 1 } // Start with one empty item
  ]);

  // ==========================================================================
  // EFFECTS
  // ==========================================================================
  
  /**
   * Load orders and products on component mount
   * We need both for displaying order details and creating new orders
   */
  useEffect(() => {
    loadData();
  }, []); // Empty deps = run once on mount

  // ==========================================================================
  // API OPERATIONS
  // ==========================================================================
  
  /**
   * Fetch orders and products in parallel
   * Uses Promise.all for better performance
   */
  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Fetch both simultaneously for faster loading
      const [ordersData, productsData] = await Promise.all([
        apiService.getOrders(),
        apiService.getProducts()
      ]);
      
      setOrders(ordersData);
      setProducts(productsData);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  // ==========================================================================
  // EVENT HANDLERS
  // ==========================================================================
  
  /**
   * Load and display detailed items for a specific order
   * 
   * @param order - Order to view details for
   */
  const handleViewDetails = async (order: Order) => {
    try {
      // Fetch detailed item information
      const details = await apiService.getOrderDetails(order.id);
      setOrderItems(details);
      setSelectedOrder(order);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Failed to load order details');
    }
  };

  /**
   * Open modal to create a new order
   * Resets form to initial state
   */
  const handleCreateOrder = () => {
    setNewOrderItems([{ productId: '', quantity: 1 }]); // Reset to one empty item
    setShowModal(true);
  };

  /**
   * Add another item row to the order form
   * Allows ordering multiple different products
   */
  const addOrderItem = () => {
    setNewOrderItems([...newOrderItems, { productId: '', quantity: 1 }]);
  };

  /**
   * Remove an item row from the order form
   * 
   * @param index - Index of item to remove
   */
  const removeOrderItem = (index: number) => {
    setNewOrderItems(newOrderItems.filter((_, i) => i !== index));
  };

  /**
   * Update a specific field in an order item
   * 
   * @param index - Index of item to update
   * @param field - Field name to update ('productId' or 'quantity')
   * @param value - New value for the field
   */
  const updateOrderItem = (index: number, field: 'productId' | 'quantity', value: string | number) => {
    const updated = [...newOrderItems];
    updated[index] = { ...updated[index], [field]: value };
    setNewOrderItems(updated);
  };

  /**
   * Submit new order to API
   * 
   * Validates that all items have valid product and quantity,
   * then sends to backend which will:
   * 1. Check stock availability
   * 2. Reduce stock
   * 3. Create order
   * 
   * @param e - Form submit event
   */
  const handleSubmitOrder = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate: ensure all items have product selected and quantity > 0
    if (newOrderItems.some(item => !item.productId || item.quantity < 1)) {
      setError('Please fill all order items with valid values');
      return;
    }

    try {
      await apiService.createOrder(newOrderItems);
      
      // Success! Close modal, reset form, and reload data
      setShowModal(false);
      setNewOrderItems([{ productId: '', quantity: 1 }]);
      loadData(); // Refresh to show new order and updated stock
    } catch (err: any) {
      // Common errors: insufficient stock, product not found
      setError(err.response?.data?.message || err.message || 'Failed to create order');
    }
  };

  // ==========================================================================
  // UTILITY FUNCTIONS
  // ==========================================================================
  
  /**
   * Get product name from product ID
   * Used to display product names in order details
   * 
   * @param productId - Product GUID
   * @returns Product name or 'Unknown Product' if not found
   */
  const getProductName = (productId: string): string => {
    const product = products.find(p => p.id === productId);
    return product?.name || 'Unknown Product';
  };

  // ==========================================================================
  // RENDER
  // ==========================================================================
  
  // Show loading indicator while fetching data
  if (loading) return <div className="loading">Loading orders...</div>;

  return (
    <div className="container">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1>Orders</h1>
        <button className="primary" onClick={handleCreateOrder}>Create Order</button>
      </div>

      {error && <div className="error">{error}</div>}

      <table>
        <thead>
          <tr>
            <th>Order ID</th>
            <th>Items Count</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {orders.length === 0 ? (
            <tr>
              <td colSpan={3} style={{ textAlign: 'center', padding: '2rem' }}>
                No orders found. Create one to get started!
              </td>
            </tr>
          ) : (
            orders.map(order => (
              <tr key={order.id}>
                <td>{order.id}</td>
                <td>{order.orderDetails?.length || 0}</td>
                <td>
                  <button className="secondary" onClick={() => handleViewDetails(order)}>
                    View Details
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      {/* Order Details Modal */}
      {selectedOrder && (
        <div className="modal-overlay" onClick={() => setSelectedOrder(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Order Details - {selectedOrder.id}</h2>
              <button onClick={() => setSelectedOrder(null)}>×</button>
            </div>
            <table>
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Quantity</th>
                </tr>
              </thead>
              <tbody>
                {orderItems.map((item, index) => (
                  <tr key={index}>
                    <td>{getProductName(item.productId)}</td>
                    <td>{item.quantity}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Create Order Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Create Order</h2>
              <button onClick={() => setShowModal(false)}>×</button>
            </div>
            <form onSubmit={handleSubmitOrder}>
              {newOrderItems.map((item, index) => (
                <div key={index} style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', alignItems: 'flex-end' }}>
                  <div className="form-group" style={{ flex: 2, marginBottom: 0 }}>
                    <label>Product *</label>
                    <select
                      value={item.productId}
                      onChange={(e) => updateOrderItem(index, 'productId', e.target.value)}
                      required
                    >
                      <option value="">Select a product</option>
                      {products.map(product => (
                        <option key={product.id} value={product.id}>
                          {product.name} (Stock: {product.stockQuantity})
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group" style={{ flex: 1, marginBottom: 0 }}>
                    <label>Quantity *</label>
                    <input
                      type="number"
                      value={item.quantity}
                      onChange={(e) => updateOrderItem(index, 'quantity', parseInt(e.target.value))}
                      required
                      min="1"
                    />
                  </div>
                  {newOrderItems.length > 1 && (
                    <button 
                      type="button" 
                      className="danger" 
                      onClick={() => removeOrderItem(index)}
                      style={{ height: '38px' }}
                    >
                      Remove
                    </button>
                  )}
                </div>
              ))}
              <button type="button" className="secondary" onClick={addOrderItem} style={{ marginBottom: '1rem' }}>
                Add Item
              </button>
              <div className="modal-actions">
                <button type="button" className="secondary" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="primary">
                  Create Order
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Orders;
