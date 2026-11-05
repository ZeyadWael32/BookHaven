namespace BookHaven.API.Repository.IRepository;

public interface IUnitOfWork
{
    IBookRepository Books { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveAsync(); // returns the number of state entries written to the database (int)
}
