using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using Microsoft.AspNetCore.JsonPatch;
using SharedModels.Models;
using System.Net;

namespace ProductsService.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(ProductContext context) : ControllerBase
{
    private readonly ProductContext _context = context;

    [HttpGet]
    [Authorize("Product View")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpGet("{id}")]
    [Authorize("Product View")]
    public async Task<ActionResult<Product>> GetProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPut("{id}/{stockQuantity}")]
    [Authorize("Product Stock")]
    public async Task<IActionResult> Put(Guid id, long stockQuantity)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound($"Could not find a product with id of {id}");
        }

        if (product.StockQuantity == stockQuantity)
        {
            return StatusCode((int)HttpStatusCode.NotModified);
        }

        if (stockQuantity < 0)
        {
            return BadRequest("Stock quantity cannot be negative");
        }

        product.StockQuantity = stockQuantity;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Products
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize("Product Edit")]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    [Authorize("Product Edit")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}