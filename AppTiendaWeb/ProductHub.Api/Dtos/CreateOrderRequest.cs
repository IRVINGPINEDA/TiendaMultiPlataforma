using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Dtos;

public class CreateOrderRequest
{
    [Required]
    [StringLength(120)]
    public string ClienteNombre { get; set; } = string.Empty;

    [StringLength(40)]
    public string? ClienteTelefono { get; set; }

    [Required]
    [StringLength(250)]
    public string DireccionEntrega { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notas { get; set; }

    [StringLength(30)]
    public string? Canal { get; set; }

    [MinLength(1)]
    public IReadOnlyList<CreateOrderItemRequest> Items { get; set; } = [];
}
