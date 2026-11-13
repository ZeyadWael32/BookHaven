using BookHaven.API.Models;

namespace BookHaven.API.Repository.IRepository;

public interface ICartRepository : IRepository<ShoppingCart, string>
{
    Task<ShoppingCart?> GetCartByUserIdAsync(string userId, bool tracked = false, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
}
