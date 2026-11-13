using BookHaven.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.Book;

public class BookCreateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
    public int AuthorId { get; set; }
    [Required]
    [Isbn13]
    public string Isbn { get; set; } = string.Empty;
    [Range(1450, 2100, ErrorMessage = "Published year must be between 1450 and 2100.")]
    public int PublishedYear { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
    public int CategoryId { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public decimal Price { get; set; }
}