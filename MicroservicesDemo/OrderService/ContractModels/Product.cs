namespace OrderService.ContractModels
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public double StockQuantity { get; set; }
    }
}
