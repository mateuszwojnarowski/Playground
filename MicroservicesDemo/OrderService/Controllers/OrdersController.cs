using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.Data;
using OrderService.EntityModels;
using SharedModels.Models;

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

            List<Product> models;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                models = JsonConvert.DeserializeObject<List<Product>>(json);
            }



            // get products from products service 
            // check if they're in stock and stock is able to fulfill the order;

            // patch to products service with updated stock
            // if succeeded can add order and save changes

            //_context.Orders.Add(order);
            //await _context.SaveChangesAsync();

            return NoContent();
            //return CreatedAtAction("GetOrder", new { id = order.Id }, order);
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
