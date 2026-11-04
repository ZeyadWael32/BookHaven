using BookHaven.API.Data;
using BookHaven.API.Models;
using BookHaven.API.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Repository;

public class CategoryRepository : Repository<Category, int>, ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _db.Categories.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _db.Categories.AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), cancellationToken);
    }

    public async Task<bool> HasBooksAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _db.Books.AnyAsync(b => b.CategoryId == categoryId, cancellationToken);
    }
}