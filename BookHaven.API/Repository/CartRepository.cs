using BookHaven.API.Data;
using BookHaven.API.Models;
using BookHaven.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Repository;

public class CartRepository : Repository<ShoppingCart, string>, ICartRepository
{
    private readonly ApplicationDbContext _db;
    public CartRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    public async Task<ShoppingCart?> GetCartByUserIdAsync(string userId, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<ShoppingCart> query = _db.ShoppingCarts;

        if (!tracked)
            query = query.AsNoTracking();

        query = query.Include(c => c.Items)
                     .ThenInclude(ci => ci.Book)
                     .ThenInclude(b => b.Author);

        return await query.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public Task UpdateAsync(ShoppingCart entity, CancellationToken cancellationToken = default)
    {
        _db.ShoppingCarts.Update(entity);
        return Task.CompletedTask;
    }
}
