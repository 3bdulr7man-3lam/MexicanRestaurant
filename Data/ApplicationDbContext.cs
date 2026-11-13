using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MexicanRestaurant.Models;

namespace MexicanRestaurant.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CartItem: one user, one menu item per row (unique per user+item)
            builder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(c => c.MenuItem)
                .WithMany(m => m.CartItems)
                .HasForeignKey(c => c.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.MenuItem)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed menu items
            builder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, Name = "Beef Tacos", Description = "Crispy corn tortillas stuffed with seasoned ground beef, fresh salsa, cheddar cheese, and lettuce.", Price = 8.99m, Category = "Tacos", ImageUrl = "/images/beef-tacos.jpg", IsAvailable = true, IsFeatured = true },
                new MenuItem { Id = 2, Name = "Chicken Tacos", Description = "Grilled chicken breast with avocado crema, pico de gallo, and cilantro in soft flour tortillas.", Price = 9.49m, Category = "Tacos", ImageUrl = "/images/chicken-tacos.jpg", IsAvailable = true, IsFeatured = true },
                new MenuItem { Id = 3, Name = "Fish Tacos", Description = "Beer-battered cod with shredded cabbage, chipotle mayo, and fresh lime on corn tortillas.", Price = 10.99m, Category = "Tacos", ImageUrl = "/images/fish-tacos.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 4, Name = "Beef Burrito", Description = "A massive flour tortilla filled with seasoned beef, rice, black beans, sour cream, and salsa.", Price = 12.99m, Category = "Burritos", ImageUrl = "/images/beef-burrito.jpg", IsAvailable = true, IsFeatured = true },
                new MenuItem { Id = 5, Name = "Veggie Burrito", Description = "Roasted seasonal vegetables, black beans, Spanish rice, guacamole wrapped in a warm flour tortilla.", Price = 11.49m, Category = "Burritos", ImageUrl = "/images/veggie-burrito.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 6, Name = "Chicken Enchiladas", Description = "Three corn tortillas filled with shredded chicken, smothered in red chile sauce and melted cheese.", Price = 13.49m, Category = "Burritos", ImageUrl = "/images/enchiladas.jpg", IsAvailable = true, IsFeatured = true },
                new MenuItem { Id = 7, Name = "Classic Nachos", Description = "Crispy tortilla chips loaded with cheddar cheese, jalapeños, sour cream, and guacamole.", Price = 9.99m, Category = "Nachos", ImageUrl = "/images/nachos.jpg", IsAvailable = true, IsFeatured = true },
                new MenuItem { Id = 8, Name = "Loaded Nachos", Description = "Everything on Classic Nachos plus seasoned beef, black beans, pico de gallo, and drizzled hot sauce.", Price = 13.99m, Category = "Nachos", ImageUrl = "/images/loaded-nachos.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 9, Name = "Quesadilla", Description = "Grilled flour tortilla stuffed with melted Monterey Jack cheese. Served with salsa and sour cream.", Price = 7.99m, Category = "Nachos", ImageUrl = "/images/quesadilla.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 10, Name = "Horchata", Description = "Traditional Mexican rice drink with cinnamon and vanilla. Refreshing and sweet.", Price = 3.99m, Category = "Drinks", ImageUrl = "/images/horchata.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 11, Name = "Jamaica (Hibiscus Tea)", Description = "Chilled hibiscus flower tea, lightly sweetened. A Mexican classic full of antioxidants.", Price = 3.49m, Category = "Drinks", ImageUrl = "/images/jamaica.jpg", IsAvailable = true, IsFeatured = false },
                new MenuItem { Id = 12, Name = "Mexican Soda", Description = "Authentic Jarritos fruit soda in your choice of Tamarind, Mandarin, or Lime flavors.", Price = 2.99m, Category = "Drinks", ImageUrl = "/images/mexican-soda.jpg", IsAvailable = true, IsFeatured = false }
            );
        }
    }
}
