using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Dto.Order;

public class OrderCreateDto
{
    [Required]
    [StringLength(100)]
    public string RecipientName { get; set; } = string.Empty;
    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string StreetAddress { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;
}
