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
                new() { Name = "Fiction" },
                new() { Name = "Non-Fiction" },
                new() { Name = "Science Fiction" },
                new() { Name = "Fantasy" },
                new() { Name = "Biography" }
            });
            await _db.SaveChangesAsync();
        }

        if (!await _db.Authors.AnyAsync())
        {
            await _db.Authors.AddRangeAsync(new List<Author>
            {
                new() { Name = "Robert C. Martin" },
                new() { Name = "J.K. Rowling" },
                new() { Name = "Walter Isaacson" }
            });
            await _db.SaveChangesAsync();
        }

        if (!await _db.Books.AnyAsync())
        {
            var fictionCategory = await _db.Categories.FirstAsync(c => c.Name == "Fiction");
            var nonFictionCategory = await _db.Categories.FirstAsync(c => c.Name == "Non-Fiction");
            var scienceFictionCategory = await _db.Categories.FirstAsync(c => c.Name == "Science Fiction");
            var fantasyCategory = await _db.Categories.FirstAsync(c => c.Name == "Fantasy");
            var biographyCategory = await _db.Categories.FirstAsync(c => c.Name == "Biography");

            var author1 = await _db.Authors.FirstAsync(a => a.Name == "Robert C. Martin");
            var author2 = await _db.Authors.FirstAsync(a => a.Name == "J.K. Rowling");
            var author3 = await _db.Authors.FirstAsync(a => a.Name == "Walter Isaacson");

            var books = new List<Book>
            {
                new()
                {
                    Title = "Clean Code",
                    Isbn = "9780132350884",
                    PublishedYear = 2008,
                    Price = 45.99m,
                    CategoryId = nonFictionCategory.Id,
                    AuthorId = author1.Id,
                    Description = "A handbook of agile software craftsmanship that teaches developers how to write clean, maintainable code.",
                    CoverImageUrl = "https://placehold.co/600x400"
                },
                new()
                {
                    Title = "Clean Architecture",
                    Isbn = "9780134494166",
                    PublishedYear = 2017,
                    Price = 39.95m,
                    CategoryId = nonFictionCategory.Id,
                    AuthorId = author1.Id,
                    Description = "A craftsman's guide to software structure and design, presenting universal rules of software architecture.",
                    CoverImageUrl = "https://placehold.co/600x400"
                },
                new()
                {
                    Title = "Harry Potter and the Sorcerer's Stone",
                    Isbn = "9780590353427",
                    PublishedYear = 1997,
                    Price = 14.99m,
                    CategoryId = fantasyCategory.Id,
                    AuthorId = author2.Id,
                    Description = "The first book in the legendary wizarding chronicle following a young boy who discovers his magical heritage.",
                    CoverImageUrl = "https://placehold.co/600x400",
                },
                new()
                {
                    Title = "Harry Potter and the Chamber of Secrets",
                    Isbn = "9780439064873",
                    PublishedYear = 1998,
                    Price = 15.99m,
                    CategoryId = fantasyCategory.Id,
                    AuthorId = author2.Id,
                    Description = "The second book in the Harry Potter series, detailing his secondary year at Hogwarts School of Witchcraft and Wizardry.",
                    CoverImageUrl = "https://placehold.co/600x400",
                },
                new()
                {
                    Title = "Steve Jobs",
                    Isbn = "9781451648539",
                    PublishedYear = 2011,
                    Price = 29.99m,
                    CategoryId = biographyCategory.Id,
                    AuthorId = author3.Id,
                    Description = "The exclusive, bestselling biography of Apple's co-founder, based on more than forty interviews with Jobs conducted over two years.",
                    CoverImageUrl = "https://placehold.co/600x400",
                }
            };

            await _db.Books.AddRangeAsync(books);
            await _db.SaveChangesAsync();
        }
    }
}