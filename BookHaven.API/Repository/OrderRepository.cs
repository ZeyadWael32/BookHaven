using BookHaven.API.Data;
using BookHaven.API.Repository.IRepository;
using BookHaven.Utility;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Repository;

public class OrderRepository : Repository<OrderHeader, int>, IOrderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<OrderHeader?> GetOrderByIdAsync(int orderId, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<OrderHeader> query = _db.OrderHeaders;

        if (!tracked)
            query = query.AsNoTracking();

        query = query.Include(o => o.OrderDetails)
                     .ThenInclude(od => od.Book)
                     .ThenInclude(b => b.Author);

        return await query.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }
    public async Task<OrderHeader?> GetOrderByUserIdAsync(int orderId, string userId, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<OrderHeader> query = _db.OrderHeaders;

        if (!tracked)
            query = query.AsNoTracking();

        query = query.Include(o => o.OrderDetails)
                     .ThenInclude(od => od.Book)
                     .ThenInclude(b => b.Author);

        return await query.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<OrderHeader>> GetOrdersByUserIdAsync(string userId, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<OrderHeader> query = _db.OrderHeaders;

        if (!tracked)
            query = query.AsNoTracking();

        query = query.Include(o => o.OrderDetails)
                     .ThenInclude(od => od.Book)
                     .ThenInclude(b => b.Author);

        return await query.Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id) // In case of multiple orders on the same date, order by Id to maintain consistency
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(SD.OrderStatus? status = null, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<OrderHeader> query = _db.OrderHeaders;

        if (!tracked)
            query = query.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(o => o.OrderStatus == status.Value);
        }
        
        query = query.Include(o => o.OrderDetails)
                     .ThenInclude(od => od.Book)
                     .ThenInclude(b => b.Author);

        return await query.OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(OrderHeader entity, CancellationToken cancellationToken = default)
    {
        _db.OrderHeaders.Update(entity);
        return Task.CompletedTask;
    }
}
