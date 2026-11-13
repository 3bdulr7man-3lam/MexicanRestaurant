using System.ComponentModel.DataAnnotations;

namespace MexicanRestaurant.Models.ViewModels
{
    // ─── Auth ViewModels ──────────────────────────────────────────────
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    // ─── Cart ViewModels ──────────────────────────────────────────────
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(c => c.Subtotal);
        public decimal Tax => Math.Round(Subtotal * 0.08m, 2);  // 8% tax
        public decimal Total => Subtotal + Tax;
    }

    // ─── Checkout ViewModels ─────────────────────────────────────────
    public class CheckoutViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Display(Name = "Delivery Address")]
        public string Address { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public List<CartItem> CartItems { get; set; } = new();
    }

    // ─── Menu ViewModels ─────────────────────────────────────────────
    public class MenuViewModel
    {
        public List<MenuItem> MenuItems { get; set; } = new();
        public string? SelectedCategory { get; set; }
        public List<string> Categories { get; set; } = new();
    }

    // ─── Admin ViewModels ────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
    }

    // ─── MenuItem Create/Edit ────────────────────────────────────────
    public class MenuItemViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        public IFormFile? ImageFile { get; set; }
    }
}
