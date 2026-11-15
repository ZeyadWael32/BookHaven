namespace BookHaven.API.Repository.IRepository;

public interface IOrderRepository : IRepository<OrderHeader, int>
{
    Task<OrderHeader?> GetOrderForUserAsync(int orderId, string userId, bool tracked = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderHeader>> GetOrdersByUserIdAsync(string userId, bool tracked = false, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderHeader entity, CancellationToken cancellationToken = default);
}
