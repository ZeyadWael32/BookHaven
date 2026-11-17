using BookHaven.API.Models.Dto.Order;
using BookHaven.API.Services;
using BookHaven.Utility;

namespace BookHaven.API.Repository.IRepository;

public interface IOrderRepository : IRepository<OrderHeader, int>
{
    Task<OrderHeader?> GetOrderByIdAsync(int orderId, bool tracked = false, CancellationToken cancellationToken = default);
    Task<OrderHeader?> GetOrderByUserIdAsync(int orderId, string userId, bool tracked = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderHeader>> GetOrdersByUserIdAsync(string userId, bool tracked = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(SD.OrderStatus? status = null, bool tracked = false, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderHeader entity, CancellationToken cancellationToken = default);
}
