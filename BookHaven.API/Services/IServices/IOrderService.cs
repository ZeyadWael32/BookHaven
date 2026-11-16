using BookHaven.API.Models.Dto.Order;

namespace BookHaven.API.Services.IServices;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(string userId, OrderCreateDto orderCreateDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetUserOrderByIdAsync(int orderId, string userId, CancellationToken cancellationToken = default);
}
