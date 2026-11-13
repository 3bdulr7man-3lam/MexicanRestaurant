using Stripe;
using Stripe.Checkout;
using MexicanRestaurant.Models;

namespace MexicanRestaurant.Services
{
    public interface IStripeService
    {
        Task<Session> CreateCheckoutSessionAsync(Order order, List<OrderDetail> details, string successUrl, string cancelUrl);
        Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId);
    }

    public class StripeService : IStripeService
    {
        private readonly IConfiguration _config;

        public StripeService(IConfiguration config)
        {
            _config = config;
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<Session> CreateCheckoutSessionAsync(
            Order order,
            List<OrderDetail> details,
            string successUrl,
            string cancelUrl)
        {
            var lineItems = details.Select(d => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(d.UnitPrice * 100), // Stripe uses cents
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = d.ItemName,
                    }
                },
                Quantity = d.Quantity
            }).ToList();

            // Add tax as a line item
            if (order.TaxAmount > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(order.TaxAmount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Tax (8%)",
                        }
                    },
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl,
                ClientReferenceId = order.Id.ToString(),
                CustomerEmail = order.User?.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.Id.ToString() }
                }
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }
    }
}
