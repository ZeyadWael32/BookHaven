using BookHaven.API.Models.Dto.Payment;
using Stripe;

namespace BookHaven.API.Services.IServices;

public interface IPaymentService
{
    Task<ServiceResult<CheckoutSessionDto>> CreateCheckoutSessionAsync(int orderId, string userId, CancellationToken cancellationToken = default); // Creates a Stripe Checkout session for the specified order and user, returning the session details or an error result.
    Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken = default); // Handles incoming Stripe webhook events, processing them based on their type (e.g., payment succeeded, payment failed) and updating the corresponding order status in the system.
    Task RefundAsync(OrderHeader order, CancellationToken cancellationToken = default);
}
