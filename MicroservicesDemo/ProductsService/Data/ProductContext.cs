using Microsoft.EntityFrameworkCore;
using ProductsService.Models;
using SharedModels.Models;

namespace ProductsService.Data;

public class ProductContext(DbContextOptions<ProductContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<Product>()
    //        .HasData(
    //            new Product
    //            {
    //                Id =  Guid.NewGuid(), Name = "Nuka-Cola", Description = "Tasty beverage to kill your thirst", Cost = 5, StockQuantity = 10
    //            },
    //            new Product
    //            {
    //                Id = Guid.NewGuid(), Name = "Perfectly Preserved Pie", Description = "A pie from the past", Cost = 25, StockQuantity = 1
    //            },
    //            new Product
    //            {
    //                Id = Guid.NewGuid(), Name = "Aluminum Oil Can", Description = "For adding lube whenever you need", Cost = 10, StockQuantity = 100
    //            });
    //}
}