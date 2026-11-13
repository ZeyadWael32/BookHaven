using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.CartItem;

public class CartItemAddDto
{
    [Range(1, int.MaxValue, ErrorMessage = "BookId must be greater than 0.")]
    public int BookId { get; set; }
    [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
    public int Quantity { get; set; } = 1;
}
