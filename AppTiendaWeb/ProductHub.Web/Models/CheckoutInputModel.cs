using System.ComponentModel.DataAnnotations;

namespace ProductHub.Web.Models;

public class CheckoutInputModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(120)]
    public string ClienteNombre { get; set; } = string.Empty;

    [StringLength(40)]
    public string? ClienteTelefono { get; set; }

    [Required(ErrorMessage = "La direccion de entrega es obligatoria")]
    [StringLength(250)]
    public string DireccionEntrega { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notas { get; set; }
}
