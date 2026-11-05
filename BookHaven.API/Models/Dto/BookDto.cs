namespace BookHaven.API.Models.Dto;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ImageLocalPath { get; set; }
    public decimal Price { get; set; }
}
