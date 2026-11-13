using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.Category;

public class CategoryUpdateDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
    public string Name { get; set; } = string.Empty;
}
