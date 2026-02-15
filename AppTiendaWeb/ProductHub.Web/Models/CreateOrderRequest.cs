namespace ProductHub.Web.Models;

public class CreateOrderRequest
{
    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteTelefono { get; set; }

    public string DireccionEntrega { get; set; } = string.Empty;

    public string? Notas { get; set; }

    public string? Canal { get; set; }

    public IReadOnlyList<CreateOrderItemRequest> Items { get; set; } = [];
}
