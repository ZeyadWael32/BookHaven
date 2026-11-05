using BookHaven.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Data;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _db;

    public DbInitializer(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task InitializeAsync()
    {
        try
        {
            if ((await _db.Database.GetPendingMigrationsAsync()).Any())
            {
                await _db.Database.MigrateAsync();
            }

        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new Exception("An error occurred while applying migrations.", ex);
        }

        if (!await _db.Categories.AnyAsync())
        {
            await _db.Categories.AddRangeAsync(new List<Category>
            {
                new() { Id = 1, Name = "Fiction" },
                new() { Id = 2, Name = "Non-Fiction" },
                new() { Id = 3, Name = "Science Fiction" },
                new() { Id = 4, Name = "Fantasy" },
                new() { Id = 5, Name = "Biography" }
            });
            await _db.SaveChangesAsync();
        }

        if (!await _db.Authors.AnyAsync())
        {
            await _db.Authors.AddRangeAsync(new List<Author>
            {
                new() { Id = 1, Name = "Robert C. Martin" },
                new() { Id = 2, Name = "J.K. Rowling" },
                new() { Id = 3, Name = "Walter Isaacson" }
            });
            await _db.SaveChangesAsync();
        }

        if (!await _db.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new()
                {
                    Title = "Clean Code",
                    Isbn = "9780132350884",
                    PublishedYear = 2008,
                    Price = 45.99m,
                    CategoryId = 2,
                    AuthorId = 1,
                    Description = "A handbook of agile software craftsmanship that teaches developers how to write clean, maintainable code.",
                    CoverImageUrl = "/images/placeholders/clean-code.jpg",
                    ImageLocalPath = "wwwroot/images/placeholders/clean-code.jpg"
                },
                new()
                {
                    Title = "Clean Architecture",
                    Isbn = "9780134494166",
                    PublishedYear = 2017,
                    Price = 39.95m,
                    CategoryId = 2,
                    AuthorId = 1,
                    Description = "A craftsman's guide to software structure and design, presenting universal rules of software architecture.",
                    CoverImageUrl = "/images/placeholders/clean-architecture.jpg",
                    ImageLocalPath = "wwwroot/images/placeholders/clean-architecture.jpg"
                },
                new()
                {
                    Title = "Harry Potter and the Sorcerer's Stone",
                    Isbn = "9780590353427",
                    PublishedYear = 1997,
                    Price = 14.99m,
                    CategoryId = 4,
                    AuthorId = 2,
                    Description = "The first book in the legendary wizarding chronicle following a young boy who discovers his magical heritage.",
                    CoverImageUrl = "/images/placeholders/harry-potter.jpg",
                    ImageLocalPath = "wwwroot/images/placeholders/harry-potter.jpg"
                },
                new()
                {
                    Title = "Harry Potter and the Chamber of Secrets",
                    Isbn = "9780439064873",
                    PublishedYear = 1998,
                    Price = 15.99m,
                    CategoryId = 4,
                    AuthorId = 2,
                    Description = "The second book in the Harry Potter series, detailing his secondary year at Hogwarts School of Witchcraft and Wizardry.",
                    CoverImageUrl = "/images/placeholders/chamber-of-secrets.jpg",
                    ImageLocalPath = "wwwroot/images/placeholders/chamber-of-secrets.jpg"
                },
                new()
                {
                    Title = "Steve Jobs",
                    Isbn = "9781451648539",
                    PublishedYear = 2011,
                    Price = 29.99m,
                    CategoryId = 5,
                    AuthorId = 3,
                    Description = "The exclusive, bestselling biography of Apple's co-founder, based on more than forty interviews with Jobs conducted over two years.",
                    CoverImageUrl = "/images/placeholders/steve-jobs.jpg",
                    ImageLocalPath = "wwwroot/images/placeholders/steve-jobs.jpg"
                }
            };

            await _db.Books.AddRangeAsync(books);
            await _db.SaveChangesAsync();
        }
    }
}