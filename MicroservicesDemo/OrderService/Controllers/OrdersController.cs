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
public class OrdersController(OrderContext context) : ControllerBase
{
    private readonly OrderContext _context = context;

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
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new
            Uri("https://localhost:7290/api/");

        // pass the token to this request
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Headers["Authorization"].ToString().Split(" ")[1]);
        var response = await httpClient.GetAsync("products");

        List<Product>? products = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            products = JsonConvert.DeserializeObject<List<Product>>(json);
        }

        if (products is null)
        {
            return BadRequest("Could not get products.");
        }

        var orderedProducts = products.Where(x => order.OrderDetails.Any(y => y.ProductId == x.Id)).ToArray();

        if (orderedProducts is null or [] || order.OrderDetails.Count() > orderedProducts.Length)
        {
            return BadRequest("Could not find all ordered products.");
        }

        bool isError = false;
        List<(HttpStatusCode code, HttpContent content)> errors = [];

        foreach (var orderedProduct in orderedProducts)
        {
            orderedProduct.StockQuantity -= order.OrderDetails.Single(x => x.ProductId == orderedProduct.Id).Quantity;
            if (orderedProduct.StockQuantity < 0)
            {
                return BadRequest($"Not enough stock of {orderedProduct.Name}");
            }

            // very inefficient but cannae be arsed to do it properly for this play example
            response = await httpClient.PutAsync($"products/{orderedProduct.Id}/{orderedProduct.StockQuantity}", null);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                isError = true;
                errors.Add((response.StatusCode, response.Content));
            }
        }

        if (isError)
        {
            return BadRequest(errors);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetOrder", new { id = order.Id }, order);
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