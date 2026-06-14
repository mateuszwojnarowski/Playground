using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.Data;
using OrderService.EntityModels;
using SharedModels.Models;
using System.Net.Http.Headers;

namespace OrderService.Controllers;

[Route("[controller]")]
[ApiController]
public class OrdersController(
    OrderContext context,
    IHttpClientFactory httpClientFactory,
    ILogger<OrdersController> logger) : ControllerBase
{
    private readonly OrderContext _context = context;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<OrdersController> _logger = logger;

    [HttpGet]
    [Authorize("Order View")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders.Include(x => x.OrderDetails).ToListAsync();
    }

    [HttpGet("{id}/OrderDetails")]
    [Authorize("Order View")]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderDetails(Guid id)
    {
        var order = await _context.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return order.OrderDetails.ToList();
    }

    [HttpGet("{id}")]
    [Authorize("Order View")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    [HttpPost]
    [Authorize("Order Edit")]
    public async Task<ActionResult<Order>> PostOrder(Order order)
    {
        _logger.LogInformation("Starting order creation with {ItemCount} items", order.OrderDetails.Count());

        // Get HTTP client from factory (proper pattern - reuses connections)
        var httpClient = _httpClientFactory.CreateClient("ProductService");
        
        // Extract and pass the bearer token
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            _logger.LogWarning("Missing or invalid authorization header");
            return Unauthorized("Missing authorization token");
        }
        
        var token = authHeader.Split(" ")[1];
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Step 1: Fetch only the products we need (not all products)
        var productIds = order.OrderDetails.Select(x => x.ProductId).Distinct().ToList();
        var products = new List<Product>();
        
        try
        {
            foreach (var productId in productIds)
            {
                var response = await httpClient.GetAsync($"products/{productId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var product = JsonConvert.DeserializeObject<Product>(json);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }
                else
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch product information");
            return StatusCode(503, "Product service unavailable");
        }

        // Step 2: Validate all products exist
        if (products.Count != productIds.Count)
        {
            var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
            _logger.LogWarning("Missing products: {MissingIds}", string.Join(", ", missingIds));
            return BadRequest($"Products not found: {string.Join(", ", missingIds)}");
        }

        // Step 3: Validate stock availability and capture prices
        var stockValidation = new List<(Product product, int requestedQty, long newStock)>();
        
        foreach (var orderItem in order.OrderDetails)
        {
            var product = products.First(p => p.Id == orderItem.ProductId);
            var requestedQty = orderItem.Quantity;
            var newStock = product.StockQuantity - requestedQty;

            if (newStock < 0)
            {
                _logger.LogWarning("Insufficient stock for {ProductName}. Available: {Available}, Requested: {Requested}",
                    product.Name, product.StockQuantity, requestedQty);
                return BadRequest($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}, Requested: {requestedQty}");
            }

            // CRITICAL: Capture the current product price in the order
            orderItem.SoldAtUnitPrice = product.Cost;
            
            stockValidation.Add((product, requestedQty, newStock));
        }

        // Step 4: Update stock with compensating transaction support
        var successfulUpdates = new List<(Guid productId, long originalStock)>();
        
        try
        {
            foreach (var (product, requestedQty, newStock) in stockValidation)
            {
                _logger.LogInformation("Reducing stock for {ProductName} from {OldStock} to {NewStock}",
                    product.Name, product.StockQuantity, newStock);

                var response = await httpClient.PutAsync($"products/{product.Id}/{newStock}", null);

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
                {
                    // Track successful update for potential rollback
                    successfulUpdates.Add((product.Id, product.StockQuantity));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to update stock for product {ProductId}. Status: {Status}, Error: {Error}",
                        product.Id, response.StatusCode, errorContent);
                    
                    // COMPENSATING TRANSACTION: Rollback all successful updates
                    await RollbackStockUpdatesAsync(httpClient, successfulUpdates);
                    
                    return StatusCode((int)response.StatusCode, 
                        $"Failed to reserve stock for {product.Name}. Transaction rolled back.");
                }
            }

            // Step 5: Persist order to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Order {OrderId} created successfully with {ItemCount} items",
                order.Id, order.OrderDetails.Count());

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during order creation. Rolling back stock updates.");
            
            // COMPENSATING TRANSACTION: Rollback on any failure
            await RollbackStockUpdatesAsync(httpClient, successfulUpdates);
            
            return StatusCode(500, "Order creation failed. Stock has been rolled back.");
        }
    }

    /// <summary>
    /// Compensating transaction: Rollback stock updates if order creation fails
    /// </summary>
    private async Task RollbackStockUpdatesAsync(
        HttpClient httpClient, 
        List<(Guid productId, long originalStock)> successfulUpdates)
    {
        if (successfulUpdates.Count == 0)
        {
            return;
        }

        _logger.LogWarning("Rolling back {Count} stock updates", successfulUpdates.Count);

        foreach (var (productId, originalStock) in successfulUpdates)
        {
            try
            {
                var rollbackResponse = await httpClient.PutAsync($"products/{productId}/{originalStock}", null);
                if (rollbackResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully rolled back stock for product {ProductId} to {Stock}",
                        productId, originalStock);
                }
                else
                {
                    _logger.LogError("CRITICAL: Failed to rollback stock for product {ProductId}. Manual intervention required!",
                        productId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL: Exception during rollback for product {ProductId}. Manual intervention required!",
                    productId);
            }
        }
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    [Authorize("Order Edit")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        //return back stuff to stock

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}