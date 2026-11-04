using BookHaven.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto;

public class BookUpdateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;
    [Required]
    [Isbn13]
    public string Isbn { get; set; } = string.Empty;
    [Range(1450, 2100, ErrorMessage = "Published year must be between 1450 and 2100.")]
    public int PublishedYear { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
    public int CategoryId { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
}
