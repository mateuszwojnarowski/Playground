using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Models;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required, MaxLength(250)]
    public string Name { get; set; }

    [MaxLength(2048)]
    public string? Description { get; set; } = string.Empty;

    [Required]
    public decimal Cost { get; set; }

    [Required]
    public double StockQuantity { get; set; }
}