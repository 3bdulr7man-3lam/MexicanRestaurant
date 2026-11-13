using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MexicanRestaurant.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Preparing,
        Ready,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Stripe payment fields
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public bool IsPaid { get; set; } = false;

        // Delivery info
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Computed
        [NotMapped]
        public decimal SubTotal => TotalAmount - TaxAmount;
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        [Required]
        public string ItemName { get; set; } = string.Empty; // snapshot of name at order time

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // snapshot of price at order time

        [Required, Range(1, 100)]
        public int Quantity { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public MenuItem? MenuItem { get; set; }

        // Computed
        [NotMapped]
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
