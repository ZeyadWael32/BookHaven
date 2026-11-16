using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.Order;

public class ShipOrderDto
{
    [Required, StringLength(50)]
    public string Carrier { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string TrackingNumber { get; set; } = string.Empty;
}