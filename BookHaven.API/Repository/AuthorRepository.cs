using BookHaven.API.Data;
using BookHaven.API.Models;
using BookHaven.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Repository;

public class AuthorRepository : Repository<Author, int>, IAuthorRepository
{
    private readonly ApplicationDbContext _db;
    public AuthorRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public Task UpdateAsync(Author entity, CancellationToken cancellationToken = default)
    {
        _db.Authors.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> HasBooksAsync(int authorId, CancellationToken cancellationToken = default)
    {
        return await _db.Books.AnyAsync(b => b.AuthorId == authorId, cancellationToken);
    }
}
