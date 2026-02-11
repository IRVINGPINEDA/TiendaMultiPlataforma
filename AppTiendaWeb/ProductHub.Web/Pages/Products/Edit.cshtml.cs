using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductHub.Web.Models;
using ProductHub.Web.Services;

namespace ProductHub.Web.Pages.Products;

public class EditModel : PageModel
{
    private readonly IProductsApiClient _apiClient;

    public EditModel(IProductsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public ProductInputModel Input { get; set; } = new();

    public Guid Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _apiClient.GetProductByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        Id = id;
        Input = new ProductInputModel
        {
            Nombre = product.Nombre,
            Marca = product.Marca,
            Precio = product.Precio,
            CantidadStock = product.CantidadStock,
            Descripcion = product.Descripcion
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        Id = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new ProductUpsertRequest
        {
            Nombre = Input.Nombre,
            Marca = Input.Marca,
            Precio = Input.Precio,
            CantidadStock = Input.CantidadStock,
            Descripcion = Input.Descripcion
        };

        var updated = await _apiClient.UpdateProductAsync(id, request, cancellationToken);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "No fue posible actualizar el producto.");
            return Page();
        }

        return RedirectToPage("Index");
    }
}
