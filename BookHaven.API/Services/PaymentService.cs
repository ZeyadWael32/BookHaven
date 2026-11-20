using BookHaven.API.Models.Dto.Payment;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;
using BookHaven.Utility;
using Stripe;
using Stripe.Checkout;

namespace BookHaven.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public PaymentService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<ServiceResult<CheckoutSessionDto>> CreateCheckoutSessionAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByUserIdAsync(orderId, userId, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
            return ServiceResult<CheckoutSessionDto>.Failure(
                ServiceErrorType.NotFound, "Order not found.", "The specified order does not exist or does not belong to the user.");

        if (order.PaymentStatus == SD.PaymentStatus.Paid || order.OrderStatus != SD.OrderStatus.Pending)
            return ServiceResult<CheckoutSessionDto>.Failure(
                ServiceErrorType.Validation, "Invalid order status.", "The order is either already paid or not in a pending state.");

        if (order.OrderDetails is null || !order.OrderDetails.Any())
            return ServiceResult<CheckoutSessionDto>.Failure(
                ServiceErrorType.Validation, "Empty order.", "The order has no items to process for payment.");


        if (!string.IsNullOrEmpty(order.StripeSessionId))
        {
            var existingSession = await new SessionService().GetAsync(order.StripeSessionId, cancellationToken: cancellationToken);

            // If the existing session is still open, return its URL instead of creating a new session
            if (existingSession.Status == "open")
                return ServiceResult<CheckoutSessionDto>.Success(new CheckoutSessionDto { Url = existingSession.Url });

            // If the existing session is complete or expired, we can create a new session
            if (existingSession.Status == "complete")
                return ServiceResult<CheckoutSessionDto>.Failure(
                    ServiceErrorType.Validation, "Payment already completed.", "The payment for this order has already been completed.");
        }

        var frontendBaseUrl = _config["Stripe:FrontendBaseUrl"];

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            throw new InvalidOperationException("Frontend base URL is not configured. Please set 'Stripe:FrontendBaseUrl' in the configuration.");

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = $"{frontendBaseUrl}/order/{order.Id}/success?session_id={{CHECKOUT_SESSION_ID}}", // Redirect URL after successful payment, session_id for retrieving the session to verify payment status
            CancelUrl = $"{frontendBaseUrl}/order/{order.Id}/cancel",
            ClientReferenceId = order.Id.ToString(),
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
            },
            LineItems = order.OrderDetails.Select(d => new SessionLineItemOptions
            {
                Quantity = d.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = decimal.ToInt64(decimal.Round(d.UnitPrice * 100m, 0, MidpointRounding.AwayFromZero)), // Convert to cents and round to avoid floating-point issues
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = d.Book.Title
                    },
                }
            }).ToList()
        };

        var session = await new SessionService().CreateAsync(options, cancellationToken: cancellationToken); // Create the Stripe checkout session

        order.StripeSessionId = session.Id; // Store the Stripe session ID in the database table for later verification
        await _unitOfWork.SaveAsync(cancellationToken);
        return ServiceResult<CheckoutSessionDto>.Success(new CheckoutSessionDto { Url = session.Url });
    }

    public async Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken = default)
    {
        // Only handle checkout session events
        if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted && 
            stripeEvent.Type != EventTypes.CheckoutSessionAsyncPaymentSucceeded && 
            stripeEvent.Type != EventTypes.CheckoutSessionAsyncPaymentFailed)
            return;

        // If the event is not a checkout session, exit the method
        if (stripeEvent.Data.Object is not Session session) 
            return; 

        // If the order ID is not present or invalid, exit the method
        if (!session.Metadata.TryGetValue("orderId", out var orderIdStr) || !int.TryParse(orderIdStr, out var orderId))
                return; 

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId, tracked: true, cancellationToken: cancellationToken);
            if (order is null)
                throw new InvalidOperationException($"Order with ID {orderId} not found.");

            // Ensure the event corresponds to the correct order and session
            if (session.Id != order.StripeSessionId || session.ClientReferenceId != order.Id.ToString())
                    return;

        switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                    if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted && session.PaymentStatus != "paid") return; // Ensure the payment was successful
                    if (order.PaymentStatus == SD.PaymentStatus.Paid)
                    {
                        // If the order is already marked as paid, we can check if it was cancelled and needs a refund
                        if (order.OrderStatus == SD.OrderStatus.Cancelled)
                        {
                            await RefundAsync(order, cancellationToken);
                            return;
                        }
                    }
                // If the order is marked as refunded or refund pending, we don't need to process it again
                if (order.PaymentStatus == SD.PaymentStatus.RefundPending || order.PaymentStatus == SD.PaymentStatus.Refunded)
                        return;

                if (session.Currency != "usd")
                        throw new InvalidOperationException($"Unexpected currency for order {orderId}: {session.Currency}");

                    if (session.AmountTotal != decimal.ToInt64(decimal.Round(order.OrderTotal * 100m, 0, MidpointRounding.AwayFromZero)))
                        throw new InvalidOperationException($"Payment amount mismatch for order {orderId}");

                    order.PaymentStatus = SD.PaymentStatus.Paid;
                    order.PaymentDate = DateTime.UtcNow;
                    order.PaymentIntentId = session.PaymentIntentId;
                    order.StripeSessionId = session.Id;
                    await _unitOfWork.SaveAsync(cancellationToken);

                    if (order.OrderStatus == SD.OrderStatus.Cancelled)
                        await RefundAsync(order, cancellationToken);

                    break;

                case EventTypes.CheckoutSessionAsyncPaymentFailed:
                    if (order.PaymentStatus == SD.PaymentStatus.Pending)
                    {
                        order.PaymentStatus = SD.PaymentStatus.Failed;
                        await _unitOfWork.SaveAsync(cancellationToken);
                    }
                    break;
            }
        }

    public async Task RefundAsync(OrderHeader order, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(order.PaymentIntentId))
            throw new InvalidOperationException("Cannot process refund: PaymentIntentId is missing for the order.");

        if (order.PaymentStatus != SD.PaymentStatus.Paid && order.PaymentStatus != SD.PaymentStatus.RefundPending)
            throw new InvalidOperationException("Cannot process refund: Order is not in a paid or refund pending state.");

        var refund = await new RefundService().CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = order.PaymentIntentId,
            Reason = RefundReasons.RequestedByCustomer
        },
        new RequestOptions { IdempotencyKey = $"refund-order-{order.Id}" },
        cancellationToken: cancellationToken);

        if (refund.Status == "succeeded")
        {
            order.PaymentStatus = SD.PaymentStatus.Refunded; 
        }
        else if (refund.Status == "pending")
        {
            order.PaymentStatus = SD.PaymentStatus.RefundPending;

        }
        else
        {
            throw new InvalidOperationException($"Refund failed for order {order.Id}. Refund status: {refund.Status}");
        }

        await _unitOfWork.SaveAsync(cancellationToken);
    }
}
