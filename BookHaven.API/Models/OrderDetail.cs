namespace BookHaven.API.Models;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderHeaderId { get; set; }
    public OrderHeader OrderHeader { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
