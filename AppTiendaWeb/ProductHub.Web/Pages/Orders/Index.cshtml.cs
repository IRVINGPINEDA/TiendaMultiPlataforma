using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Orders;

public class IndexModel : PageModel
{
    private static readonly IReadOnlyList<string> Statuses =
    [
        "Pendiente",
        "Confirmada",
        "Enviada",
        "Completada",
        "Cancelada"
    ];

    private readonly IProductsApiClient _apiClient;

    public IndexModel(IProductsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<OrderDto> Orders { get; private set; } = [];

    public IReadOnlyList<string> AvailableStatuses => Statuses;

    [BindProperty(SupportsGet = true)]
    public string? Estado { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var filter = NormalizeStatusOrNull(Estado);
        Estado = filter;
        Orders = await _apiClient.GetOrdersAsync(filter, cancellationToken);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string newEstado, string? estado = null, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeStatusOrNull(newEstado);
        if (normalized is null)
        {
            ErrorMessage = "Debes seleccionar un estado valido.";
            return RedirectToPage(new { estado });
        }

        try
        {
            var updated = await _apiClient.UpdateOrderStatusAsync(id, new UpdateOrderStatusRequest { Estado = normalized }, cancellationToken);
            if (updated is null)
            {
                ErrorMessage = "No se pudo actualizar la compra.";
            }
            else
            {
                SuccessMessage = $"Compra #{updated.Id} actualizada a '{updated.Estado}'.";
            }
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { estado });
    }

    private static string? NormalizeStatusOrNull(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Statuses.FirstOrDefault(s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
