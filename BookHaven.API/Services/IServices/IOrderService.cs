using BookHaven.API.Models.Dto.Order;
using BookHaven.Utility;

namespace BookHaven.API.Services.IServices;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(string userId, OrderCreateDto orderCreateDto, CancellationToken cancellationToken = default);
    Task<ServiceResult<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto?>> GetOrderByUserIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IEnumerable<OrderDto>>> GetAllOrdersAsync(SD.OrderStatus? status = null, CancellationToken cancellationToken = default); // Admin only
    Task<ServiceResult<OrderDto>> AdminCancelOrderAsync(int id, CancellationToken cancellationToken = default); // Admin only

    Task<ServiceResult<OrderDto>> ProcessOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> ShipOrderAsync(int id, ShipOrderDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> DeliverOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> CancelOrderAsync(int id, string userId, CancellationToken cancellationToken = default);
}
