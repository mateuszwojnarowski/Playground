using System.ComponentModel.DataAnnotations;

namespace WebApiFundamentals.Models
{
    public class PointOfInterestForCreationDto
    {
        [Required(ErrorMessage = "You should provide a name value.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
