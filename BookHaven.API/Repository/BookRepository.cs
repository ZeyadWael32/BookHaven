using BookHaven.API.Data;
using BookHaven.API.Models;
using BookHaven.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Repository;

public class BookRepository : Repository<Book, int>, IBookRepository
{
    private readonly ApplicationDbContext _db;
    public BookRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task UpdateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        _db.Books.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _db.Books.AnyAsync(b => b.Isbn == isbn && (!excludeId.HasValue || b.Id != excludeId.Value), cancellationToken);
    }
}
