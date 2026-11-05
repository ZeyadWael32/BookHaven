using BookHaven.API.Data;
using BookHaven.API.Repository.IRepository;

namespace BookHaven.API.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public IBookRepository Books { get; private set; }
    public ICategoryRepository Categories { get; private set; }
    public IAuthorRepository Authors { get; private set; }
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Books = new BookRepository(_context);
        Categories = new CategoryRepository(_context);
        Authors = new AuthorRepository(_context);
    }

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
