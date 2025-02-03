using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ProductsService.Data;

public class ProductContext(DbContextOptions<ProductContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}