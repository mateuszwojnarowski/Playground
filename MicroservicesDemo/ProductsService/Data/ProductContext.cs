using Microsoft.EntityFrameworkCore;
using ProductsService.Models;
using SharedModels.Models;

namespace ProductsService.Data;

public class ProductContext(DbContextOptions<ProductContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}