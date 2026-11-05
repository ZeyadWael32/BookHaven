namespace BookHaven.API.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public Author Authors { get; set; } = null!;
    public string Isbn { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ImageLocalPath { get; set; }
    public decimal Price { get; set; }
}
