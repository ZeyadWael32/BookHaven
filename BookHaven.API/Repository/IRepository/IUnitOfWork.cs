namespace BookHaven.API.Repository.IRepository;

public interface IUnitOfWork
{
    IBookRepository Books { get; }
    ICategoryRepository Categories { get; }
    IAuthorRepository Authors { get; }
    ICartRepository Carts { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default); // returns the number of state entries written to the database (int)
}
