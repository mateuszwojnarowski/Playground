using Microsoft.EntityFrameworkCore;
using OrderService.EntityModels;

namespace OrderService.Data
{
    public class OrderContext(DbContextOptions<OrderContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderDetails { get; set; }
    }
}
