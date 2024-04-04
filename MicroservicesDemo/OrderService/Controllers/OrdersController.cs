using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.Data;
using OrderService.EntityModels;
using ProductsService.Models;
using SharedModels.Models;
using Exception = System.Exception;

namespace OrderService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;

        public OrdersController(OrderContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.Include(x => x.OrderDetails).ToListAsync();
        }

        [HttpGet("{id}/OrderDetails")]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderDetails(Guid id)
        {
            var order = await _context.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order.OrderDetails.ToList();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            using var httpClient = new HttpClient()
            {
                BaseAddress = new
                    Uri("http://localhost:5071/api/")
            };

            var response = await httpClient.GetAsync("products");

            List<Product>? products = null;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                products = JsonConvert.DeserializeObject<List<Product>>(json);
            }

            var orderedProducts = products?.Where(x => order.OrderDetails.Any(y => y.ProductId == x.Id))
                .Select(x => new ProductsPatch { Id = x.Id, Stock = x.StockQuantity }).ToArray();

            if (orderedProducts is null or [] || order.OrderDetails.Count() > orderedProducts.Length)
            {
                return BadRequest("Could not find all ordered products.");
            }

            var patchDoc = new JsonPatchDocument();

            foreach (var orderedProduct in orderedProducts)
            {
                orderedProduct.Stock -= order.OrderDetails.Single(x => x.ProductId == orderedProduct.Id).Quantity;
                if (orderedProduct.Stock < 0)
                {
                    return BadRequest($"Not enough stock of {orderedProduct}");
                }

                var index = products.FindIndex(x => x.Id == orderedProduct.Id);
                patchDoc.Replace($"/{index}", orderedProduct);
            }


            response = await httpClient.PatchAsync("products",
                new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json"));

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetOrder", new { id = order.Id }, order);
            }

            return BadRequest();
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
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
}
