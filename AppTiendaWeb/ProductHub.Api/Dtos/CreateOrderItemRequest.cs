using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Dtos;

public class CreateOrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
