using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Dtos;

public class UpdateOrderStatusRequest
{
    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = string.Empty;
}
