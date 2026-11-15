using BookHaven.API.Models;
using static BookHaven.Utility.SD;

public class OrderHeader
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public decimal OrderTotal { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    public DateTime? ShippingDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }

    public string RecipientName { get; set; } = string.Empty; 
    public string PhoneNumber { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PaymentDate { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

