using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.CartItem;

public class CartItemUpdateDto
{
    [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
    public int Quantity { get; set; }
}
