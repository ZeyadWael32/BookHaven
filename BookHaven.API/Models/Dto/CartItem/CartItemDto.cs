namespace BookHaven.API.Models.Dto.CartItem;

public class CartItemDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
