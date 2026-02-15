namespace ProductHub.Api.Dtos;

public class OrderResponse
{
    public Guid Id { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteTelefono { get; set; }

    public string DireccionEntrega { get; set; } = string.Empty;

    public string? Notas { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Canal { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public IReadOnlyList<OrderItemResponse> Items { get; set; } = [];
}
