using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto.Order;
using BookHaven.API.Repository;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;
using BookHaven.Utility;

namespace BookHaven.API.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(UnitOfWork unitOfWork, IMapper mapper)
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

        var created = await _unitOfWork.Orders.GetOrderForUserAsync(order.Id, userId, tracked: false, cancellationToken);

        return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(created));
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, tracked: false, cancellationToken);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetUserOrderByIdAsync(int orderId, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderForUserAsync(orderId, userId, tracked: false, cancellationToken);
        if (order is null)
            return null;

        return _mapper.Map<OrderDto>(order);
    }
}
