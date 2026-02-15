namespace ProductHub.Infrastructure.Entities;

public class Order
{
    public Guid Id { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteTelefono { get; set; }

    public string DireccionEntrega { get; set; } = string.Empty;

    public string? Notas { get; set; }

    public string Estado { get; set; } = OrderStatus.Pendiente;

    public string Canal { get; set; } = "Web";

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}
