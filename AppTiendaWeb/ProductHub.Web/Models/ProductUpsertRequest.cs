namespace ProductHub.Web.Models;

public class ProductUpsertRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int CantidadStock { get; set; }

    public string? Descripcion { get; set; }
}
