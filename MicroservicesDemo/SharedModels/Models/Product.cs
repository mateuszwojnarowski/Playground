using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Models;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }

    [Required, MaxLength(250)]
    public required string Name { get; init; }

    [MaxLength(2048)]
    public string? Description { get; init; }

    [Required]
    public decimal Cost { get; init; }

    [Required]
    public long StockQuantity { get; set; }
}