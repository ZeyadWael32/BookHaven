using static BookHaven.Utility.SD;

namespace BookHaven.API.Models.Dto.Order;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal OrderTotal { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public DateTime? ShippingDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }

    public List<OrderDetailDto> Items { get; set; } = new List<OrderDetailDto>();

}
