using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MexicanRestaurant.Data;
using MexicanRestaurant.Models;
using MexicanRestaurant.Models.ViewModels;

namespace MexicanRestaurant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ─── Dashboard ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var dashboard = new AdminDashboardViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders.Where(o => o.IsPaid).SumAsync(o => o.TotalAmount),
                TotalCustomers = await _context.Users.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync()
            };
            return View(dashboard);
        }

        // ─── Menu Items CRUD ──────────────────────────────────────────
        public async Task<IActionResult> MenuItems()
        {
            var items = await _context.MenuItems.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
            return View(items);
        }

        public IActionResult CreateMenuItem()
        {
            return View(new MenuItemViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var item = new MenuItem
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Category = model.Category,
                IsAvailable = model.IsAvailable,
                IsFeatured = model.IsFeatured
            };

            // Handle image upload
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "menu");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                item.ImageUrl = $"/images/menu/{fileName}";
            }
            else
            {
                item.ImageUrl = model.ImageUrl;
            }

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{item.Name} added to menu!";
            return RedirectToAction("MenuItems");
        }

        public async Task<IActionResult> EditMenuItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            var model = new MenuItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Category = item.Category,
                ImageUrl = item.ImageUrl,
                IsAvailable = item.IsAvailable,
                IsFeatured = item.IsFeatured
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var item = await _context.MenuItems.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.Price = model.Price;
            item.Category = model.Category;
            item.IsAvailable = model.IsAvailable;
            item.IsFeatured = model.IsFeatured;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "menu");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                item.ImageUrl = $"/images/menu/{fileName}";
            }
            else if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                item.ImageUrl = model.ImageUrl;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{item.Name} updated!";
            return RedirectToAction("MenuItems");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item != null)
            {
                _context.MenuItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{item.Name} removed from menu.";
            }
            return RedirectToAction("MenuItems");
        }

        // ─── Orders ───────────────────────────────────────────────────
        public async Task<IActionResult> Orders(string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var statusEnum))
                query = query.Where(o => o.Status == statusEnum);

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            ViewBag.StatusFilter = status;
            return View(orders);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            }
            return RedirectToAction("Orders");
        }
    }
}
