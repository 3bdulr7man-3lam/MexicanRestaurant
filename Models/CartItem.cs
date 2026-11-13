using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MexicanRestaurant.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int MenuItemId { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public MenuItem? MenuItem { get; set; }

        // Computed property
        [NotMapped]
        public decimal Subtotal => (MenuItem?.Price ?? 0) * Quantity;
    }
}
