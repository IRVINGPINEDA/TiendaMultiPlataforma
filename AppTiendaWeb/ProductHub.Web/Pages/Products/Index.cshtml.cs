using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductsApiClient _apiClient;

    public IndexModel(IProductsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<ProductDto> Products { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IncludeInactive { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var products = await _apiClient.GetProductsAsync(IncludeInactive, cancellationToken);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            products = products
                .Where(p => p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            p.Marca.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Products = products
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, string? search, bool includeInactive, CancellationToken cancellationToken)
    {
        await _apiClient.DeleteProductAsync(id, cancellationToken);
        return RedirectToPage(new { search, includeInactive });
    }
}
