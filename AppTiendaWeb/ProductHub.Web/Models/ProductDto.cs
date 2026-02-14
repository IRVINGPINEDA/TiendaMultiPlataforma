namespace ProductHub.Web.Models;

public class ProductDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int CantidadStock { get; set; }

    public string? Descripcion { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}
