using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto.Order;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;
using BookHaven.Utility;

namespace BookHaven.API.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(string userId, OrderCreateDto orderCreateDto, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, tracked: true, cancellationToken);
        if (cart is null || !cart.Items.Any())
        {
            return ServiceResult<OrderDto>.Failure(
                ServiceErrorType.Validation, "Cart is empty or does not exist.", "Add at least one item to the cart before placing an order.");
        }

        var order = new OrderHeader
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            OrderStatus = SD.OrderStatus.Pending,
            PaymentStatus = SD.PaymentStatus.Pending,
            RecipientName = orderCreateDto.RecipientName.Trim(),
            PhoneNumber = orderCreateDto.PhoneNumber.Trim(),
            StreetAddress = orderCreateDto.StreetAddress.Trim(),
            City = orderCreateDto.City.Trim(),
            State = orderCreateDto.State.Trim(),
            PostalCode = orderCreateDto.PostalCode.Trim(),
        };

        foreach (var item in cart.Items)
        {
            order.OrderDetails.Add(new OrderDetail
            {
                BookId = item.BookId,
                Quantity = item.Quantity,
                UnitPrice = item.Book.Price // current price at the time of order
            });
        }

        order.OrderTotal = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

        await _unitOfWork.Orders.CreateAsync(order, cancellationToken);
        cart.Items.Clear(); // Clear the cart after creating the order
        cart.UpdatedAt = DateTime.UtcNow; 

        await _unitOfWork.SaveAsync(cancellationToken);

        var created = await _unitOfWork.Orders.GetOrderByUserIdAsync(order.Id, userId, tracked: false, cancellationToken);

        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(created));
    }

    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, tracked: false, cancellationToken);
        if (orders is null)
            return ServiceResult<IEnumerable<OrderDto>>.Failure(ServiceErrorType.NotFound, "Orders not found.", $"No orders found for user with ID {userId  }.");
        return ServiceResult<IEnumerable<OrderDto>>.Success(_mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    public async Task<ServiceResult<OrderDto?>> GetOrderByUserIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByUserIdAsync(id, userId, tracked: false, cancellationToken);
        if (order is null)
            return ServiceResult<OrderDto?>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");

        return ServiceResult<OrderDto?>.Success(_mapper.Map<OrderDto?>(order));
    }

    public async Task<ServiceResult<OrderDto>> CancelOrderAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");
        }
        if (order.UserId != userId)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Forbidden, "Unauthorized action.", $"User with ID {userId} is not authorized to cancel order with ID {id}.");
        }
        if (order.OrderStatus != SD.OrderStatus.Pending)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Order cannot be cancelled.", $"Order with ID {id} can only be cancelled while it is in a pending state.");
        }
        if (order.PaymentStatus is SD.PaymentStatus.Paid)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Payment already made.", $"Order with ID {id} cannot be cancelled because payment has already been made.");
        }

        order.OrderStatus = SD.OrderStatus.Cancelled;
        await _unitOfWork.SaveAsync(cancellationToken);
        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    #region Admin Methods

    public async Task<ServiceResult<IEnumerable<OrderDto>>> GetAllOrdersAsync(SD.OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersAsync(status, tracked: false, cancellationToken);
        if (orders is null || !orders.Any())
            return ServiceResult<IEnumerable<OrderDto>>.Failure(ServiceErrorType.NotFound, "Orders not found.", "No orders found.");
        return ServiceResult<IEnumerable<OrderDto>>.Success(_mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    public async Task<ServiceResult<OrderDto>> ProcessOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");
        }

        if (order.OrderStatus != SD.OrderStatus.Pending)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Invalid order status.", $"Order with ID {id} cannot be processed because it is not in a pending state.");
        }
        if (order.PaymentStatus != SD.PaymentStatus.Paid)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Payment not completed.", $"Order with ID {id} cannot be processed because payment has not been completed.");
        }

        order.OrderStatus = SD.OrderStatus.Processing;
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<ServiceResult<OrderDto>> ShipOrderAsync(int id, ShipOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");
        }
        if (order.OrderStatus != SD.OrderStatus.Processing)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Invalid order status.", $"Order with ID {id} cannot be shipped because it is not in a processing state.");
        }
        order.OrderStatus = SD.OrderStatus.Shipped;
        order.ShippingDate = DateTime.UtcNow;
        order.TrackingNumber = dto.TrackingNumber?.Trim();
        order.Carrier = dto.Carrier?.Trim();
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<ServiceResult<OrderDto>> DeliverOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");
        }
        if (order.OrderStatus != SD.OrderStatus.Shipped)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Invalid order status.", $"Order with ID {id} cannot be marked as delivered because it is not in a shipped state.");
        }
        order.OrderStatus = SD.OrderStatus.Delivered;
        await _unitOfWork.SaveAsync(cancellationToken);
        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
    
    public async Task<ServiceResult<OrderDto>> AdminCancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, tracked: true, cancellationToken: cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.NotFound, "Order not found.", $"No order found with ID {id}.");
        }
        if (order.OrderStatus != SD.OrderStatus.Pending && order.OrderStatus != SD.OrderStatus.Processing)
            return ServiceResult<OrderDto>.Failure(ServiceErrorType.Conflict, "Order cannot be cancelled.", $"Order with ID {id} can only be cancelled while it is in a pending or processing state.");

        order.OrderStatus = SD.OrderStatus.Cancelled;
        await _unitOfWork.SaveAsync(cancellationToken);
        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    #endregion
}
