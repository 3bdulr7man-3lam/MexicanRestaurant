using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MexicanRestaurant.Data;
using MexicanRestaurant.Models.ViewModels;

namespace MexicanRestaurant.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Menu
        public async Task<IActionResult> Index(string? category)
        {
            var query = _context.MenuItems.Where(m => m.IsAvailable);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(m => m.Category == category);

            var items = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
            var categories = await _context.MenuItems
                .Where(m => m.IsAvailable)
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var viewModel = new MenuViewModel
            {
                MenuItems = items,
                SelectedCategory = category,
                Categories = categories
            };

            return View(viewModel);
        }

        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
