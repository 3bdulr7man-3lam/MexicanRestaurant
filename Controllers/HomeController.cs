using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MexicanRestaurant.Data;

namespace MexicanRestaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Load featured items for the homepage
            var featuredItems = await _context.MenuItems
                .Where(m => m.IsFeatured && m.IsAvailable)
                .Take(4)
                .ToListAsync();

            return View(featuredItems);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
