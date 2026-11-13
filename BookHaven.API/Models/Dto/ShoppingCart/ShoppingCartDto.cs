using BookHaven.API.Models.Dto.CartItem;

namespace BookHaven.API.Models.Dto.ShoppingCart;

public class ShoppingCartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public int TotalQuantity => Items.Sum(item => item.Quantity);
    public decimal TotalPrice => Items.Sum(item => item.TotalPrice);
}
