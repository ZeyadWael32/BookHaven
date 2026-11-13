namespace BookHaven.API.Models;

public class ShoppingCart
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!; // Keycloak subject claim (sub)
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
