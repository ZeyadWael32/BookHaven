using BookHaven.API.Models;

namespace BookHaven.API.Repository.IRepository;

public interface IBookRepository : IRepository<Book, int>
{
    Task UpdateAsync(Book entity, CancellationToken cancellationToken = default);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null, CancellationToken cancellationToken = default);
}
