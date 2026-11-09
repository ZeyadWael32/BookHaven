using BookHaven.API.Models;

namespace BookHaven.API.Repository.IRepository;

public interface IAuthorRepository : IRepository<Author, int>
{
    Task UpdateAsync(Author entity, CancellationToken cancellationToken = default);
    Task<bool> HasBooksAsync(int categoryId, CancellationToken cancellationToken = default);
}
