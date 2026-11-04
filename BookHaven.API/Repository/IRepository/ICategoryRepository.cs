using BookHaven.API.Models;

namespace BookHaven.API.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category, int>
{
    Task UpdateAsync(Category entity, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasBooksAsync(int categoryId, CancellationToken cancellationToken = default);
}
