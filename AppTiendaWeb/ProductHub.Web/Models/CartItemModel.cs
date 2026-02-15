namespace ProductHub.Web.Models;

public class CartItemModel
{
    public Guid ProductId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }

    public int StockDisponible { get; set; }

    public decimal LineTotal => Precio * Quantity;
}
