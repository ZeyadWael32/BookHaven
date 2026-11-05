using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto;

public class AuthorUpdateDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;
    [StringLength(2000, ErrorMessage = "Biography cannot exceed 2000 characters.")]
    public string? Biography { get; set; }
    [StringLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters.")]
    public string? ProfileImageUrl { get; set; }
    [StringLength(500, ErrorMessage = "Local image path cannot exceed 500 characters.")]
    public string? ImageLocalPath { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
