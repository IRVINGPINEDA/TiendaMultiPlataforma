using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Dtos;

public class UpdateProductRequest
{
    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue)]
    public int CantidadStock { get; set; }

    [StringLength(1000)]
    public string? Descripcion { get; set; }
}
