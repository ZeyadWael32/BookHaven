using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        var frontendBaseUrl = _config["Stripe:FrontendBaseUrl"];

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
                    UnitAmount = (long)(d.UnitPrice * 100), // Stripe expects the amount in cents
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
        if (stripeEvent.Data.Object is not Session session) 
            return; // If the event is not a checkout session, exit the method

        if (!session.Metadata.TryGetValue("orderId", out var orderIdStr) || !int.TryParse(orderIdStr, out var orderId))
                return; // If the order ID is not present or invalid, exit the method

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId, tracked: true, cancellationToken: cancellationToken);
            if (order is null)
                throw new InvalidOperationException($"Order with ID {orderId} not found.");

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                    if (session.PaymentStatus != "paid") return; // Ensure the payment was successful
                    if (order.PaymentStatus == SD.PaymentStatus.Paid) return; // If the order is already marked as paid, no further action is needed

                    if (session.AmountTotal != (long)Math.Round(order.OrderTotal * 100m))
                    throw new InvalidOperationException($"Payment amount mismatch for order {orderId}");

                    order.PaymentStatus = SD.PaymentStatus.Paid;
                    order.PaymentDate = DateTime.UtcNow;
                    order.PaymentIntentId = session.PaymentIntentId;
                    order.StripeSessionId = session.Id;
                    await _unitOfWork.SaveAsync(cancellationToken);

                    if (order.OrderStatus == SD.OrderStatus.Cancelled)
                    {
                        await RefundAsync(order, cancellationToken);
                        await _unitOfWork.SaveAsync(cancellationToken);
                    }
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

        if (order.PaymentStatus != SD.PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot process refund: Order is not in a paid state.");

        await new RefundService().CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = order.PaymentIntentId,
            Reason = RefundReasons.RequestedByCustomer
        },
        new RequestOptions { IdempotencyKey = $"refund-order-{order.Id}" },
        cancellationToken: cancellationToken);

        order.PaymentStatus = SD.PaymentStatus.Refunded;
        await _unitOfWork.SaveAsync(cancellationToken);
    }
}
