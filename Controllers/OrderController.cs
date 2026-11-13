using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MexicanRestaurant.Data;
using MexicanRestaurant.Models;
using MexicanRestaurant.Models.ViewModels;
using MexicanRestaurant.Services;

namespace MexicanRestaurant.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStripeService _stripeService;

        public OrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStripeService stripeService)
        {
            _context = context;
            _userManager = userManager;
            _stripeService = stripeService;
        }

        // GET: /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User)!;
            var cartItems = await _context.CartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var subtotal = cartItems.Sum(c => c.Subtotal);
            var tax = Math.Round(subtotal * 0.08m, 2);

            var viewModel = new CheckoutViewModel
            {
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                Address = user?.Address ?? "",
                OrderTotal = subtotal + tax,
                TaxAmount = tax,
                CartItems = cartItems
            };

            return View(viewModel);
        }

        // POST: /Order/PlaceOrder  (Creates order + redirects to Stripe)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            var cartItems = await _context.CartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid) return View("Checkout", model);

            var subtotal = cartItems.Sum(c => c.Subtotal);
            var tax = Math.Round(subtotal * 0.08m, 2);

            // Create the order
            var order = new Order
            {
                UserId = userId,
                TotalAmount = subtotal + tax,
                TaxAmount = tax,
                Status = OrderStatus.Pending,
                DeliveryAddress = model.Address,
                Notes = model.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order details
            var details = cartItems.Select(c => new OrderDetail
            {
                OrderId = order.Id,
                MenuItemId = c.MenuItemId,
                ItemName = c.MenuItem!.Name,
                UnitPrice = c.MenuItem.Price,
                Quantity = c.Quantity
            }).ToList();

            _context.OrderDetails.AddRange(details);
            await _context.SaveChangesAsync();

            // Load user for Stripe
            order.User = await _userManager.GetUserAsync(User);

            // Create Stripe session
            try
            {
                var successUrl = Url.Action("Success", "Order", null, Request.Scheme)!;
                var cancelUrl = Url.Action("Cancel", "Order", null, Request.Scheme)!;

                var session = await _stripeService.CreateCheckoutSessionAsync(order, details, successUrl, cancelUrl);

                // Save session ID
                order.StripeSessionId = session.Id;
                await _context.SaveChangesAsync();

                // Clear cart after successful order creation
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // Redirect to Stripe
                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                // If Stripe fails, still save order as pending
                TempData["Warning"] = "Payment gateway unavailable. Your order has been saved. Please contact us to complete payment.";
                return RedirectToAction("History");
            }
        }

        // GET: /Order/Success
        public async Task<IActionResult> Success(string session_id)
        {
            if (!string.IsNullOrEmpty(session_id))
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.StripeSessionId == session_id);

                if (order != null)
                {
                    order.IsPaid = true;
                    order.Status = OrderStatus.Paid;
                    await _context.SaveChangesAsync();
                }
            }

            return View();
        }

        // GET: /Order/Cancel
        public IActionResult Cancel()
        {
            return View();
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.MenuItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && (o.UserId == userId || User.IsInRole("Admin")));

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
