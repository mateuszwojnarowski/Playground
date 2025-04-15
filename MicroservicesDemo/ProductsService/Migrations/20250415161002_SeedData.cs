using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductsService.Migrations
{
    public partial class SeedProductsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Cost", "Description", "Name", "StockQuantity" },
                values: new object[,]
                {
                    { new Guid("0f5583e2-d5a3-491b-8e13-f57e04f46083"), 25m, "A pie from the past", "Perfectly Preserved Pie", 1 },
                    { new Guid("7223634c-c992-4967-9cda-4cb4192f0a2e"), 10m, "For adding lube whenever you need", "Aluminum Oil Can", 100 },
                    { new Guid("a0fa5a1f-fc38-4491-90da-2b04ea7bd679"), 5m, "Tasty beverage to kill your thirst", "Nuka-Cola", 10 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("0f5583e2-d5a3-491b-8e13-f57e04f46083"),
                    new Guid("7223634c-c992-4967-9cda-4cb4192f0a2e"),
                    new Guid("a0fa5a1f-fc38-4491-90da-2b04ea7bd679")
                });
        }
    }
}
