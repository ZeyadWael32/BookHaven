using BookHaven.API.Models.Dto.Order;

namespace BookHaven.API.Services.IServices;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(string userId, OrderCreateDto orderCreateDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByUserIdAsync(int orderId, string userId, CancellationToken cancellationToken = default);

    Task<ServiceResult<OrderDto>> ProcessOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> ShipOrderAsync(int id, ShipOrderDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> DeliverOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<OrderDto>> CancelOrderAsync(int id, string userId, CancellationToken cancellationToken = default);
}
